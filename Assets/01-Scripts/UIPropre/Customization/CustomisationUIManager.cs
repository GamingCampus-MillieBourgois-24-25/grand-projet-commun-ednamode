using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CharacterCustomization;
using CharacterCustomizationNamespace = CharacterCustomization;

/// <summary>
/// UI de personnalisation d’un personnage local, avec gestion des catégories SlotType et GroupType,
/// ainsi que les textures et couleurs. Basé sur SlotLibrary + CharacterCustomization.
/// </summary>
public class CustomisationUIManager : MonoBehaviour
{
    #region ✨ Data & References

    [Header("✨ Configuration Personnage")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private SlotLibrary slotLibrary;

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

    private Dictionary<(SlotType, GroupType?), List<CustomizationItem>> categorizedItems;
    private Dictionary<GroupType, SlotType> redirectedGroups;
    private (SlotType, GroupType?) currentCategory;
    private CustomizationItem currentSelectedItem;

    private CharacterCustomizationNamespace.CharacterCustomization character;
    private HashSet<SlotType> availableSlotTypes;

    #endregion

    #region 🚀 Initialisation

    /// <summary>
    /// Démarre le système de customisation : instancie le personnage, charge les items, et construit l’UI
    /// </summary>
    private void Start()
    {
        character = new CharacterCustomizationNamespace.CharacterCustomization(characterPrefab, slotLibrary);
        availableSlotTypes = character.Slots.Select(s => s.Type).ToHashSet();
        BuildRedirectMap();

        LoadItems();
        PopulateCategoryButtons();
        Debug.Log($"[CustomisationUI] {categorizedItems.Count} catégories chargées.");

        if (categorizedItems.Count > 0)
        {
            currentCategory = categorizedItems.Keys.First();
            PopulateItemList();
        }
        else
        {
            Debug.LogWarning("[CustomisationUI] Aucun item trouvé dans Resources/Items.");
        }

        tabItemButton.onClick.AddListener(() => SelectTab(TabType.Item));
        tabTextureButton.onClick.AddListener(() => SelectTab(TabType.Texture));
        tabColorButton.onClick.AddListener(() => SelectTab(TabType.Color));
    }

    /// <summary>
    /// Construit une map de redirection entre les GroupTypes et leurs SlotType parent.
    /// </summary>
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

    /// <summary>
    /// Charge tous les items depuis Resources/Items et les catégorise par SlotType + GroupType
    /// </summary>
    private void LoadItems()
    {
        categorizedItems = new();
        var allItems = Resources.LoadAll<CustomizationItem>("Items");
        Debug.Log($"[CustomisationUI] {allItems.Length} items chargés depuis Resources/Items.");

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
                categorizedItems[key] = new List<CustomizationItem>();

            categorizedItems[key].Add(item);
        }
    }

    /// <summary>
    /// Instancie un bouton par catégorie (SlotType ou GroupType) en bas de l’écran
    /// </summary>
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
    private void EquipItem(CustomizationItem item)
    {
        currentSelectedItem = item;
        var slotType = currentCategory.Item1;

        var slot = character.Slots.FirstOrDefault(s => s.Type == slotType);
        if (slot == null)
        {
            Debug.LogWarning($"[CustomisationUI] Aucun slot pour la catégorie {slotType}");
            return;
        }

        slot.SetPrefab(item.prefab);
        slot.Toggle(true);
        character.RefreshCustomization();

        Debug.Log($"[CustomisationUI] Équipement : {item.itemName} sur {slotType}");
    }

    #endregion

    #region 🎨 Textures & Couleurs

    /// <summary>
    /// Efface et prépare le panneau des textures
    /// </summary>
    private void PopulateTextureList() => ClearContainer(textureListContainer);

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
        if (currentSelectedItem == null) return;
        var slotType = currentCategory.Item1;
        var slot = character.Slots.FirstOrDefault(s => s.Type == slotType);
        if (slot == null || !slot.HasPrefab()) return;

        var preview = slot.Preview;
        if (preview == null) return;

        foreach (var rend in preview.GetComponentsInChildren<Renderer>())
            foreach (var mat in rend.materials)
                mat.color = color;
    }

    #endregion

    #region ♲ Utils

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