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

    [Header("✨ Effet de Particules")]
    [SerializeField] private GameObject equipEffectPrefab; // Prefab du ParticleSystem

    private CustomizationData dataToSave;
    private Dictionary<(SlotType, GroupType?), List<Item>> categorizedItems;
    private Dictionary<GroupType, SlotType> redirectedGroups;
    private (SlotType, GroupType?) currentCategory;
    private Item currentSelectedItem;

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
        Debug.Log("[CustomisationUI] Start appelé !");
        StartCoroutine(WaitForLocalPlayerThenInit());
    }

    public void ForceInit()
    {
        StartCoroutine(WaitForLocalPlayerThenInit());
    }

    private IEnumerator WaitForLocalPlayerThenInit()
    {
        Debug.Log("[CustomisationUI] ⏳ Attente du NetworkPlayer...");

        while (NetworkPlayerManager.Instance == null)
            yield return null;

        Debug.Log("[CustomisationUI] ✅ NetworkPlayer.Instance trouvé.");

        while (NetworkPlayerManager.Instance.LocalPlayerData == null)
        {
            yield return null;
        }

        Debug.Log("[CustomisationUI] ✅ Joueur local prêt.");

        customizationData = NetworkPlayerManager.Instance.LocalPlayerData;
        if (customizationData == null)
        {
            Debug.LogError("[CustomisationUI] ❌ PlayerCustomizationData introuvable !");
            yield break;
        }

        GameObject characterBody = null;
        while (characterBody == null)
        {
            characterBody = NetworkPlayerManager.Instance.GetBodyRoot()?.gameObject;
            if (characterBody == null)
            {
                Debug.Log("[CustomisationUI] ⏳ En attente du corps du joueur...");
                yield return null;
            }
        }
        Debug.Log("[CustomisationUI] ✅ Corps du joueur trouvé : " + characterBody.name);

        if (slotLibrary == null)
        {
            Debug.LogError("[CustomisationUI] ❌ slotLibrary non assigné dans l’inspecteur !");
            yield break;
        }

        Transform bodyOrMesh = characterBody.transform.Find("Body")
                            ?? characterBody.GetComponentInChildren<SkinnedMeshRenderer>()?.transform
                            ?? characterBody.transform;

        character = new CharacterCustomizationNamespace.CharacterCustomization(bodyOrMesh.gameObject, slotLibrary);
        if (character == null || character.Slots == null)
        {
            Debug.LogError("[CustomisationUI] ❌ character ou Slots est null !");
            yield break;
        }

        Debug.Log($"[CustomisationUI] ✅ CharacterCustomization créée avec {character.Slots.Length} slot(s).");

        visualsHandler = NetworkPlayerManager.Instance.GetLocalVisuals();
        if (visualsHandler == null)
            Debug.LogWarning("[CustomisationUI] ⚠️ Aucun EquippedVisualsHandler trouvé sur le joueur.");

        availableSlotTypes = character.Slots.Select(s => s.Type).ToHashSet();
        Debug.Log($"[CustomisationUI] ✅ SlotTypes détectés : {availableSlotTypes.Count}");

        try
        {
            BuildRedirectMap();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[CustomisationUI] ❌ Exception dans BuildRedirectMap : {ex.Message}\n{ex.StackTrace}");
        }

        try
        {
            LoadItems();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[CustomisationUI] ❌ Exception dans LoadItems : {ex.Message}\n{ex.StackTrace}");
        }

        Debug.Log($"[CustomisationUI] ✅ {categorizedItems.Count} catégories chargées depuis Resources/Items");

        PopulateCategoryButtons();

        if (categorizedItems.Count > 0)
        {
            currentCategory = categorizedItems.Keys.First();
            PopulateItemList();
        }
        else
        {
            Debug.LogWarning("[CustomisationUI] ⚠️ Aucun item trouvé — vérifie Resources/Items.");
        }

        tabItemButton.onClick.AddListener(() => SelectTab(TabType.Item));
        tabColorButton.onClick.AddListener(() => SelectTab(TabType.Color));
        tabTextureButton.onClick.AddListener(() => SelectTab(TabType.Texture));

        InitializeTexturePanel();
        DebugTextureLoading(); // Tester les textures au démarrage

        Debug.Log("[CustomisationUI] ✅ Initialisation complète du CustomisationUIManager.");
    }

    private void BuildRedirectMap()
    {
        redirectedGroups = new();

        foreach (var entry in slotLibrary.Slots)
        {
            Debug.Log($"[CustomisationUI] SlotEntry : {entry.Type}, Groups : {entry.Groups?.Length}");

            if (entry.Groups == null) continue;

            foreach (var group in entry.Groups)
            {
                Debug.Log($"[CustomisationUI]   ↳ Redirige {group.Type} vers {entry.Type}");
                if (!redirectedGroups.ContainsKey(group.Type))
                    redirectedGroups[group.Type] = entry.Type;
            }
        }
    }

    private void LoadItems()
    {
        Debug.Log("[CustomisationUI] Chargement des items depuis Resources/Items...");
        categorizedItems = new();
        var allItems = Resources.LoadAll<Item>("Items");
        Debug.Log($"[CustomisationUI] → categorizedItems.Count = {categorizedItems.Count}");

        foreach (var item in allItems)
        {
            if (item == null || item.prefab == null)
            {
                Debug.LogWarning($"[CustomisationUI] Item invalide ou prefab manquant : {item?.name}");
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
        Debug.Log($"[CustomisationUI] 📌 PopulateCategoryButtons() appelé");

        foreach (var kvp in categorizedItems)
        {
            Debug.Log($"[CustomisationUI] Catégorie ajoutée : SlotType = {kvp.Key.Item1}, GroupType = {kvp.Key.Item2}, {kvp.Value.Count} item(s)");
        }

        foreach (var category in categorizedItems.Keys)
        {
            var btnObj = Instantiate(categoryButtonPrefab, categoryButtonContainer);
            var label = btnObj.GetComponentInChildren<TMP_Text>();
            label.text = category.Item2?.ToString() ?? category.Item1.ToString();
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnCategorySelected(category));
        }
        Debug.Log($"[CustomisationUI] Génération des boutons de catégories : {categorizedItems.Count}");
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
        Debug.Log($"[CustomisationUI] Catégorie sélectionnée : {category}");
        SelectTab(TabType.Item);
    }

    private void PopulateItemList()
    {
        ClearContainer(itemListContainer);

        if (!categorizedItems.ContainsKey(currentCategory))
        {
            Debug.LogWarning($"[CustomisationUI] Aucune entrée pour {currentCategory}");
            return;
        }

        var items = categorizedItems[currentCategory];
        Debug.Log($"[CustomisationUI] Affichage de {items.Count} items pour {currentCategory}");

        foreach (var item in items)
        {
            var btnObj = Instantiate(itemButtonPrefab, itemListContainer);
            btnObj.SetActive(true);
            var label = btnObj.GetComponentInChildren<TMP_Text>();
            if (label) label.text = item.itemName;
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
            Debug.Log($"[CustomisationUI] Couleur conservée pour {slotType}: {ColorUtility.ToHtmlStringRGBA(existingColor)}");
        }
        if (customizationData.Data.TryGetTexture(slotType, out var existingTexture))
        {
            dataToSave.SetTexture(slotType, existingTexture);
            Debug.Log($"[CustomisationUI] Texture conservée pour {slotType}: {existingTexture}");
        }

        if (!customizationData.IsSpawned || customizationData.NetworkObject == null)
        {
            Debug.LogWarning("[CustomisationUI] ❌ Impossible d’envoyer un ServerRpc car le NetworkObject n’est pas prêt.");
            return;
        }
        customizationData.SetItemAndApplyLocal(slotType, item.itemId, item);

        // Jouer l'effet de particules
        PlayEquipEffect(slotType);
    }

    private void PlayEquipEffect(SlotType slotType)
    {
        if (equipEffectPrefab == null)
        {
            Debug.LogWarning("[CustomisationUI] ❌ Prefab d'effet de particules non assigné dans l’inspecteur.");
            return;
        }

        if (visualsHandler == null)
        {
            Debug.LogWarning("[CustomisationUI] ❌ EquippedVisualsHandler non trouvé.");
            return;
        }

        var equippedObject = visualsHandler.GetEquippedObject(slotType);
        if (equippedObject == null)
        {
            Debug.LogWarning($"[CustomisationUI] ❌ Aucun vêtement équipé pour le slot {slotType}.");
            return;
        }

        var renderer = equippedObject.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"[CustomisationUI] ❌ Aucun SkinnedMeshRenderer trouvé pour le vêtement dans le slot {slotType}.");
            return;
        }

        // Instancier l’effet de particules
        var effectInstance = Instantiate(equipEffectPrefab, equippedObject.transform);
        Debug.Log($"[CustomisationUI] ✨ Effet de particules instancié pour {slotType}: {effectInstance.name}");

        // Positionner au centre du vêtement
        var bounds = renderer.bounds;
        effectInstance.transform.localPosition = bounds.center - equippedObject.transform.position;
        Debug.Log($"[CustomisationUI] Position de l’effet: {effectInstance.transform.position} (Centre des bounds: {bounds.center})");

        // Obtenir le ParticleSystem
        var particleSystem = effectInstance.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            // Ajuster la forme pour entourer le vêtement
            var shape = particleSystem.shape;
            if (shape.shapeType == ParticleSystemShapeType.Sphere)
            {
                shape.radius = bounds.extents.magnitude * 0.5f; // Ajuster au rayon du vêtement
                Debug.Log($"[CustomisationUI] Rayon de l’effet ajusté: {shape.radius}");
            }

            // S’assurer que l’effet se joue
            particleSystem.Play();
            Debug.Log($"[CustomisationUI] Effet de particules joué pour {slotType}. Durée: {particleSystem.main.duration}s");

            // Détruire après la durée si Stop Action n’est pas Destroy
            if (particleSystem.main.stopAction != ParticleSystemStopAction.Destroy)
            {
                Destroy(effectInstance, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
                Debug.Log($"[CustomisationUI] Destruction planifiée de l’effet après {particleSystem.main.duration + particleSystem.main.startLifetime.constantMax}s");
            }
        }
        else
        {
            Debug.LogWarning($"[CustomisationUI] ❌ Aucun ParticleSystem trouvé sur {effectInstance.name}.");
            Destroy(effectInstance, 5f); // Destruction par défaut après 5s
        }
    }

    #endregion

    #region Couleurs avec ColorPicker

    private void OpenColorPicker()
    {
        if (currentSelectedItem == null)
        {
            Debug.LogWarning("[CustomisationUI] Aucun item sélectionné pour la couleur.");
            return;
        }

        if (visualsHandler == null)
        {
            Debug.LogWarning("[CustomisationUI] EquippedVisualsHandler non trouvé.");
            return;
        }

        var slotType = currentCategory.Item1;
        var equippedObject = visualsHandler.GetEquippedObject(slotType);
        if (equippedObject == null)
        {
            Debug.LogWarning($"[CustomisationUI] Aucun vêtement équipé pour le slot {slotType}.");
            return;
        }

        var renderer = equippedObject.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"[CustomisationUI] Aucun SkinnedMeshRenderer trouvé pour le vêtement dans le slot {slotType}.");
            return;
        }

        // Sauvegarder l'état initial
        _initialColor = renderer.material.color;
        _initialTextureName = customizationData.Data.TryGetTexture(slotType, out var textureName) ? textureName : null;
        Debug.Log($"[CustomisationUI] État initial sauvegardé pour {slotType}: Couleur={ColorUtility.ToHtmlStringRGBA(_initialColor)}, Texture={_initialTextureName ?? "Aucune"}");

        // Désactiver le panneau de textures et réinitialiser la texture
        if (tabTexturePanel != null) tabTexturePanel.SetActive(false);
        dataToSave.SetTexture(slotType, null);
        customizationData.Data = dataToSave;
        customizationData.SyncCustomizationDataServerRpc(dataToSave);

        // Charger la couleur actuelle ou par défaut
        Color currentColor = Color.white;
        if (customizationData.Data.TryGetColor(slotType, out var storedColor))
        {
            currentColor = storedColor;
            Debug.Log($"[CustomisationUI] Couleur chargée pour {slotType}: {ColorUtility.ToHtmlStringRGBA(currentColor)}");
        }

        visualsHandler.ApplyColorWithoutTexture(slotType, currentColor);

        // Créer le ColorPicker
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
            Debug.LogWarning("[CustomisationUI] Échec de l'ouverture du ColorPicker.");
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

            if (dataToSave.TryGetColor(slotType, out var confirmColor))
            {
                Debug.Log($"[UI → Confirm] Couleur bien stockée pour {slotType} = {ColorUtility.ToHtmlStringRGBA(confirmColor)}");
            }
            else
            {
                Debug.LogWarning($"[UI → ERROR] La couleur n'a PAS été stockée dans dataToSave !");
            }

            dataToSave.SetTexture(slotType, null);
            Debug.Log($"[CustomisationUI] Couleur temporaire enregistrée pour {slotType}: {ColorUtility.ToHtmlStringRGBA(color)}");

            // ✅ Synchronisation immédiate même pendant le glissement
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
            Debug.Log($"[CustomisationUI] Couleur finale enregistrée pour {slotType}: {ColorUtility.ToHtmlStringRGBA(color)}");
        }

        // ❗ Correction ici : plus de assignation directe
        customizationData.SyncCustomizationDataServerRpc(dataToSave);

        foreach (var kvp in dataToSave.equippedColors)
        {
            Debug.Log($"[UI → Envoi] Couleur envoyée pour {kvp.Key} = {ColorUtility.ToHtmlStringRGBA(kvp.Value)}");
        }

        if (tabColorPanel != null) tabColorPanel.SetActive(false);
    }

    #endregion

    #region Textures

    private void InitializeTexturePanel()
    {
        if (textureButtonContainer == null || textureButtonPrefab == null)
        {
            Debug.LogWarning("[CustomisationUI] textureButtonContainer ou textureButtonPrefab non assigné.");
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
            else
            {
                Debug.LogWarning($"[CustomisationUI] Bouton manquant sur textureButtonPrefab pour {option.name}.");
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
            else
            {
                Debug.LogWarning($"[CustomisationUI] TextMeshProUGUI manquant sur le bouton de texture {option.name}.");
            }
        }

        if (tabTexturePanel != null) tabTexturePanel.SetActive(false);
    }

    private void OpenTexturePanel()
    {
        if (currentSelectedItem == null)
        {
            Debug.LogWarning("[CustomisationUI] Aucun item sélectionné pour la texture.");
            return;
        }

        if (tabTexturePanel == null)
        {
            Debug.LogWarning("[CustomisationUI] TabTexturePanel non assigné.");
            return;
        }

        if (visualsHandler == null)
        {
            Debug.LogWarning("[CustomisationUI] EquippedVisualsHandler non trouvé.");
            return;
        }

        var slotType = currentCategory.Item1;
        var equippedObject = visualsHandler.GetEquippedObject(slotType);
        if (equippedObject == null)
        {
            Debug.LogWarning($"[CustomisationUI] Aucun vêtement équipé pour le slot {slotType}.");
            return;
        }

        var renderer = equippedObject.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"[CustomisationUI] Aucun SkinnedMeshRenderer trouvé pour le vêtement dans le slot {slotType}.");
            return;
        }

        // Sauvegarder l'état initial
        _initialColor = renderer.material.color;
        _initialTextureName = customizationData.Data.TryGetTexture(slotType, out var textureName) ? textureName : null;
        Debug.Log($"[CustomisationUI] État initial sauvegardé pour {slotType}: Couleur={ColorUtility.ToHtmlStringRGBA(_initialColor)}, Texture={_initialTextureName ?? "Aucune"}");

        // Désactiver le panneau de couleurs
        if (tabColorPanel != null) tabColorPanel.SetActive(false);

        // Appliquer la texture existante si disponible
        if (customizationData.Data.TryGetTexture(slotType, out textureName))
        {
            var textureOption = availableTextures.FirstOrDefault(t => t.name == textureName);
            if (textureOption != null && textureOption.texture != null)
            {
                renderer.material.SetTexture("_BaseMap", textureOption.texture);
                renderer.material.color = Color.white; // Réinitialiser la couleur pour la texture
                Debug.Log($"[CustomisationUI] Texture existante {textureName} appliquée avec couleur réinitialisée pour {slotType}.");
            }
            else
            {
                Debug.LogWarning($"[CustomisationUI] Texture {textureName} introuvable dans availableTextures ou texture non assignée pour {slotType}.");
            }
        }

        tabTexturePanel.SetActive(true);
    }

    private void ApplyTexture(int textureIndex)
    {
        if (visualsHandler == null)
        {
            Debug.LogWarning("[CustomisationUI] EquippedVisualsHandler non trouvé.");
            return;
        }

        if (currentSelectedItem == null)
        {
            Debug.LogWarning("[CustomisationUI] Aucun item sélectionné pour la texture.");
            return;
        }

        if (textureIndex < 0 || textureIndex >= availableTextures.Count)
        {
            Debug.LogWarning($"[CustomisationUI] Index de texture invalide : {textureIndex}.");
            return;
        }

        var slotType = currentCategory.Item1;
        var equippedObject = visualsHandler.GetEquippedObject(slotType);
        if (equippedObject == null)
        {
            Debug.LogWarning($"[CustomisationUI] Aucun vêtement équipé pour le slot {slotType}.");
            return;
        }

        var renderer = equippedObject.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"[CustomisationUI] Aucun SkinnedMeshRenderer trouvé pour le vêtement dans le slot {slotType}.");
            return;
        }

        TextureOption option = availableTextures[textureIndex];
        Debug.Log($"[CustomisationUI] Tentative d'application de la texture {option.name} pour {slotType}");

        // Vérifier le matériau
        if (renderer.material == null)
        {
            Debug.LogWarning($"[CustomisationUI] Le matériau du renderer est null pour {slotType}. Création d'un nouveau matériau par défaut.");
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
            renderer.material.color = Color.white; // Réinitialiser la couleur pour utiliser la _BaseMap de base
            Debug.Log($"[CustomisationUI] Texture {option.name} appliquée avec couleur réinitialisée (Color.white) pour {slotType}.");
        }
        else
        {
            Debug.LogWarning($"[CustomisationUI] La texture {option.name} est null dans availableTextures pour {slotType}. Vérifiez l'assignation dans l'inspecteur.");
            renderer.material.SetTexture("_BaseMap", null);
            renderer.material.color = Color.white; // Réinitialiser la couleur même si la texture est null
        }

        // Enregistrer la texture et réinitialiser la couleur
        dataToSave.SetTexture(slotType, option.name);
        dataToSave.SetColor(slotType, Color.white);
        Debug.Log($"[CustomisationUI] Texture enregistrée pour {slotType}: {option.name}, Couleur réinitialisée: {ColorUtility.ToHtmlStringRGBA(Color.white)}");

        customizationData.Data = dataToSave;
        customizationData.SyncCustomizationDataServerRpc(dataToSave);
    }


    #endregion

    #region Utilitaires

    public void CommitLocalCustomization()
    {
        if (customizationData == null) return;

        Debug.Log("[CustomisationUI] ✅ Commit de la tenue locale via ServerRpc.");
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

        // Restaurer l'état initial
        if (_initialTextureName != null)
        {
            var textureOption = availableTextures.FirstOrDefault(t => t.name == _initialTextureName);
            if (textureOption != null && textureOption.texture != null)
            {
                renderer.material.SetTexture("_BaseMap", textureOption.texture);
                renderer.material.color = Color.white; // Réinitialiser la couleur pour la texture
                Debug.Log($"[CustomisationUI] Texture initiale {_initialTextureName} restaurée avec couleur réinitialisée pour {slotType}.");
            }
            else
            {
                Debug.LogWarning($"[CustomisationUI] Texture initiale {_initialTextureName} introuvable ou non assignée pour {slotType}.");
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
        Debug.Log($"[CustomisationUI] État initial restauré pour {slotType}: Couleur={ColorUtility.ToHtmlStringRGBA(_initialColor)}, Texture={_initialTextureName ?? "Aucune"}");

        if (tabColorPanel != null) tabColorPanel.SetActive(false);
        if (tabTexturePanel != null) tabTexturePanel.SetActive(false);
        SelectTab(TabType.Item);
    }

    // Méthode de débogage pour tester le chargement des textures
    public void DebugTextureLoading()
    {
        Debug.Log("[CustomisationUI] Début du test de chargement des textures...");
        foreach (var option in availableTextures)
        {
            Debug.Log($"[CustomisationUI] Vérification de la texture {option.name}:");
            if (option.texture != null)
            {
                Debug.Log($"  - Texture assignée dans l'inspecteur: {option.texture.name}");
            }
            else
            {
                Debug.LogWarning($"  - Texture NON assignée dans l'inspecteur pour {option.name}. Assignez la Texture2D dans l'inspecteur.");
            }

            var loadedTexture = Resources.Load<Texture2D>($"Textures/{option.name}");
            if (loadedTexture != null)
            {
                Debug.Log($"  - Texture chargée via Resources: Textures/{option.name}");
            }
            else
            {
                Debug.LogWarning($"  - Échec du chargement via Resources: Textures/{option.name}. Vérifiez le fichier dans Assets/Resources/Textures.");
            }
        }
    }

    #endregion
}