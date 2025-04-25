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
    [SerializeField] private GameObject tabColorPanel; // Panneau pour le ColorPicker
    [SerializeField] private Button tabItemButton;
    [SerializeField] private Button tabColorButton;

    [Header("🔹 Listes dynamiques")]
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject itemButtonPrefab;

    private CustomizationData dataToSave;
    private Dictionary<(SlotType, GroupType?), List<Item>> categorizedItems;
    private Dictionary<GroupType, SlotType> redirectedGroups;
    private (SlotType, GroupType?) currentCategory;
    private Item currentSelectedItem;

    private NetworkPlayer localPlayer;
    private CharacterCustomizationNamespace.CharacterCustomization character;
    private HashSet<SlotType> availableSlotTypes;

    private EquippedVisualsHandler visualsHandler;

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
        // var theme = ThemeManager.Instance.CurrentTheme;
        // if (theme != null)
        // {
        //     themeReminderText.text = theme.themeName;
        // }
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

    private enum TabType { Item, Color }

    private void SelectTab(TabType tab)
    {
        tabItemPanel?.SetActive(tab == TabType.Item);
        tabColorPanel?.SetActive(tab == TabType.Color);

        switch (tab)
        {
            case TabType.Item:
                PopulateItemList();
                break;
            case TabType.Color:
                OpenColorPicker();
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
        // Conserver la couleur existante si elle existe
        if (customizationData.Data.TryGetColor(slotType, out var existingColor))
        {
            dataToSave.SetColor(slotType, existingColor);
            Debug.Log($"[CustomisationUI] Couleur conservée pour {slotType}: {ColorUtility.ToHtmlStringRGBA(existingColor)}");
        }

        if (!customizationData.IsSpawned || customizationData.NetworkObject == null)
        {
            Debug.LogWarning("[CustomisationUI] ❌ Impossible d’envoyer un ServerRpc car le NetworkObject n’est pas prêt.");
            return;
        }
        customizationData.SetItemAndApplyLocal(slotType, item.itemId, item);
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

        if (tabColorPanel == null)
        {
            Debug.LogWarning("[CustomisationUI] TabColorPanel non assigné.");
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

        Color currentColor = Color.white;
        if (customizationData.Data.TryGetColor(slotType, out var storedColor))
        {
            currentColor = storedColor;
            Debug.Log($"[CustomisationUI] Couleur chargée pour {slotType}: {ColorUtility.ToHtmlStringRGBA(currentColor)}");
        }

        visualsHandler.ApplyColorWithoutTexture(slotType, currentColor);

        tabColorPanel.SetActive(true);
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
            tabColorPanel.SetActive(false);
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

        if (tabColorPanel) tabColorPanel.SetActive(false);
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

    #endregion
}