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
        public Button buttonChangeColor;

        [Header("Color Picker")]
        public ColorPicker colorPicker;

        [Header("Configuration des slots et des éléments UI")]
        public List<SlotUI> slotUIs;
        public GameObject prefabsPanel;
        public GameObject tagsPanel;
        public GameObject buttonPrefab;
        public Sprite defaultSprite;

        [Header("Peinture")]
        public Texture2D brushCursor;
        public int brushSize = 10;
        public Color brushColor = Color.black;

        private CharacterCustomization _characterCustomization;
        private SlotType _selectedSlotType;
        private GameObject _selectedInstance;
        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;
        private bool _isPaintingMode = false;
        private Texture2D _paintTexture;
        private SkinnedMeshRenderer _selectedRenderer; // Changé en SkinnedMeshRenderer

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
            buttonChangeColor.gameObject.SetActive(false);

            PopulateUI();
        }

        void Update()
        {
            Debug.Log($"Mode peinture actif : {_isPaintingMode}");
            if (_isPaintingMode)
            {
                if (Input.GetMouseButton(0))
                {
                    PaintOnClothing();
                }
            }
            else
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
                else if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log($"Clic souris détecté à la position : {Input.mousePosition}");
                    HandleTouch(Input.mousePosition);
                }
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
                    DisableUIPanels();
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
            SkinnedMeshRenderer renderer = target.GetComponentInChildren<SkinnedMeshRenderer>();
            Vector3 targetCenter = renderer != null ? renderer.bounds.center : target.transform.position;
            Vector3 direction = (targetCenter - mainCamera.transform.position).normalized;
            Vector3 zoomPosition = targetCenter - direction * 1f;
            mainCamera.transform.position = zoomPosition;
            mainCamera.transform.LookAt(targetCenter);
            Debug.Log($"Zoom sur {target.name}, Centre : {targetCenter}");
        }

        private void ResetCamera()
        {
            Debug.Log("ResetCamera appelé");
            mainCamera.transform.position = _originalCameraPosition;
            mainCamera.transform.rotation = _originalCameraRotation;
            buttonEdit.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            buttonCustomize.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);
            EnableUIPanels();
            _selectedInstance = null;
            _isPaintingMode = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            _paintTexture = null;
            _selectedRenderer = null;
        }

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
            Debug.Log("Événement Change Color assigné");
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
                    Debug.Log($"Equipped {slotType} with prefab {prefab.name}");
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
                ShowEditOptions();
            }
        }

        private void OnDeleteClicked()
        {
            UnequipPrefab(_selectedSlotType);
            ResetCamera();
        }

        private void OnCustomizeClicked()
        {
            if (_selectedInstance != null)
            {
                Debug.Log("Mode peinture activé !");
                _isPaintingMode = true;

                Cursor.SetCursor(brushCursor, new Vector2(brushSize / 2, brushSize / 2), CursorMode.Auto);

                _selectedRenderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (_selectedRenderer != null)
                {
                    _paintTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
                    _paintTexture.Clear(Color.white);
                    // Utiliser le matériau existant et assigner la nouvelle texture
                    Material currentMaterial = _selectedRenderer.material;
                    currentMaterial.SetTexture("_MainTex", _paintTexture); // "_MainTex" est standard pour la plupart des shaders
                    Debug.Log($"Texture assignée à _MainTex : {_paintTexture}, Matériau : {currentMaterial.name}, Shader : {currentMaterial.shader.name}");
                }
                else
                {
                    Debug.LogWarning("Aucun SkinnedMeshRenderer trouvé sur l’instance sélectionnée pour la peinture.");
                    _isPaintingMode = false;
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
        }

        private void OnChangeColorClicked()
        {
            Debug.Log("Bouton Change Color cliqué");
            if (_selectedInstance != null)
            {
                Debug.Log($"Instance sélectionnée : {_selectedInstance.name}");
                SkinnedMeshRenderer renderer = _selectedInstance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    Debug.Log($"Renderer trouvé : {renderer.name}, Couleur actuelle : {renderer.material.color}");
                    if (colorPicker == null)
                    {
                        Debug.LogError("ColorPicker n’est pas assigné dans l’inspecteur !");
                        return;
                    }
                    Color currentColor = renderer.material.color;
                    ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                    Debug.Log("ColorPicker.Create appelé");
                }
                else
                {
                    Debug.LogWarning("Aucun SkinnedMeshRenderer trouvé sur l’instance sélectionnée.");
                }
            }
            else
            {
                Debug.LogWarning("Aucune instance sélectionnée (_selectedInstance est null).");
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

        private void PaintOnClothing()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"Raycast touché : {hit.collider.gameObject.name}, Position : {hit.point}");
                if (hit.collider.gameObject == _selectedInstance)
                {
                    Vector2 uv = hit.textureCoord;
                    Debug.Log($"Peinture à UV : {uv}, Renderer : {_selectedRenderer.name}");

                    int x = (int)(uv.x * _paintTexture.width);
                    int y = (int)(uv.y * _paintTexture.height);
                    Debug.Log($"Coordonnées texture : ({x}, {y})");
                    float radius = brushSize / 2f;

                    for (int i = -50; i < 50; i++)
                    {
                        for (int j = -50; j < 50; j++)
                        {
                            int px = x + i;
                            int py = y + j;
                            if (px >= 0 && px < _paintTexture.width && py >= 0 && py < _paintTexture.height)
                            {
                                _paintTexture.SetPixel(px, py, brushColor);
                            }
                        }
                    }
                    _paintTexture.Apply();
                    Debug.Log("Texture mise à jour");
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

    public static class Texture2DExtensions
    {
        public static void Clear(this Texture2D texture, Color color)
        {
            Color[] pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
        }
    }
}