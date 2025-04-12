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
            public SlotType slotType;
            public ScrollRect scrollView;
            public Transform content;
        }

        [System.Serializable]
        public class TextureOption
        {
            public string name;
            public Texture2D texture;
            public Sprite preview;
        }

        [Header("UI Configuration")]
        public GameObject characterPrefab; // Prefab du personnage de base
        public Camera mainCamera;
        public ButtonScrollViewManager buttonManager;

        [Header("Boutons d'action")]
        public Button buttonChangeTexture;
        public Button buttonChangeColor;
        public Button buttonChangeBaseColor; // Pour la couleur du personnage de base

        [Header("Panels")]
        public GameObject texturePanel;
        public GameObject colorPickerPanel;

        [Header("Configuration des slots et des éléments UI")]
        public List<SlotUI> slotUIs;
        public GameObject prefabsPanel;
        public GameObject tagsPanel;
        public GameObject buttonPrefab; // Bouton pour afficher les options de vêtements
        public Sprite defaultSprite;

        [Header("Textures")]
        public List<TextureOption> availableTextures;
        public GameObject textureButtonPrefab;
        public Transform textureButtonContainer;

        [Header("Vêtements (Scriptable Objects)")]
        public List<Item> clothingItems; // Liste des ScriptableObject Item

        private GameObject _characterInstance; // Instance du personnage de base
        private SlotType _selectedSlotType;
        private GameObject _selectedInstance; // Instance de vêtement sélectionnée
        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;
        private Dictionary<SlotType, (Item item, GameObject instance)> _equippedClothing = new();
        private Color _initialColor;
        private Texture2D _initialTexture;

        public void Initialize()
        {
            _originalCameraPosition = mainCamera.transform.position;
            _originalCameraRotation = mainCamera.transform.rotation;

            if (_characterInstance == null && characterPrefab != null)
            {
                _characterInstance = Instantiate(characterPrefab, Vector3.zero, Quaternion.identity);
            }

            if (buttonManager != null)
            {
                buttonManager.Initialize(this);
            }

            if (buttonChangeTexture != null) buttonChangeTexture.gameObject.SetActive(false);
            if (buttonChangeColor != null) buttonChangeColor.gameObject.SetActive(false);
            if (buttonChangeBaseColor != null) buttonChangeBaseColor.gameObject.SetActive(true);

            if (texturePanel != null) texturePanel.SetActive(false);
            if (colorPickerPanel != null) colorPickerPanel.SetActive(false);

            PopulateUI();
            InitializeTexturePanel();

            if (buttonChangeTexture != null)
            {
                buttonChangeTexture.onClick.RemoveAllListeners();
                buttonChangeTexture.onClick.AddListener(OnChangeTextureClicked);
            }
            if (buttonChangeColor != null)
            {
                buttonChangeColor.onClick.RemoveAllListeners();
                buttonChangeColor.onClick.AddListener(OnChangeColorClicked);
            }
            if (buttonChangeBaseColor != null)
            {
                buttonChangeBaseColor.onClick.RemoveAllListeners();
                buttonChangeBaseColor.onClick.AddListener(OnChangeBaseColorClicked);
            }
        }

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

        private void HandleTouch(Vector2 touchPosition)
        {
            if ((buttonChangeTexture != null && buttonChangeTexture.gameObject.activeSelf) ||
                (buttonChangeColor != null && buttonChangeColor.gameObject.activeSelf))
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("UI")))
            {
                SlotType? clickedSlotType = GetSlotTypeFromInstance(hit.collider.gameObject);
                if (clickedSlotType.HasValue)
                {
                    _selectedSlotType = clickedSlotType.Value;
                    _selectedInstance = hit.collider.gameObject;
                    ZoomToObject(_selectedInstance);
                    buttonManager.ShowInitialButtons();
                    DisableUIPanels();
                }
            }
        }

        private void ZoomToObject(GameObject target)
        {
            SkinnedMeshRenderer renderer = target.GetComponentInChildren<SkinnedMeshRenderer>();
            Vector3 targetCenter = renderer != null ? renderer.bounds.center : target.transform.position;
            Vector3 direction = (targetCenter - mainCamera.transform.position).normalized;
            mainCamera.transform.position = targetCenter - direction * 1f;
            mainCamera.transform.LookAt(targetCenter);
        }

        public void ResetCamera()
        {
            mainCamera.transform.position = _originalCameraPosition;
            mainCamera.transform.rotation = _originalCameraRotation;
            EnableUIPanels();
            if (buttonChangeTexture != null) buttonChangeTexture.gameObject.SetActive(false);
            if (buttonChangeColor != null) buttonChangeColor.gameObject.SetActive(false);
            if (texturePanel != null) texturePanel.SetActive(false);
            if (colorPickerPanel != null) colorPickerPanel.SetActive(false);
            _selectedInstance = null;
        }

        public void OnEditClicked()
        {
            if (_selectedInstance != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    _initialColor = renderer.material.color;
                    _initialTexture = renderer.material.GetTexture("_BaseMap") as Texture2D;
                }

                if (buttonChangeTexture != null) buttonChangeTexture.gameObject.SetActive(true);
                if (buttonChangeColor != null) buttonChangeColor.gameObject.SetActive(true);
                buttonManager.ShowEditOptions();
            }
        }

        public void OnDeleteClicked()
        {
            UnequipClothing(_selectedSlotType);
            ResetCamera();
        }

        public void OnChangeTextureClicked()
        {
            if (_selectedInstance != null && texturePanel != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.color = _initialColor;
                    renderer.material.SetTexture("_BaseMap", _initialTexture);
                }

                if (colorPickerPanel != null) colorPickerPanel.SetActive(false);

                texturePanel.SetActive(true);
                buttonManager.ShowTextureOptions();
            }
        }

        public void OnChangeColorClicked()
        {
            if (_selectedInstance != null && colorPickerPanel != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.color = _initialColor;
                    renderer.material.SetTexture("_BaseMap", _initialTexture);

                    if (texturePanel != null) texturePanel.SetActive(false);

                    colorPickerPanel.SetActive(true);
                    Color currentColor = renderer.material.color;
                    bool success = ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                    if (!success)
                    {
                        colorPickerPanel.SetActive(false);
                    }
                }
            }
        }

        public void OnChangeBaseColorClicked()
        {
            if (_characterInstance != null && colorPickerPanel != null)
            {
                SkinnedMeshRenderer renderer = _characterInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    if (texturePanel != null) texturePanel.SetActive(false);

                    colorPickerPanel.SetActive(true);
                    Color currentColor = renderer.material.color;
                    ColorPicker.Create(currentColor, "Couleur du personnage de base", renderer, OnColorChangedBase, OnBaseColorSelected);
                }
            }
        }

        public void OnBackFromTextureClicked()
        {
            if (texturePanel != null) texturePanel.SetActive(false);
            buttonManager.ShowEditOptions();
        }

        public void OnBackFromEditClicked()
        {
            if (buttonChangeTexture != null) buttonChangeTexture.gameObject.SetActive(false);
            if (buttonChangeColor != null) buttonChangeColor.gameObject.SetActive(false);
            buttonManager.ShowInitialButtons();
        }

        public void ShowTagsPanel()
        {
            if (tagsPanel != null) tagsPanel.SetActive(true);
        }

        private void DisableUIPanels()
        {
            foreach (var slotUI in slotUIs)
            {
                if (slotUI.scrollView != null) slotUI.scrollView.gameObject.SetActive(false);
            }
            if (prefabsPanel != null) prefabsPanel.SetActive(false);
            if (tagsPanel != null) tagsPanel.SetActive(false);
        }

        private void EnableUIPanels()
        {
            foreach (var slotUI in slotUIs)
            {
                if (slotUI.scrollView != null) slotUI.scrollView.gameObject.SetActive(true);
            }
            if (prefabsPanel != null) prefabsPanel.SetActive(true);
            if (tagsPanel != null) tagsPanel.SetActive(true);
        }

        public void PopulateUI()
        {
            ApplyTagFilter(new List<string>());
        }

        public void ApplyTagFilter(List<string> selectedTags)
        {
            foreach (var slotUI in slotUIs)
            {
                foreach (var item in clothingItems)
                {
                    if (item.category == slotUI.slotType)
                    {
                        // Vérifier les tags directement depuis l'Item
                        if (selectedTags.Count == 0 || selectedTags.Any(tag => item.tags.Contains(tag)))
                        {
                            GameObject buttonObj = Instantiate(buttonPrefab, slotUI.content);
                            SetupButtonSprite(buttonObj, item);
                            AddButtonListener(buttonObj, slotUI.slotType, item);
                        }
                    }
                }
            }
        }

        private void SetupButtonSprite(GameObject buttonObj, Item item)
        {
            Image buttonImage = buttonObj.GetComponentInChildren<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = item.icon != null ? item.icon : defaultSprite;
                buttonImage.preserveAspect = true;
                buttonImage.color = Color.white;
            }
        }

        private void AddButtonListener(GameObject buttonObj, SlotType slotType, Item item)
        {
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => EquipClothing(slotType, item));
            }
        }

        private void EquipClothing(SlotType slotType, Item clothingItem)
        {
            if (_equippedClothing.ContainsKey(slotType))
            {
                Destroy(_equippedClothing[slotType].instance);
            }

            GameObject instance = Instantiate(clothingItem.prefab, _characterInstance.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instance.SetActive(true);
            _equippedClothing[slotType] = (clothingItem, instance);
        }

        private void UnequipClothing(SlotType slotType)
        {
            if (_equippedClothing.ContainsKey(slotType))
            {
                Destroy(_equippedClothing[slotType].instance);
                _equippedClothing.Remove(slotType);
            }
        }

        private void OnColorChanged(Color color)
        {
            if (_selectedInstance != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null) renderer.material.color = color;
            }
        }

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

        private void OnColorChangedBase(Color color)
        {
            if (_characterInstance != null)
            {
                SkinnedMeshRenderer renderer = _characterInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null) renderer.material.color = color;
            }
        }

        private void OnBaseColorSelected(Color color)
        {
            if (_characterInstance != null)
            {
                SkinnedMeshRenderer renderer = _characterInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(renderer.material);
                    renderer.material.color = color;
                }
            }
            ResetCamera();
        }

        private void InitializeTexturePanel()
        {
            if (textureButtonContainer == null || textureButtonPrefab == null) return;

            foreach (Transform child in textureButtonContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < availableTextures.Count; i++)
            {
                int index = i;
                TextureOption option = availableTextures[i];
                GameObject buttonObj = Instantiate(textureButtonPrefab, textureButtonContainer);
                buttonObj.SetActive(true);

                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => ApplyTexture(index));
                }

                Image buttonImage = buttonObj.GetComponent<Image>();
                if (buttonImage != null && option.preview != null)
                {
                    buttonImage.sprite = option.preview;
                    buttonImage.preserveAspect = true;
                    buttonImage.color = Color.white;
                }
            }

            if (texturePanel != null) texturePanel.SetActive(false);
        }

        private void ApplyTexture(int textureIndex)
        {
            if (_selectedInstance != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null && textureIndex >= 0 && textureIndex < availableTextures.Count)
                {
                    Texture2D newTexture = availableTextures[textureIndex].texture;
                    Material material = new Material(renderer.material);
                    material.SetTexture("_BaseMap", newTexture);
                    renderer.material = material;
                }
            }
        }

        private SlotType? GetSlotTypeFromInstance(GameObject instance)
        {
            foreach (var equipped in _equippedClothing)
            {
                if (equipped.Value.instance == instance) return equipped.Key;
            }
            return null;
        }
    }
}