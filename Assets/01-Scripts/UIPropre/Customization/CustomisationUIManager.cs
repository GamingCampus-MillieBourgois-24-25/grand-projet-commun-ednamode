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

    [Header("🔊 Sounds")]
    [SerializeField] private AudioClip countdownBeep;
    private AudioSource audioSource;

    [Header("⚖️ Catégories et onglets")]
    [SerializeField] private Transform categoryButtonContainer;
    [SerializeField] private GameObject categoryButtonPrefab;

    [SerializeField] private GameObject tabItemPanel;
    [SerializeField] private GameObject tabTexturePanel;
    [SerializeField] private GameObject tabColorPanel;

    [SerializeField] private Button tabItemButton;
    [SerializeField] private Button tabTextureButton;
    [SerializeField] private Button tabColorButton;

    [Header("🔹 Listes dynamiques")]
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Transform textureListContainer;
    [SerializeField] private GameObject textureButtonPrefab;
    [SerializeField] private Transform colorListContainer;
    [SerializeField] private GameObject colorButtonPrefab;

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

    #region 🚀 Initialisation

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
    }

    /// <summary>
    /// Démarre le système de customisation : instancie le personnage, charge les items, et construit l’UI
    /// </summary>
    private void Start()
    {
        Debug.Log("[CustomisationUI] Start appelé !");
        themeReminderText.text = $"Theme: {ThemeManager.Instance.CurrentTheme.themeName}";
        StartCoroutine(WaitForLocalPlayerThenInit());
    }

    /// <summary>
    /// Force l'initialisation du système de customisation, utile pour les tests
    /// </summary>
    public void ForceInit()
    {
        StartCoroutine(WaitForLocalPlayerThenInit());
    }

    /// <summary>
    /// Attends que le NetworkPlayer local soit prêt avant de lancer l'initialisation
    /// </summary>
    /// <summary>
    /// Coroutine d'initialisation du système de customisation une fois le joueur local prêt.
    /// </summary>
    private IEnumerator WaitForLocalPlayerThenInit()
    {
        Debug.Log("[CustomisationUI] ⏳ Attente du NetworkPlayer...");

        // 🔁 Attente de l'instance du manager NetworkPlayer dans la scène
        while (NetworkPlayerManager.Instance == null)
            yield return null;

        Debug.Log("[CustomisationUI] ✅ NetworkPlayer.Instance trouvé.");

        // 🔁 Attente que le joueur local (NetworkObject + PlayerCustomizationData) soit dispo
        while (NetworkPlayerManager.Instance.LocalPlayerData == null)
        {
            //Debug.Log("[CustomisationUI] 🔁 En attente de LocalPlayerData (joueur local)...");
            yield return null;
        }

        Debug.Log("[CustomisationUI] ✅ Joueur local prêt.");

        // 📦 Récupération du PlayerCustomizationData
        customizationData = NetworkPlayerManager.Instance.LocalPlayerData;
        if (customizationData == null)
        {
            Debug.LogError("[CustomisationUI] ❌ PlayerCustomizationData introuvable !");
            yield break;
        }

        // 🔁 Attente que le body (visuel joueur) soit prêt
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

        // 📚 Vérification du slotLibrary
        if (slotLibrary == null)
        {
            Debug.LogError("[CustomisationUI] ❌ slotLibrary non assigné dans l’inspecteur !");
            yield break;
        }

        // 🧠 Création de la logique de customisation
        Transform bodyOrMesh = characterBody.transform.Find("Body")
                            ?? characterBody.GetComponentInChildren<SkinnedMeshRenderer>()?.transform
                            ?? characterBody.transform;

        character = new CharacterCustomization.CharacterCustomization(bodyOrMesh.gameObject, slotLibrary);
        if (character == null || character.Slots == null)
        {
            Debug.LogError("[CustomisationUI] ❌ character ou Slots est null !");
            yield break;
        }

        Debug.Log($"[CustomisationUI] ✅ CharacterCustomization créée avec {character.Slots.Length} slot(s).");

        // 🎨 Récupération du visuel équipé
        visualsHandler = NetworkPlayerManager.Instance.GetLocalVisuals();
        if (visualsHandler == null)
            Debug.LogWarning("[CustomisationUI] ⚠️ Aucun EquippedVisualsHandler trouvé sur le joueur.");

        // 📦 Détection des SlotTypes disponibles
        availableSlotTypes = character.Slots.Select(s => s.Type).ToHashSet();
        Debug.Log($"[CustomisationUI] ✅ SlotTypes détectés : {availableSlotTypes.Count}");

        // 🔄 Mapping des GroupType → SlotType
        try
        {
            BuildRedirectMap();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[CustomisationUI] ❌ Exception dans BuildRedirectMap : {ex.Message}\n{ex.StackTrace}");
        }

        // 📦 Chargement des items
        try
        {
            LoadItems();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[CustomisationUI] ❌ Exception dans LoadItems : {ex.Message}\n{ex.StackTrace}");
        }

        Debug.Log($"[CustomisationUI] ✅ {categorizedItems.Count} catégories chargées depuis Resources/Items");

        // 🧭 Génération des boutons de catégories
        PopulateCategoryButtons();

        // 🔘 Affichage initial si au moins une catégorie existe
        if (categorizedItems.Count > 0)
        {
            currentCategory = categorizedItems.Keys.First();
            PopulateItemList();
        }
        else
        {
            Debug.LogWarning("[CustomisationUI] ⚠️ Aucun item trouvé — vérifie Resources/Items.");
        }

        // 📌 Bind des boutons UI
        tabItemButton.onClick.AddListener(() => SelectTab(TabType.Item));
        tabTextureButton.onClick.AddListener(() => SelectTab(TabType.Texture));
        tabColorButton.onClick.AddListener(() => SelectTab(TabType.Color));

        Debug.Log("[CustomisationUI] ✅ Initialisation complète du CustomisationUIManager.");
    }

    /// <summary>
    /// Construit une map de redirection entre les GroupTypes et leurs SlotType parent.
    /// </summary>
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

    /// <summary>
    /// Charge tous les items depuis Resources/Items et les catégorise par SlotType + GroupType
    /// </summary>
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

    /// <summary>
    /// Instancie un bouton par catégorie (SlotType ou GroupType) en bas de l’écran
    /// </summary>
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

    #region 🧭 Theme 

    /// <summary>
    /// Affiche le nom et l'icône du thème actuel
    /// </summary>
    public void DisplayCurrentTheme()
    {
        var theme = ThemeManager.Instance.CurrentTheme;
        if (theme != null)
        {
            themeReminderText.text = theme.themeName;
        }
    }

    #endregion

    #region ⏱️ Timer de Customisation

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

            // 🎨 Color shift + Pulsation
            if (timer < 10f)
            {
                fillImage.color = Color.Lerp(Color.red, baseColor, timer / 10f);

                if (!isPulsating)
                {
                    isPulsating = true;
                    PulsateSlider(fillImage.transform);
                }

                // 🔊 Play beep once per second
                if (seconds < lastSecond)
                {
                    lastSecond = seconds;
                    PlayCountdownBeep();
                }
            }

            yield return null;
        }

        // Fin du timer
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
            // 🎵 Change le pitch en fonction du temps restant
            audioSource.pitch = Mathf.Lerp(1f, 1.5f, (10f - timer) / 10f);
            audioSource.PlayOneShot(countdownBeep);

            // Remettre le pitch à 1 après le son pour éviter d'impacter d'autres sons
            DOVirtual.DelayedCall(countdownBeep.length, () => audioSource.pitch = 1f);
        }
    }

    #endregion

    #region 📅 Onglets UI

    private enum TabType { Item, Texture, Color }

    /// <summary>
    /// Affiche le bon panneau de droite (Items, Textures, Couleurs)
    /// </summary>
    private void SelectTab(TabType tab)
    {
        tabItemPanel?.SetActive(tab == TabType.Item);
        tabTexturePanel?.SetActive(tab == TabType.Texture);
        tabColorPanel?.SetActive(tab == TabType.Color);

        switch (tab)
        {
            case TabType.Item:
                PopulateItemList();
                break;
            case TabType.Texture:
                PopulateTextureList();
                break;
            case TabType.Color:
                PopulateColorList();
                break;
        }
    }

    #endregion

    #region 📂 Items & Catégories

    /// <summary>
    /// Lorsqu’un utilisateur clique sur une catégorie, on l’active
    /// </summary>
    private void OnCategorySelected((SlotType, GroupType?) category)
    {
        currentCategory = category;
        Debug.Log($"[CustomisationUI] Catégorie sélectionnée : {category}");
        SelectTab(TabType.Item);
    }

    /// <summary>
    /// Affiche dynamiquement tous les items correspondant à la catégorie sélectionnée
    /// </summary>
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

    /// <summary>
    /// Instancie et applique un item dans le bon slot (parent)
    /// </summary>
    private void EquipItem(Item item)
    {
        currentSelectedItem = item;
        var slotType = currentCategory.Item1;

        var slot = character.Slots.FirstOrDefault(s => s.Type == slotType);
        if (slot == null) return;

        slot.SetPrefab(item.prefab);
        slot.Toggle(true);
        //character.RefreshCustomization();
        // visualsHandler.Equip(slotType, item.prefab);


        // 🔄 Enregistre les choix locaux dans le struct
        dataToSave.SetItem(slotType, item.itemId);
        //customizationData.Data.Value = dataToSave;

        if (!customizationData.IsSpawned || customizationData.NetworkObject == null)
        {
            Debug.LogWarning("[CustomisationUI] ❌ Impossible d’envoyer un ServerRpc car le NetworkObject n’est pas prêt.");
            return;
        }
        customizationData.SetItemAndApplyLocal(slotType, item.itemId, item);

        if (item == null || item.prefab == null)
        {
            Debug.LogError($"[CustomisationUI] ❌ L’item ou son prefab est null → {item?.itemId}");
            return;
        }

        //if (IsHost)
        //{
        //    var allItems = Resources.LoadAll<Item>("Items").ToList();
        //    customizationData.ApplyToVisuals(visualsHandler, allItems);
        //}

    }
    #endregion

    #region 🎨 Textures & Couleurs

    /// <summary>
    /// Efface et prépare le panneau des textures
    /// </summary>
    private void PopulateTextureList()
    {
        ClearContainer(textureListContainer);

        var textureNames = new[] { "TextureDenim", "TextureFloral", "TextureZebra" };
        foreach (var texName in textureNames)
        {
            var tex = Resources.Load<Texture>($"Textures/{texName}");
            if (tex == null) continue;

            var btnObj = Instantiate(textureButtonPrefab, textureListContainer);
            btnObj.GetComponentInChildren<TMP_Text>().text = texName;
            btnObj.GetComponent<Button>().onClick.AddListener(() => ApplyTexture(texName));
        }
    }

    private void ApplyTexture(string textureName)
    {
        if (currentSelectedItem == null) return;
        var slotType = currentCategory.Item1;
        dataToSave.SetTexture(slotType, textureName);
        //customizationData.Data.Value.SetTexture(slotType, textureName);
    }


    /// <summary>
    /// Affiche une palette de couleurs à appliquer à l’item sélectionné
    /// </summary>
    private void PopulateColorList()
    {
        ClearContainer(colorListContainer);
        Color[] colors = { Color.white, Color.black, Color.red, Color.green, Color.blue, Color.yellow };
        foreach (var color in colors)
        {
            var btnObj = Instantiate(colorButtonPrefab, colorListContainer);
            btnObj.SetActive(true);
            var btnImage = btnObj.GetComponent<Image>();
            if (btnImage != null) btnImage.color = color;
            btnObj.GetComponent<Button>().onClick.AddListener(() => ApplyColor(color));
        }
    }

    /// <summary>
    /// Applique la couleur choisie au prefab actuellement équipé
    /// </summary>
    private void ApplyColor(Color color)
    {
        if (currentSelectedItem == null)
        {
            Debug.LogWarning("[CustomisationUI] Aucun item sélectionné.");
            return;
        }

        var slotType = currentCategory.Item1;
        var slot = character.Slots.FirstOrDefault(s => s.Type == slotType);

        if (slot == null)
        {
            Debug.LogWarning($"[CustomisationUI] Slot introuvable pour {slotType}");
            return;
        }

        var preview = slot.Preview;
        if (preview == null)
        {
            Debug.LogWarning($"[CustomisationUI] Aucun preview disponible pour {slotType}");
            return;
        }

        var renderers = preview.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning($"[CustomisationUI] Aucun Renderer trouvé pour {preview.name}");
            return;
        }

        foreach (var rend in renderers)
        {
            if (rend == null) continue;

            // `.material` crée une copie runtime (safe)
            Material[] runtimeMats = rend.materials;
            foreach (var mat in runtimeMats)
            {
                if (mat != null)
                    mat.color = color;
            }
        }

        // Sauvegarde de la couleur dans la structure
        dataToSave.SetColor(slotType, color);
        //customizationData.Data.Value.SetColor(slotType, color);
        Debug.Log($"[CustomisationUI] ✅ Couleur {color} appliquée à {slotType}");
    }


    #endregion

    #region ♲ Utils

    /// <summary>
    /// Sauvegarde toutes les données locales dans la variable réseau synchronisée.
    /// À appeler avant le défilé.
    /// </summary>
    public void CommitLocalCustomization()
    {
        if (customizationData == null) return;

        Debug.Log("[CustomisationUI] ✅ Commit de la tenue locale dans la NetworkVariable.");

        customizationData.Data = dataToSave;
        customizationData.SyncCustomizationDataServerRpc(dataToSave);
    }

    /// <summary>
    /// Rafraîchit la tenue globale du joueur, en envoyant les données au serveur.
    /// À appeler après un changement de tenue.
    /// </summary>
    public void RefreshTenueGlobale()
    {
        customizationData.SyncCustomizationDataServerRpc(dataToSave);
    }
    /// <summary>
    /// Détruit tous les enfants d’un conteneur
    /// </summary>
    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    #endregion

}