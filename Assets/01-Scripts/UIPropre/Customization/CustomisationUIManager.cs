using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CharacterCustomization;
using CharacterCustomizationNamespace = CharacterCustomization;
using System.Collections;
using Unity.Netcode;
using DG.Tweening;

/// <summary>
/// UI de personnalisation d’un personnage local, avec gestion des catégories SlotType et GroupType,
/// ainsi que les textures et couleurs. Basé sur SlotLibrary + CharacterCustomization.
/// </summary>
public class CustomisationUIManager : NetworkBehaviour
{
    #region ✨ Data & References
    [System.Serializable]
    public class TextureOption
    {
        public string name;
        public Texture2D texture;
        public Sprite preview;
    }
    [Header("🔧 Références")]
    [SerializeField] private SlotLibrary slotLibrary;
    private PlayerCustomizationData customizationData;

    [Header("🖌️ Thème")]
    [SerializeField] private TMP_Text themeReminderText;

    [Header("⏱️ Timer de Customisation")]
    [SerializeField] private GameObject customizationTimerPanel;
    [SerializeField] private Slider customizationSlider;
    [SerializeField] private TMP_Text timerText;

    [Header("🔊 Sons")]
    [SerializeField] private AudioClip countdownBeep;
    private AudioSource audioSource;

    [Header("⚖️ Catégories et onglets")]
    [SerializeField] private Transform categoryButtonContainer;
    [SerializeField] private GameObject categoryButtonPrefab;

    [SerializeField] private GameObject tabItemPanel;
    [SerializeField] private GameObject tabColorPanel;
    [SerializeField] private GameObject tabTexturePanel;
    [SerializeField] private Button tabItemButton;
    [SerializeField] private Button tabColorButton;
    [SerializeField] private Button tabTextureButton;

    [Header("🔹 Listes dynamiques")]
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject itemButtonPrefab;

    [Header("🖼️ Textures")]
    [SerializeField] private List<TextureOption> availableTextures;
    [SerializeField] private GameObject textureButtonPrefab;
    [SerializeField] private Transform textureButtonContainer;
    [SerializeField] private Sprite defaultTexturePreview;

    [Header("🔍 Filtrage par Tags")]
    [SerializeField] private TagFilterUI tagFilterUI; // Référence au composant TagFilterUI

    [Header("✨ Effet de Particules")]
    [SerializeField] private GameObject equipEffectPrefab; // Prefab du ParticleSystem

    private CustomizationData dataToSave;
    private Dictionary<(SlotType, GroupType?), List<Item>> categorizedItems;
    private Dictionary<GroupType, SlotType> redirectedGroups;
    private (SlotType, GroupType?) currentCategory;
    private Item currentSelectedItem;
    private List<string> selectedTags = new List<string>(); // Tags sélectionnés pour le filtrage

    private NetworkPlayer localPlayer;
    private CharacterCustomizationNamespace.CharacterCustomization character;
    private HashSet<SlotType> availableSlotTypes;

    private EquippedVisualsHandler visualsHandler;
    private Color _initialColor;
    private string _initialTextureName;

    #endregion

    #region Initialisation

    public static CustomisationUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        dataToSave = new CustomizationData
        {
            equippedItemIds = new Dictionary<SlotType, string>(),
            equippedColors = new Dictionary<SlotType, Color32>(),
            equippedTextures = new Dictionary<SlotType, string>()
        };
    }

    private void Start()
    {
        if (tagFilterUI != null && tagFilterUI.tagPanel != null)
        {
            tagFilterUI.tagPanel.SetActive(false);
        }

        StartCoroutine(WaitForLocalPlayerThenInit());
    }

    public void ForceInit()
    {
        StartCoroutine(WaitForLocalPlayerThenInit());
    }

    private IEnumerator WaitForLocalPlayerThenInit()
    {
        while (NetworkPlayerManager.Instance == null)
            yield return null;

        while (NetworkPlayerManager.Instance.LocalPlayerData == null)
        {
            yield return null;
        }

        customizationData = NetworkPlayerManager.Instance.LocalPlayerData;
        if (customizationData == null)
        {
            yield break;
        }

        GameObject characterBody = null;
        while (characterBody == null)
        {
            characterBody = NetworkPlayerManager.Instance.GetBodyRoot()?.gameObject;
            if (characterBody == null)
            {
                yield return null;
            }
        }

        if (slotLibrary == null)
        {
            yield break;
        }

        Transform bodyOrMesh = characterBody.transform.Find("Body")
                            ?? characterBody.GetComponentInChildren<SkinnedMeshRenderer>()?.transform
                            ?? characterBody.transform;

        character = new CharacterCustomizationNamespace.CharacterCustomization(bodyOrMesh.gameObject, slotLibrary);
        if (character == null || character.Slots == null)
        {
            yield break;
        }

        visualsHandler = NetworkPlayerManager.Instance.GetLocalVisuals();

        availableSlotTypes = character.Slots.Select(s => s.Type).ToHashSet();

        BuildRedirectMap();
        LoadItems();

        PopulateCategoryButtons();

        if (categorizedItems.Count > 0)
        {
            currentCategory = categorizedItems.Keys.First();
            PopulateItemList();
        }

        tabItemButton.onClick.AddListener(() => SelectTab(TabType.Item));
        tabColorButton.onClick.AddListener(() => SelectTab(TabType.Color));
        tabTextureButton.onClick.AddListener(() => SelectTab(TabType.Texture));

        InitializeTexturePanel();
    }

    private void BuildRedirectMap()
    {
        redirectedGroups = new();

        foreach (var entry in slotLibrary.Slots)
        {
            if (entry.Groups == null) continue;

            foreach (var group in entry.Groups)
            {
                if (!redirectedGroups.ContainsKey(group.Type))
                    redirectedGroups[group.Type] = entry.Type;
            }
        }
    }

    private void LoadItems()
    {
        categorizedItems = new();
        var allItems = Resources.LoadAll<Item>("Items");

        foreach (var item in allItems)
        {
            if (item == null || item.prefab == null)
            {
                continue;
            }

            var key = (item.category, null as GroupType?);

            foreach (var tag in item.tags)
            {
                if (System.Enum.TryParse(tag, out GroupType parsedGroup))
                {
                    key = (redirectedGroups.TryGetValue(parsedGroup, out var resolvedSlot) ? resolvedSlot : item.category, parsedGroup);
                    break;
                }
            }

            if (!categorizedItems.ContainsKey(key))
                categorizedItems[key] = new List<Item>();

            categorizedItems[key].Add(item);
        }
    }

    private void PopulateCategoryButtons()
    {
        foreach (var category in categorizedItems.Keys)
        {
            var btnObj = Instantiate(categoryButtonPrefab, categoryButtonContainer);
            var label = btnObj.GetComponentInChildren<TMP_Text>();
            label.text = category.Item2?.ToString() ?? category.Item1.ToString();
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnCategorySelected(category));
        }
    }

    #endregion

    #region Thème

    public void DisplayCurrentTheme()
    {
    }

    #endregion

    #region Timer de Customisation

    private float timerMax = 0f;
    private float timer = 0f;

    public void StartCustomizationTimer(float duration)
    {
        timerMax = duration;
        timer = duration;

        customizationSlider.maxValue = 1f;
        customizationSlider.value = 1f;

        customizationTimerPanel.SetActive(true);
        StartCoroutine(UpdateCustomizationTimer());
    }

    private IEnumerator UpdateCustomizationTimer()
    {
        Image fillImage = customizationSlider.fillRect.GetComponent<Image>();
        Color baseColor = Color.green;

        fillImage.color = baseColor;

        bool isPulsating = false;
        int lastSecond = Mathf.CeilToInt(timer);

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            customizationSlider.value = timer / timerMax;

            int seconds = Mathf.CeilToInt(timer);
            timerText.text = $"{seconds}s";

            if (timer < 10f)
            {
                fillImage.color = Color.Lerp(Color.red, baseColor, timer / 10f);

                if (!isPulsating)
                {
                    isPulsating = true;
                    PulsateSlider(fillImage.transform);
                }

                if (seconds < lastSecond)
                {
                    lastSecond = seconds;
                    PlayCountdownBeep();
                }
            }

            yield return null;
        }

        customizationSlider.value = 0;
        fillImage.transform.DOKill();
        fillImage.transform.localScale = Vector3.one;

        customizationTimerPanel.SetActive(false);
    }

    private void PulsateSlider(Transform target)
    {
        target.DOScale(1.1f, 0.5f)
              .SetLoops(-1, LoopType.Yoyo)
              .SetEase(Ease.InOutSine);
    }

    private void PlayCountdownBeep()
    {
        if (countdownBeep != null)
        {
            audioSource.pitch = Mathf.Lerp(1f, 1.5f, (10f - timer) / 10f);
            audioSource.PlayOneShot(countdownBeep);
            DOVirtual.DelayedCall(countdownBeep.length, () => audioSource.pitch = 1f);
        }
    }

    #endregion

    #region Onglets UI

    private enum TabType { Item, Color, Texture }

    private void SelectTab(TabType tab)
    {
        tabItemPanel?.SetActive(tab == TabType.Item);
        tabColorPanel?.SetActive(tab == TabType.Color);
        tabTexturePanel?.SetActive(tab == TabType.Texture);

        switch (tab)
        {
            case TabType.Item:
                PopulateItemList();
                break;
            case TabType.Color:
                OpenColorPicker();
                break;
            case TabType.Texture:
                OpenTexturePanel();
                break;
        }
    }

    #endregion

    #region Items et Catégories

    private void OnCategorySelected((SlotType, GroupType?) category)
    {
        currentCategory = category;
        if (tagFilterUI != null)
        {
            tagFilterUI.ClearFilters();
        }
        SelectTab(TabType.Item);
    }

    private void PopulateItemList()
    {
        ClearContainer(itemListContainer);

        if (!categorizedItems.ContainsKey(currentCategory))
        {
            return;
        }

        var items = categorizedItems[currentCategory];
        var filteredItems = items.Where(item =>
            selectedTags.Count == 0 || item.tags.Any(tag => selectedTags.Contains(tag))
        ).ToList();

        foreach (var item in filteredItems)
        {
            var btnObj = Instantiate(itemButtonPrefab, itemListContainer);
            btnObj.SetActive(true);

            // Configurer le texte du bouton
            var label = btnObj.GetComponentInChildren<TMP_Text>();
            if (label) label.text = item.itemName;

            // Configurer l'icône du bouton
            var buttonImage = btnObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = item.icon != null ? item.icon : null;
                buttonImage.enabled = item.icon != null;
            }
            else
            {
                var iconImage = btnObj.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = item.icon != null ? item.icon : null;
                    iconImage.enabled = item.icon != null;
                }
            }

            btnObj.GetComponent<Button>().onClick.AddListener(() => EquipItem(item));
        }
    }

    private void EquipItem(Item item)
    {
        currentSelectedItem = item;
        var slotType = currentCategory.Item1;

        var slot = character.Slots.FirstOrDefault(s => s.Type == slotType);
        if (slot == null) return;

        slot.SetPrefab(item.prefab);
        slot.Toggle(true);

        dataToSave.SetItem(slotType, item.itemId);
        if (customizationData.Data.TryGetColor(slotType, out var existingColor))
        {
            dataToSave.SetColor(slotType, existingColor);
        }
        if (customizationData.Data.TryGetTexture(slotType, out var existingTexture))
        {
            dataToSave.SetTexture(slotType, existingTexture);
        }

        if (!customizationData.IsSpawned || customizationData.NetworkObject == null)
        {
            return;
        }
        customizationData.SetItemAndApplyLocal(slotType, item.itemId, item);

        PlayEquipEffect(slotType);
    }

    private void PlayEquipEffect(SlotType slotType)
    {
        if (equipEffectPrefab == null || visualsHandler == null)
        {
            return;
        }

        var equippedObject = visualsHandler.GetEquippedObject(slotType);
        if (equippedObject == null)
        {
            return;
        }

        var renderer = equippedObject.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer == null)
        {
            return;
        }

        var effectInstance = Instantiate(equipEffectPrefab, equippedObject.transform);
        var bounds = renderer.bounds;
        effectInstance.transform.localPosition = bounds.center - equippedObject.transform.position;

        var particleSystem = effectInstance.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            var shape = particleSystem.shape;
            if (shape.shapeType == ParticleSystemShapeType.Sphere)
            {
                shape.radius = bounds.extents.magnitude * 0.5f;
            }

            particleSystem.Play();
            if (particleSystem.main.stopAction != ParticleSystemStopAction.Destroy)
            {
                Destroy(effectInstance, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
            }
        }
        else
        {
            Destroy(effectInstance, 5f);
        }
    }

    #endregion

    #region Filtrage par Tags

    public Dictionary<(SlotType, GroupType?), List<Item>> GetCategorizedItems()
    {
        return categorizedItems;
    }

    public void ApplyTagFilter(List<string> tags)
    {
        selectedTags = new List<string>(tags);
        PopulateItemList();
    }

    #endregion

    #region Couleurs avec ColorPicker

    private void OpenColorPicker()
    {
        if (currentSelectedItem == null || visualsHandler == null)
        {
            return;
        }

        var slotType = currentCategory.Item1;
        var equippedObject = visualsHandler.GetEquippedObject(slotType);
        if (equippedObject == null)
        {
            return;
        }

        var renderer = equippedObject.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer == null)
        {
            return;
        }

        _initialColor = renderer.material.color;
        _initialTextureName = customizationData.Data.TryGetTexture(slotType, out var textureName) ? textureName : null;

        if (tabTexturePanel != null) tabTexturePanel.SetActive(false);
        dataToSave.SetTexture(slotType, null);
        customizationData.Data = dataToSave;
        customizationData.SyncCustomizationDataServerRpc(dataToSave);

        Color currentColor = Color.white;
        if (customizationData.Data.TryGetColor(slotType, out var storedColor))
        {
            currentColor = storedColor;
        }

        visualsHandler.ApplyColorWithoutTexture(slotType, currentColor);

        bool success = ColorPicker.Create(
            original: currentColor,
            message: "Choisissez une couleur pour le vêtement",
            renderer: renderer,
            onColorChanged: (color) => OnColorChanged(slotType, color),
            onColorSelected: (color) => OnColorSelected(slotType, color),
            useAlpha: false
        );

        if (!success)
        {
            if (tabColorPanel != null) tabColorPanel.SetActive(false);
        }
        else
        {
            if (tabColorPanel != null) tabColorPanel.SetActive(true);
        }
    }

    private void OnColorChanged(SlotType slotType, Color color)
    {
        if (visualsHandler != null && currentSelectedItem != null)
        {
            visualsHandler.ApplyColorWithoutTexture(slotType, color);
            dataToSave.SetColor(slotType, color);
            dataToSave.SetTexture(slotType, null);
            customizationData.SyncCustomizationDataServerRpc(dataToSave);
        }
    }
        private void OnColorSelected(SlotType slotType, Color color)
        {
            if (visualsHandler != null && currentSelectedItem != null)
            {
                visualsHandler.ApplyColorWithoutTexture(slotType, color);
                dataToSave.SetColor(slotType, (Color32)color);
                dataToSave.SetTexture(slotType, null);
            }

            customizationData.SyncCustomizationDataServerRpc(dataToSave);

            if (tabColorPanel != null) tabColorPanel.SetActive(false);
        }

        #endregion

        #region Textures

        private void InitializeTexturePanel()
        {
            if (textureButtonContainer == null || textureButtonPrefab == null)
            {
                return;
            }

            ClearContainer(textureButtonContainer);

            for (int i = 0; i < availableTextures.Count; i++)
            {
                int index = i;
                TextureOption option = availableTextures[i];
                GameObject buttonObj = Instantiate(textureButtonPrefab, textureButtonContainer);
                buttonObj.SetActive(true);

                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.enabled = true;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ApplyTexture(index));
                }

                Image buttonImage = buttonObj.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.enabled = true;
                    buttonImage.sprite = option.preview != null ? option.preview : defaultTexturePreview;
                    buttonImage.preserveAspect = true;
                    buttonImage.color = Color.white;
                }

                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.enabled = true;
                    buttonText.text = option.name;
                }
            }

            if (tabTexturePanel != null) tabTexturePanel.SetActive(false);
        }

        private void OpenTexturePanel()
        {
            if (currentSelectedItem == null || tabTexturePanel == null || visualsHandler == null)
            {
                return;
            }

            var slotType = currentCategory.Item1;
            var equippedObject = visualsHandler.GetEquippedObject(slotType);
            if (equippedObject == null)
            {
                return;
            }

            var renderer = equippedObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer == null)
            {
                return;
            }

            _initialColor = renderer.material.color;
            _initialTextureName = customizationData.Data.TryGetTexture(slotType, out var textureName) ? textureName : null;

            if (tabColorPanel != null) tabColorPanel.SetActive(false);

            if (customizationData.Data.TryGetTexture(slotType, out textureName))
            {
                var textureOption = availableTextures.FirstOrDefault(t => t.name == textureName);
                if (textureOption != null && textureOption.texture != null)
                {
                    renderer.material.SetTexture("_BaseMap", textureOption.texture);
                    renderer.material.color = Color.white;
                }
            }

            tabTexturePanel.SetActive(true);
        }

        private void ApplyTexture(int textureIndex)
        {
            if (visualsHandler == null || currentSelectedItem == null)
            {
                return;
            }

            if (textureIndex < 0 || textureIndex >= availableTextures.Count)
            {
                return;
            }

            var slotType = currentCategory.Item1;
            var equippedObject = visualsHandler.GetEquippedObject(slotType);
            if (equippedObject == null)
            {
                return;
            }

            var renderer = equippedObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer == null)
            {
                return;
            }

            TextureOption option = availableTextures[textureIndex];

            // Vérifier le matériau
            if (renderer.material == null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }

            // Vérifier le shader
            if (renderer.material.shader.name != "Universal Render Pipeline/Lit")
            {
                Debug.LogWarning($"[CustomisationUI] Shader non compatible pour {slotType}: {renderer.material.shader.name}. Remplacement par URP/Lit.");
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }

            // Appliquer la texture directement
            if (option.texture != null)
            {
                renderer.material.SetTexture("_BaseMap", option.texture);
                renderer.material.color = Color.white;
            }
            else
            {
                renderer.material.SetTexture("_BaseMap", null);
                renderer.material.color = Color.white; // Réinitialiser la couleur même si la texture est null
            }

            dataToSave.SetTexture(slotType, option.name);
            dataToSave.SetColor(slotType, Color.white);
            customizationData.SyncCustomizationDataServerRpc(dataToSave);
        }

        #endregion

        #region Utilitaires

        public void CommitLocalCustomization()
        {
            if (customizationData == null) return;
            customizationData.SyncCustomizationDataServerRpc(dataToSave);
        }

        public void RefreshTenueGlobale()
        {
            customizationData.SyncCustomizationDataServerRpc(dataToSave);
        }

        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
                Destroy(child.gameObject);
        }

        public void ResetToInitial()
        {
            if (currentSelectedItem == null || visualsHandler == null) return;

            var slotType = currentCategory.Item1;
            var equippedObject = visualsHandler.GetEquippedObject(slotType);
            if (equippedObject == null) return;

            var renderer = equippedObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer == null) return;

            if (_initialTextureName != null)
            {
                var textureOption = availableTextures.FirstOrDefault(t => t.name == _initialTextureName);
                if (textureOption != null && textureOption.texture != null)
                {
                    renderer.material.SetTexture("_BaseMap", textureOption.texture);
                    renderer.material.color = Color.white;
                }
                dataToSave.SetTexture(slotType, _initialTextureName);
                dataToSave.SetColor(slotType, _initialColor);
            }
            else
            {
                visualsHandler.ApplyColorWithoutTexture(slotType, _initialColor);
                dataToSave.SetColor(slotType, _initialColor);
                dataToSave.SetTexture(slotType, null);
            }

            customizationData.Data = dataToSave;
            customizationData.SyncCustomizationDataServerRpc(dataToSave);

            if (tabColorPanel != null) tabColorPanel.SetActive(false);
            if (tabTexturePanel != null) tabTexturePanel.SetActive(false);
            SelectTab(TabType.Item);
        }
    }

#endregion