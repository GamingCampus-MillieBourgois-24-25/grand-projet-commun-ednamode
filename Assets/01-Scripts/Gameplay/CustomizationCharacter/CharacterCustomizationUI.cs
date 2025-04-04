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

        [Header("UI Configuration")]
        public GameObject characterPrefab;
        public SlotLibrary slotLibrary;
        public Camera mainCamera;

        [Header("Boutons d'action")]
        public Button buttonEdit;
        public Button buttonDelete;
        public Button buttonCustomize;

        [Header("Color Picker")]
        public ColorPicker colorPicker;

        [Header("Configuration des slots et des éléments UI")]
        public List<SlotUI> slotUIs;
        public GameObject buttonPrefab;
        public Sprite defaultSprite;

        private CharacterCustomization _characterCustomization;
        private SlotType _selectedSlotType;
        private GameObject _selectedInstance;
        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;

        private Dictionary<SlotType, (GameObject prefab, GameObject instance)> _equippedObjects = new();

        public void Initialize(CharacterCustomization characterCustomization)
        {
            _characterCustomization = characterCustomization;
            _originalCameraPosition = mainCamera.transform.position;
            _originalCameraRotation = mainCamera.transform.rotation;
            Debug.Log($"Caméra assignée : {mainCamera?.name}, Position : {_originalCameraPosition}");

            buttonEdit.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            buttonCustomize.gameObject.SetActive(false);

            PopulateUI();

            // Plus d’équipement par défaut ici
            // Les vêtements ne seront équipés que via l’UI (slotUIs)
        }

        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Debug.Log($"Touch détecté à la position : {touch.position}");
                if (touch.phase == TouchPhase.Began)
                {
                    HandleTouch(touch.position);
                }
            }
            else if (Input.GetMouseButtonDown(0)) // Test dans l’éditeur
            {
                Debug.Log($"Clic souris détecté à la position : {Input.mousePosition}");
                HandleTouch(Input.mousePosition);
            }
        }

        private void HandleTouch(Vector2 touchPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(touchPosition);
            Debug.Log($"Raycast lancé depuis : {touchPosition}");
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("UI")))
            {
                GameObject hitObject = hit.collider.gameObject;
                Debug.Log($"Objet touché : {hitObject.name}");
                SlotType? clickedSlotType = GetSlotTypeFromInstance(hitObject);

                if (clickedSlotType.HasValue)
                {
                    _selectedSlotType = clickedSlotType.Value;
                    _selectedInstance = hitObject;
                    Debug.Log($"Slot sélectionné : {_selectedSlotType}, Instance : {_selectedInstance.name}");
                    ZoomToObject(_selectedInstance);
                    ShowActionButtons();
                }
                else
                {
                    Debug.LogWarning("Aucun SlotType trouvé pour cet objet.");
                }
            }
            else
            {
                Debug.LogWarning("Raycast n’a rien touché.");
            }
        }

        private void ZoomToObject(GameObject target)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            Vector3 targetCenter = renderer != null ? renderer.bounds.center : target.transform.position;
            Vector3 direction = (targetCenter - mainCamera.transform.position).normalized;
            Vector3 zoomPosition = targetCenter - direction * 1.5f;
            mainCamera.transform.position = zoomPosition;
            mainCamera.transform.LookAt(targetCenter);
            Debug.Log($"Zoom sur {target.name}, Centre : {targetCenter}");
        }

        private void ResetCamera()
        {
            mainCamera.transform.position = _originalCameraPosition;
            mainCamera.transform.rotation = _originalCameraRotation;
            buttonEdit.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            buttonCustomize.gameObject.SetActive(false);
            _selectedInstance = null;
        }

        private void ShowActionButtons()
        {
            buttonEdit.gameObject.SetActive(true);
            buttonDelete.gameObject.SetActive(true);
            buttonCustomize.gameObject.SetActive(true);

            buttonEdit.onClick.RemoveAllListeners();
            buttonEdit.onClick.AddListener(() => OnEditClicked());

            buttonDelete.onClick.RemoveAllListeners();
            buttonDelete.onClick.AddListener(() => OnDeleteClicked());

            buttonCustomize.onClick.RemoveAllListeners();
            buttonCustomize.onClick.AddListener(() => OnCustomizeClicked());
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

        private void EquipPrefab(SlotType slotType, GameObject prefab)
        {
            var slot = _characterCustomization.Slots.FirstOrDefault(s => s.Type == slotType);
            if (slot != null)
            {
                if (_equippedObjects.ContainsKey(slotType))
                {
                    GameObject previousObject = _equippedObjects[slotType].instance;
                    if (previousObject != null) Destroy(previousObject);
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
                    Debug.Log($"Équipé : {slotType} avec instance {instance.name}, Total équipés : {_equippedObjects.Count}");
                }
                else
                {
                    Debug.LogWarning($"slot.Preview est null pour {slotType} avec prefab {prefab.name} !");
                }

                _characterCustomization.RefreshCustomization();
            }
        }

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

        private void OnEditClicked()
        {
            if (_selectedInstance != null)
            {
                Renderer renderer = _selectedInstance.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    Color currentColor = renderer.material.color;
                    ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                }
            }
        }

        private void OnDeleteClicked()
        {
            UnequipPrefab(_selectedSlotType);
            ResetCamera();
        }

        private void OnCustomizeClicked()
        {
            Debug.Log("Personnalisation à implémenter plus tard !");
        }

        private void OnColorChanged(Color color)
        {
            if (_selectedInstance != null)
            {
                Renderer renderer = _selectedInstance.GetComponentInChildren<Renderer>();
                if (renderer != null) renderer.material.color = color;
            }
        }

        private void OnColorSelected(Color color)
        {
            if (_selectedInstance != null)
            {
                Renderer renderer = _selectedInstance.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(renderer.material);
                    renderer.material.color = color;
                }
            }
            ResetCamera();
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