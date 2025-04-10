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
        public GameObject characterPrefab;
        public SlotLibrary slotLibrary;
        public Camera mainCamera;
        public ButtonScrollViewManager buttonManager;

        [Header("Boutons d'action")]
        public Button buttonChangeTexture;
        public Button buttonChangeColor;

        [Header("Panels")]
        public GameObject texturePanel;
        public GameObject colorPickerPanel;

        [Header("Configuration des slots et des éléments UI")]
        public List<SlotUI> slotUIs;
        public GameObject prefabsPanel;
        public GameObject tagsPanel;
        public GameObject buttonPrefab;
        public Sprite defaultSprite;

        [Header("Textures")]
        public List<TextureOption> availableTextures;
        public GameObject textureButtonPrefab;
        public Transform textureButtonContainer;

        private CharacterCustomization _characterCustomization;
        private SlotType _selectedSlotType;
        private GameObject _selectedInstance;
        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;
        private Dictionary<SlotType, (GameObject prefab, GameObject instance)> _equippedObjects = new();

        // Variables pour stocker l’état initial
        private Color _initialColor;
        private Texture2D _initialTexture;

        public void Initialize(CharacterCustomization characterCustomization)
        {
            _characterCustomization = characterCustomization;
            _originalCameraPosition = mainCamera.transform.position;
            _originalCameraRotation = mainCamera.transform.rotation;

            if (buttonManager != null)
            {
                buttonManager.Initialize(this);
            }
            else
            {
                Debug.LogError("ButtonScrollViewManager n’est pas assigné dans l’inspecteur !");
            }

            if (buttonChangeTexture != null) buttonChangeTexture.gameObject.SetActive(false);
            if (buttonChangeColor != null) buttonChangeColor.gameObject.SetActive(false);
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
                // Stocker l’état initial de l’objet
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
            UnequipPrefab(_selectedSlotType);
            ResetCamera();
        }

        public void OnChangeTextureClicked()
        {
            if (_selectedInstance != null && texturePanel != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    // Restaurer l’état initial avant de changer la texture
                    renderer.material.color = _initialColor;
                    renderer.material.SetTexture("_BaseMap", _initialTexture);
                }

                // Fermer le colorPickerPanel s’il est ouvert
                if (colorPickerPanel != null) colorPickerPanel.SetActive(false);

                texturePanel.SetActive(true);
                buttonManager.ShowTextureOptions();
            }
            else
            {
                Debug.LogError("texturePanel n’est pas assigné ou _selectedInstance est null !");
            }
        }

        public void OnChangeColorClicked()
        {
            if (_selectedInstance != null && colorPickerPanel != null)
            {
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    // Restaurer l’état initial avant de changer la couleur
                    renderer.material.color = _initialColor;
                    renderer.material.SetTexture("_BaseMap", _initialTexture);

                    // Fermer le texturePanel s’il est ouvert
                    if (texturePanel != null) texturePanel.SetActive(false);

                    colorPickerPanel.SetActive(true);
                    Color currentColor = renderer.material.color; // Doit être après la réinitialisation
                    bool success = ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                    if (!success)
                    {
                        Debug.LogError("Échec de la création du ColorPicker ! Une instance est peut-être déjà active.");
                        colorPickerPanel.SetActive(false);
                    }
                    else
                    {
                        Debug.Log("ColorPicker activé avec succès.");
                    }
                }
                else
                {
                    Debug.LogError("Aucun SkinnedMeshRenderer trouvé sur l’instance sélectionnée !");
                }
            }
            else
            {
                Debug.LogError("colorPickerPanel ou _selectedInstance est null !");
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
                            buttonImage.sprite = itemSprite.ItemSprite != null ? itemSprite.ItemSprite : defaultSprite;
                            buttonImage.preserveAspect = true;
                            buttonImage.color = Color.white;
                        }

                        Button button = buttonObj.GetComponent<Button>();
                        button.onClick.AddListener(() => EquipPrefab(slotUI.slotType, prefab));
                    }
                }
            }
        }

        private void EquipPrefab(SlotType slotType, GameObject prefab)
        {
            var slot = _characterCustomization.Slots.FirstOrDefault(s => s.Type == slotType);
            if (slot != null)
            {
                if (_equippedObjects.ContainsKey(slotType))
                {
                    Destroy(_equippedObjects[slotType].instance);
                }

                slot.SetPrefab(prefab);
                slot.Toggle(true);

                GameObject instance = Instantiate(slot.Preview, _characterCustomization.CharacterInstance.transform);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.SetActive(true);
                _equippedObjects[slotType] = (prefab, instance);

                _characterCustomization.RefreshCustomization();
            }
        }

        private void UnequipPrefab(SlotType slotType)
        {
            if (_equippedObjects.ContainsKey(slotType))
            {
                Destroy(_equippedObjects[slotType].instance);
                _equippedObjects.Remove(slotType);
                var slot = _characterCustomization.Slots.FirstOrDefault(s => s.Type == slotType);
                if (slot != null) slot.Toggle(false);
                _characterCustomization.RefreshCustomization();
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

        private void InitializeTexturePanel()
        {
            if (textureButtonContainer == null || textureButtonPrefab == null)
            {
                Debug.LogError("textureButtonContainer ou textureButtonPrefab n’est pas assigné dans l’inspecteur !");
                return;
            }

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
                    button.enabled = true;
                    button.interactable = true;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ApplyTexture(index));
                }
                else
                {
                    Debug.LogWarning($"Aucun composant Button trouvé sur textureButtonPrefab pour la texture {option.name} !");
                }

                Image buttonImage = buttonObj.GetComponent<Image>();
                if (buttonImage != null && option.preview != null)
                {
                    buttonImage.enabled = true;
                    buttonImage.sprite = option.preview;
                    buttonImage.preserveAspect = true;
                    buttonImage.color = Color.white;
                }
                else
                {
                    Debug.LogWarning($"Image ou preview manquant pour la texture : {option.name}");
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
            foreach (var equipped in _equippedObjects)
            {
                if (equipped.Value.instance == instance) return equipped.Key;
            }
            return null;
        }
    }
}