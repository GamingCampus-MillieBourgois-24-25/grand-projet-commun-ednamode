using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Ajout pour TextMeshProUGUI

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
            private List<GameObject> buttonPool = new List<GameObject>();
            public int currentButtonIndex = 0;

            public void ResetButtonPool()
            {
                currentButtonIndex = 0;
                foreach (var button in buttonPool)
                {
                    button.SetActive(false);
                }
            }

            public GameObject GetOrCreateButton(GameObject buttonPrefab, Transform parent)
            {
                GameObject buttonObj;
                if (currentButtonIndex < buttonPool.Count)
                {
                    buttonObj = buttonPool[currentButtonIndex];
                    buttonObj.SetActive(true);
                }
                else
                {
                    buttonObj = Instantiate(buttonPrefab, parent);
                    buttonPool.Add(buttonObj);
                }
                currentButtonIndex++;
                return buttonObj;
            }
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
        public Camera mainCamera;
        public ButtonScrollViewManager buttonManager;

        [Header("Boutons d'action")]
        public Button buttonChangeTexture;
        public Button buttonChangeColor;
        public Button buttonChangeBaseColor;
        public Button buttonReturnToClothing;
        public Button buttonReturnToModified;

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

        [Header("Slot Library (contient les Items)")]
        public SlotLibrary slotLibrary;

        [Header("Équipés - ScrollView")]
        public ScrollRect equippedItemsScrollView;
        public Transform equippedItemsContent;

        private GameObject _characterInstance;
        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;
        private Dictionary<SlotType, (Item item, GameObject instance)> _equippedClothing = new();
        private GameObject _lastEquippedInstance;
        private Color _initialColor;
        private Texture2D _initialTexture;
        private Color _lastModifiedColor;
        private bool _hasModifiedColor;
        private Texture2D _lastModifiedTexture;
        private List<string> _lastAppliedTags;
        private string _lastPanelUsed;

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

            if (buttonChangeTexture != null) buttonChangeTexture.gameObject.SetActive(true);
            if (buttonChangeColor != null) buttonChangeColor.gameObject.SetActive(true);
            if (buttonChangeBaseColor != null) buttonChangeBaseColor.gameObject.SetActive(true);
            if (buttonReturnToClothing != null) buttonReturnToClothing.gameObject.SetActive(true);
            if (buttonReturnToModified != null) buttonReturnToModified.gameObject.SetActive(true);

            if (texturePanel != null) texturePanel.SetActive(false);
            if (colorPickerPanel != null) colorPickerPanel.SetActive(false);
            if (tagsPanel != null)
            {
                tagsPanel.SetActive(false);
            }

            if (equippedItemsScrollView != null) equippedItemsScrollView.gameObject.SetActive(true);

            EnableUIPanels();

            InitializeButtons();
            InitializeTexturePanel();
            UpdateEquippedItemsUI();

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
            if (buttonReturnToClothing != null)
            {
                buttonReturnToClothing.onClick.RemoveAllListeners();
                buttonReturnToClothing.onClick.AddListener(OnReturnToClothingClicked);
            }
            if (buttonReturnToModified != null)
            {
                buttonReturnToModified.onClick.RemoveAllListeners();
                buttonReturnToModified.onClick.AddListener(OnReturnToModifiedClicked);
            }
        }

        private void InitializeButtons()
        {
            if (slotLibrary == null) return;

            foreach (var slotUI in slotUIs)
            {
                if (slotUI.content == null)
                {
                    continue;
                }

                slotUI.ResetButtonPool();

                var slotEntry = slotLibrary.Slots.FirstOrDefault(slot => slot.Type == slotUI.slotType);
                if (slotEntry == null) continue;

                foreach (var group in slotEntry.Groups)
                {
                    foreach (var item in group.Items)
                    {
                        if (item == null) continue;

                        if (item.category != slotUI.slotType) continue;

                        GameObject buttonObj = slotUI.GetOrCreateButton(buttonPrefab, slotUI.content);
                        SetupButtonSprite(buttonObj, item);
                        AddButtonListener(buttonObj, slotUI.slotType, item);
                    }
                }
            }
        }

        public void ResetCamera()
        {
            mainCamera.transform.position = _originalCameraPosition;
            mainCamera.transform.rotation = _originalCameraRotation;
            EnableUIPanels();
            if (buttonChangeTexture != null) buttonChangeTexture.gameObject.SetActive(true);
            if (buttonChangeColor != null) buttonChangeColor.gameObject.SetActive(true);
            if (buttonReturnToClothing != null) buttonReturnToClothing.gameObject.SetActive(true);
            if (buttonReturnToModified != null) buttonReturnToModified.gameObject.SetActive(true);
            if (texturePanel != null) texturePanel.SetActive(false);
            if (colorPickerPanel != null) colorPickerPanel.SetActive(false);
        }

        public void OnChangeTextureClicked()
        {
            if (_lastEquippedInstance != null && texturePanel != null)
            {
                SkinnedMeshRenderer renderer = _lastEquippedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    _initialColor = renderer.material.color;
                    _initialTexture = renderer.material.GetTexture("_BaseMap") as Texture2D;
                    renderer.material.color = _initialColor;
                    renderer.material.SetTexture("_BaseMap", _initialTexture);
                }

                if (colorPickerPanel != null) colorPickerPanel.SetActive(false);

                texturePanel.SetActive(true);
                _lastPanelUsed = "Texture";
                buttonManager.ShowTextureOptions();
            }
        }

        public void OnChangeColorClicked()
        {
            if (_lastEquippedInstance != null && colorPickerPanel != null)
            {
                SkinnedMeshRenderer renderer = _lastEquippedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    _initialColor = renderer.material.color;
                    _initialTexture = renderer.material.GetTexture("_BaseMap") as Texture2D;
                    renderer.material.color = _initialColor;
                    renderer.material.SetTexture("_BaseMap", _initialTexture);

                    if (texturePanel != null) texturePanel.SetActive(false);

                    colorPickerPanel.SetActive(true);
                    _lastPanelUsed = "Color";
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
                    _lastPanelUsed = "Color";
                    Color currentColor = renderer.material.color;
                    ColorPicker.Create(currentColor, "Couleur du personnage de base", renderer, OnColorChangedBase, OnBaseColorSelected);
                }
            }
        }

        public void OnBackFromTextureClicked()
        {
            if (texturePanel != null) texturePanel.SetActive(false);
            buttonManager.ReturnToMainView();
        }

        public void OnReturnToClothingClicked()
        {
            ResetCamera();

            if (_lastEquippedInstance != null)
            {
                SkinnedMeshRenderer renderer = _lastEquippedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.color = _initialColor;
                    renderer.material.SetTexture("_BaseMap", _initialTexture);
                }
            }
        }

        public void OnReturnToModifiedClicked()
        {
            if (_lastPanelUsed == "Texture" && texturePanel != null)
            {
                if (colorPickerPanel != null) colorPickerPanel.SetActive(false);
                texturePanel.SetActive(true);
                buttonManager.ShowTextureOptions();
            }
            else if (_lastPanelUsed == "Color" && colorPickerPanel != null)
            {
                if (texturePanel != null) texturePanel.SetActive(false);
                colorPickerPanel.SetActive(true);
                if (_lastEquippedInstance != null)
                {
                    SkinnedMeshRenderer renderer = _lastEquippedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (renderer != null)
                    {
                        Color currentColor = renderer.material.color;
                        ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                    }
                }
            }

            if (_lastEquippedInstance != null)
            {
                SkinnedMeshRenderer renderer = _lastEquippedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    if (_hasModifiedColor)
                    {
                        renderer.material.color = _lastModifiedColor;
                    }

                    if (_lastModifiedTexture != null)
                    {
                        renderer.material.SetTexture("_BaseMap", _lastModifiedTexture);
                    }
                }
            }
        }

        public void ShowTagsPanel()
        {
            if (tagsPanel != null)
            {
                bool newState = !tagsPanel.activeSelf;
                tagsPanel.SetActive(newState);
            }
        }

        private void EnableUIPanels()
        {
            if (prefabsPanel != null)
            {
                prefabsPanel.SetActive(true);
            }
            if (equippedItemsScrollView != null)
            {
                equippedItemsScrollView.gameObject.SetActive(true);
            }
        }

        public void ApplyTagFilter(List<string> selectedTags)
        {
            if (slotLibrary == null)
            {
                return;
            }

            if (slotLibrary.Slots == null)
            {
                return;
            }

            if (_lastAppliedTags != null && selectedTags.SequenceEqual(_lastAppliedTags))
            {
                return;
            }

            _lastAppliedTags = new List<string>(selectedTags);

            foreach (var slotUI in slotUIs)
            {
                if (slotUI == null || slotUI.content == null)
                {
                    continue;
                }

                slotUI.ResetButtonPool();

                var slotEntry = slotLibrary.Slots.FirstOrDefault(slot => slot.Type == slotUI.slotType);
                if (slotEntry == null) continue;

                foreach (var group in slotEntry.Groups)
                {
                    if (group == null || group.Items == null) continue;

                    foreach (var item in group.Items)
                    {
                        if (item == null || item.category != slotUI.slotType || item.tags == null) continue;

                        bool matchesTag = selectedTags.Count > 0 && selectedTags.Any(tag => item.tags.Contains(tag));
                        if (matchesTag)
                        {
                            GameObject buttonObj = slotUI.GetOrCreateButton(buttonPrefab, slotUI.content);
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
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => EquipClothing(slotType, item));
            }
        }

        private void EquipClothing(SlotType slotType, Item item)
        {
            if (_equippedClothing.ContainsKey(slotType))
            {
                Destroy(_equippedClothing[slotType].instance);
            }

            GameObject instance = Instantiate(item.prefab, _characterInstance.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instance.SetActive(true);

            AnimationSync animationSync = instance.GetComponent<AnimationSync>();
            if (animationSync != null)
            {
                animationSync.Initialize(_characterInstance);
            }
           

            _equippedClothing[slotType] = (item, instance);
            _lastEquippedInstance = instance;
            _hasModifiedColor = false;
            _lastModifiedColor = Color.white;
            _lastModifiedTexture = null;
            UpdateEquippedItemsUI();
        }

        private void UnequipClothing(SlotType slotType)
        {
            if (_equippedClothing.ContainsKey(slotType))
            {
                Destroy(_equippedClothing[slotType].instance);
                if (_lastEquippedInstance == _equippedClothing[slotType].instance)
                {
                    _lastEquippedInstance = null;
                    _hasModifiedColor = false;
                    _lastModifiedColor = Color.white;
                    _lastModifiedTexture = null;
                }
                _equippedClothing.Remove(slotType);
                UpdateEquippedItemsUI();
            }
        }

        private void UpdateEquippedItemsUI()
        {
            if (equippedItemsContent == null)
            {
                return;
            }

            foreach (Transform child in equippedItemsContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var equipped in _equippedClothing)
            {
                Item item = equipped.Value.item;
                GameObject buttonObj = Instantiate(buttonPrefab, equippedItemsContent);
                SetupButtonSprite(buttonObj, item);
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        _lastEquippedInstance = equipped.Value.instance;
                        UnequipClothing(equipped.Key);
                    });
                }
            }
        }

        private void OnColorChanged(Color color)
        {
            if (_lastEquippedInstance != null)
            {
                SkinnedMeshRenderer renderer = _lastEquippedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                    _lastModifiedColor = color;
                    _hasModifiedColor = true;
                }
            }
        }

        private void OnColorSelected(Color color)
        {
            if (_lastEquippedInstance != null)
            {
                SkinnedMeshRenderer renderer = _lastEquippedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(renderer.material);
                    renderer.material.color = color;
                    _lastModifiedColor = color;
                    _hasModifiedColor = true;
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
            if (textureButtonContainer == null || textureButtonPrefab == null)
            {
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
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ApplyTexture(index));
                }
                

                Image buttonImage = buttonObj.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.enabled = true; 
                    if (option.preview != null)
                    {
                        buttonImage.sprite = option.preview;
                        buttonImage.preserveAspect = true;
                        buttonImage.color = Color.white;
                    }
                  
                }
             

                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.enabled = true;
                    buttonText.text = option.name; 
                }

                Text legacyText = buttonObj.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.enabled = true; 
                    legacyText.text = option.name; 
                }
            }

            if (texturePanel != null) texturePanel.SetActive(false);
        }

        private void ApplyTexture(int textureIndex)
        {
            if (_lastEquippedInstance != null)
            {
                SkinnedMeshRenderer renderer = _lastEquippedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null && textureIndex >= 0 && textureIndex < availableTextures.Count)
                {
                    Texture2D newTexture = availableTextures[textureIndex].texture;
                    Material material = new Material(renderer.material);
                    material.SetTexture("_BaseMap", newTexture);
                    renderer.material = material;
                    _lastModifiedTexture = newTexture;
                }
            }
        }
    }
}