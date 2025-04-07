using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterCustomization
{
    public class CustomizableCharacterUI : MonoBehaviour
    {
        [System.Serializable]
        public class SlotUI
        {
            public SlotType slotType; // Type de slot (ex. Haut, Bas)
            public ScrollRect scrollView; // ScrollView pour la liste des prefabs
            public Transform content; // Conteneur des boutons dans le ScrollView
        }

        [System.Serializable]
        public class TextureOption
        {
            public string name; // Nom de la texture 
            public Texture2D texture; // Texture à appliquer au matériau
            public Sprite preview; // Vignette pour l’affichage dans l’UI
        }

        [Header("UI Configuration")]
        public GameObject characterPrefab; // Prefab du personnage
        public SlotLibrary slotLibrary; // Bibliothèque des slots disponibles
        public Camera mainCamera; // Caméra principale pour les raycasts et le zoom

        [Header("Boutons d'action")]
        public Button buttonEdit; // Bouton pour éditer un slot
        public Button buttonDelete; // Bouton pour supprimer un slot
        public Button buttonCustomize; // Bouton pour personnaliser la texture
        public Button buttonChangeColor; // Bouton pour changer la couleur

        [Header("Color Picker")]
        public ColorPicker colorPicker; // Composant pour sélectionner les couleurs

        [Header("Configuration des slots et des éléments UI")]
        public List<SlotUI> slotUIs; // Liste des slots UI
        public GameObject prefabsPanel; // Panneau pour les prefabs
        public GameObject tagsPanel; // Panneau pour les tags
        public GameObject buttonPrefab; // Prefab pour les boutons dans les slots
        public Sprite defaultSprite; // Sprite par défaut pour les boutons

        [Header("Textures")]
        public List<TextureOption> availableTextures; // Liste des textures disponibles
        public GameObject texturePanel; // Panneau UI pour choisir les textures
        public GameObject textureButtonPrefab; // Prefab pour les boutons de texture
        public Transform textureButtonContainer; // Conteneur des boutons de texture

        private CharacterCustomization _characterCustomization; // Référence au système de personnalisation
        private SlotType _selectedSlotType; // Type de slot actuellement sélectionné
        private GameObject _selectedInstance; // Instance de l’objet sélectionné
        private Vector3 _originalCameraPosition; // Position initiale de la caméra
        private Quaternion _originalCameraRotation; // Rotation initiale de la caméra

        private Dictionary<SlotType, (GameObject prefab, GameObject instance)> _equippedObjects = new(); // Dictionnaire des objets équipés

        // Initialisation du système de personnalisation
        public void Initialize(CharacterCustomization characterCustomization)
        {
            _characterCustomization = characterCustomization;
            _originalCameraPosition = mainCamera.transform.position;
            _originalCameraRotation = mainCamera.transform.rotation;

            // Désactiver les boutons d’action par défaut
            buttonEdit.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            buttonCustomize.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);

            PopulateUI(); 
            InitializeTexturePanel(); 
        }

        // Gestion des entrées tactiles
        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    HandleTouch(touch.position);
                }
            }
        }

        // Détection et sélection d’un objet touché
        private void HandleTouch(Vector2 touchPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("UI")))
            {
                GameObject hitObject = hit.collider.gameObject;
                SlotType? clickedSlotType = GetSlotTypeFromInstance(hitObject);

                if (clickedSlotType.HasValue)
                {
                    _selectedSlotType = clickedSlotType.Value;
                    _selectedInstance = hitObject;
                    ZoomToObject(_selectedInstance); 
                    ShowActionButtons(); 
                    DisableUIPanels();
                }
            }
        }

        // Zoom sur l’objet sélectionné
        private void ZoomToObject(GameObject target)
        {
            SkinnedMeshRenderer renderer = target.GetComponentInChildren<SkinnedMeshRenderer>();
            Vector3 targetCenter = renderer != null ? renderer.bounds.center : target.transform.position;
            Vector3 direction = (targetCenter - mainCamera.transform.position).normalized;
            Vector3 zoomPosition = targetCenter - direction * 1f;
            mainCamera.transform.position = zoomPosition;
            mainCamera.transform.LookAt(targetCenter);
        }

        // Réinitialisation de la caméra et de l’UI
        private void ResetCamera()
        {
            mainCamera.transform.position = _originalCameraPosition;
            mainCamera.transform.rotation = _originalCameraRotation;
            buttonEdit.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            buttonCustomize.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);
            EnableUIPanels(); 
            if (texturePanel != null) texturePanel.SetActive(false); 
            _selectedInstance = null;
        }

        // Affichage des boutons "Edit" et "Delete"
        private void ShowActionButtons()
        {
            buttonEdit.gameObject.SetActive(true);
            buttonDelete.gameObject.SetActive(true);
            buttonCustomize.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);

            buttonEdit.onClick.RemoveAllListeners();
            buttonEdit.onClick.AddListener(() => OnEditClicked());

            buttonDelete.onClick.RemoveAllListeners();
            buttonDelete.onClick.AddListener(() => OnDeleteClicked());
        }

        // Affichage des options d’édition 
        private void ShowEditOptions()
        {
            buttonEdit.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            buttonCustomize.gameObject.SetActive(true);
            buttonChangeColor.gameObject.SetActive(true);

            buttonCustomize.onClick.RemoveAllListeners();
            buttonCustomize.onClick.AddListener(() => OnCustomizeClicked());

            buttonChangeColor.onClick.RemoveAllListeners();
            buttonChangeColor.onClick.AddListener(() => OnChangeColorClicked());
        }

        // Désactivation des panneaux UI
        private void DisableUIPanels()
        {
            foreach (var slotUI in slotUIs)
            {
                if (slotUI.scrollView != null) slotUI.scrollView.gameObject.SetActive(false);
            }
            if (prefabsPanel != null) prefabsPanel.SetActive(false);
            if (tagsPanel != null) tagsPanel.SetActive(false);
        }

        // Réactivation des panneaux UI
        private void EnableUIPanels()
        {
            foreach (var slotUI in slotUIs)
            {
                if (slotUI.scrollView != null) slotUI.scrollView.gameObject.SetActive(true);
            }
            if (prefabsPanel != null) prefabsPanel.SetActive(true);
            if (tagsPanel != null) tagsPanel.SetActive(true);
        }

        // Remplissage de l’UI avec les prefabs disponibles
        public void PopulateUI()
        {
            ApplyTagFilter(new List<string>());
        }

        // Application d’un filtre par tags aux prefabs affichés
        public void ApplyTagFilter(List<string> selectedTags)
        {
            foreach (var slotUI in slotUIs)
            {
                var slot = _characterCustomization.Slots.FirstOrDefault(s => s.Type == slotUI.slotType);
                if (slot == null) continue;

                foreach (Transform child in slotUI.content)
                {
                    Destroy(child.gameObject); 
                }

                var prefabs = slot.GetAvailablePrefabs();
                foreach (var prefab in prefabs)
                {
                    ItemsSprite itemSprite = prefab.GetComponent<ItemsSprite>();
                    if (itemSprite == null || itemSprite.Tags == null) continue;

                    if (selectedTags.Count == 0 || selectedTags.Any(tag => itemSprite.Tags.Contains(tag)))
                    {
                        GameObject buttonObj = Instantiate(buttonPrefab, slotUI.content);
                        var buttonImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
                        var textMesh = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();

                        if (textMesh != null) textMesh.gameObject.SetActive(false);

                        if (buttonImage != null)
                        {
                            Sprite sprite = itemSprite.ItemSprite != null ? itemSprite.ItemSprite : defaultSprite;
                            buttonImage.sprite = sprite;
                            buttonImage.preserveAspect = true;
                            buttonImage.color = Color.white;
                        }

                        Button button = buttonObj.GetComponent<Button>();
                        button.onClick.AddListener(() => EquipPrefab(slotUI.slotType, prefab));
                    }
                }
            }
        }

        // Équiper un prefab sur un slot
        private void EquipPrefab(SlotType slotType, GameObject prefab)
        {
            var slot = _characterCustomization.Slots.FirstOrDefault(s => s.Type == slotType);
            if (slot != null)
            {
                if (_equippedObjects.ContainsKey(slotType))
                {
                    GameObject previousObject = _equippedObjects[slotType].instance;
                    if (previousObject != null)
                    {
                        Destroy(previousObject);
                    }
                }

                slot.SetPrefab(prefab);
                slot.Toggle(true);

                GameObject targetObject = slot.Preview;
                if (targetObject != null)
                {
                    GameObject instance = Instantiate(targetObject);
                    instance.transform.SetParent(_characterCustomization.CharacterInstance.transform);
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    instance.SetActive(true);
                    _equippedObjects[slotType] = (prefab, instance);
                }
                else
                {
                    Debug.LogWarning($"slot.Preview est null pour {slotType} avec prefab {prefab.name} !");
                }

                _characterCustomization.RefreshCustomization(); 
            }
        }

        // Déséquiper un prefab d’un slot
        private void UnequipPrefab(SlotType slotType)
        {
            if (_equippedObjects.ContainsKey(slotType))
            {
                GameObject equippedObject = _equippedObjects[slotType].instance;
                if (equippedObject != null) Destroy(equippedObject);

                _equippedObjects.Remove(slotType);
                var slot = _characterCustomization.Slots.FirstOrDefault(s => s.Type == slotType);
                if (slot != null) slot.Toggle(false);

                _characterCustomization.RefreshCustomization();
            }
        }

        // Gestion du clic sur le bouton "Edit"
        private void OnEditClicked()
        {
            if (_selectedInstance != null)
            {
                ShowEditOptions(); 
            }
        }

        // Gestion du clic sur le bouton "Delete"
        private void OnDeleteClicked()
        {
            UnequipPrefab(_selectedSlotType); 
            ResetCamera(); 
        }

        // Gestion du clic sur le bouton "Customize"
        private void OnCustomizeClicked()
        {
            if (_selectedInstance != null)
            {
                if (texturePanel != null)
                {
                    texturePanel.SetActive(true); 
                }
                else
                {
                    Debug.LogError("TexturePanel n’est pas assigné dans l’inspecteur !");
                }
            }
        }

        // Gestion du clic sur le bouton "Change Color"
        private void OnChangeColorClicked()
        {
            if (_selectedInstance != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    if (colorPicker == null)
                    {
                        Debug.LogError("ColorPicker n’est pas assigné dans l’inspecteur !");
                        return;
                    }
                    Color currentColor = renderer.material.color;
                    ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                }
            }
        }

        // Mise à jour de la couleur pendant la sélection
        private void OnColorChanged(Color color)
        {
            if (_selectedInstance != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null) renderer.material.color = color;
            }
        }

        // Confirmation de la couleur sélectionnée
        private void OnColorSelected(Color color)
        {
            if (_selectedInstance != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(renderer.material);
                    renderer.material.color = color;
                }
            }
            ResetCamera(); 
        }

        // Initialisation du panneau de textures avec des boutons
        private void InitializeTexturePanel()
        {
            if (textureButtonContainer == null || textureButtonPrefab == null)
            {
                Debug.LogError("textureButtonContainer ou textureButtonPrefab n’est pas assigné dans l’inspecteur !");
                return;
            }

            // Supprimer les anciens boutons
            foreach (Transform child in textureButtonContainer)
            {
                Destroy(child.gameObject);
            }

            // Créer un bouton pour chaque texture
            for (int i = 0; i < availableTextures.Count; i++)
            {
                int index = i;
                TextureOption textureOption = availableTextures[i];
                GameObject buttonObj = Instantiate(textureButtonPrefab, textureButtonContainer);
                buttonObj.SetActive(true); 
                Button button = buttonObj.GetComponent<Button>();
                Image buttonImage = buttonObj.GetComponent<Image>();

                if (buttonImage != null && textureOption.preview != null)
                {
                    buttonImage.enabled = true; 
                    buttonImage.sprite = textureOption.preview;
                    buttonImage.preserveAspect = true;
                }
                else
                {
                    Debug.LogWarning($"Image ou preview manquant pour la texture : {textureOption.name}");
                }

                if (button != null)
                {
                    button.enabled = true; 
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ApplyTexture(index));
                }
                else
                {
                    Debug.LogWarning($"Aucun composant Button trouvé sur le clone pour la texture : {textureOption.name}");
                }
            }

            if (texturePanel != null) texturePanel.SetActive(false); 
        }

        // Application d’une texture sur l’objet sélectionné
        private void ApplyTexture(int textureIndex)
        {
            if (_selectedInstance != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    if (textureIndex >= 0 && textureIndex < availableTextures.Count)
                    {
                        Texture2D newTexture = availableTextures[textureIndex].texture;
                        Material material = new Material(renderer.material); 
                        renderer.material = material;
                        if (!material.HasProperty("_BaseMap"))
                        {
                            Debug.LogWarning("Le shader n’a pas de propriété _BaseMap ! Vérifiez le shader utilisé.");
                        }
                        else
                        {
                            material.SetTexture("_BaseMap", newTexture); // Appliquer la texture sur _BaseMap 
                        }
                        texturePanel.SetActive(false); // Cacher le panneau après sélection
                    }
                    else
                    {
                        Debug.LogWarning("Index de texture invalide !");
                    }
                }
                else
                {
                    Debug.LogWarning("Aucun SkinnedMeshRenderer trouvé sur l’instance sélectionnée.");
                }
            }
        }

        // Récupération du type de slot à partir d’une instance
        private SlotType? GetSlotTypeFromInstance(GameObject instance)
        {
            foreach (var equipped in _equippedObjects)
            {
                if (equipped.Value.instance == instance) return equipped.Key;
            }
            return null;
        }
    }
}