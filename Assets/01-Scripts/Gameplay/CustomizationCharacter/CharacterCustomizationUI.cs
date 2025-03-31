using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

        [Header("UI des objets équipés")]
        public ScrollRect equippedItemsScrollView;
        public Transform equippedItemsContent;

        [Header("Boutons d'action")]
        public Button buttonSupprimer;
        public Button buttonChangeColor;

        [Header("Color Picker")]
        public ColorPicker colorPicker;

        [Header("Configuration des slots et des éléments UI")]
        public List<SlotUI> slotUIs;
        public GameObject buttonPrefab;
        public SlotLibrary slotLibrary;

        [Header("Fallback Sprite")]
        public Sprite defaultSprite; // Sprite par défaut si aucun thumbnail n’est trouvé

        private CharacterCustomization _characterGameObject;
        public GameObject characterPrefab;

        private Dictionary<SlotType, (GameObject prefab, GameObject instance)> _equippedObjects = new Dictionary<SlotType, (GameObject prefab, GameObject instance)>();
        private SlotType _selectedSlotType;

        public void Initialize(CharacterCustomization characterCustomization)
        {
            _characterGameObject = characterCustomization;
            PopulateUI();
            UpdateEquippedItemsUI();
        }

        private void PopulateUI()
        {
            foreach (var slotUI in slotUIs)
            {
                var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotUI.slotType);
                if (slot == null) continue;

                foreach (Transform child in slotUI.content)
                {
                    Destroy(child.gameObject);
                }

                foreach (var prefab in slot.GetAvailablePrefabs())
                {
                    GameObject buttonObj = Instantiate(buttonPrefab, slotUI.content);
                    var buttonImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>(); // Ou "Sprite"
                    var textMesh = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();

                    if (textMesh != null)
                    {
                        textMesh.gameObject.SetActive(false);
                    }

                    if (buttonImage != null)
                    {
                        ItemsSprite itemSprite = prefab.GetComponent<ItemsSprite>();
                        if (itemSprite == null)
                        {
                            Debug.LogWarning($"Prefab {prefab.name} n’a pas de composant ItemsSprite !");
                        }
                        else if (itemSprite.ItemSprite == null)
                        {
                            Debug.LogWarning($"Prefab {prefab.name} a un composant ItemsSprite mais aucun sprite assigné !");
                        }
                        Sprite sprite = itemSprite != null ? itemSprite.ItemSprite : defaultSprite;
                        Debug.Log($"Button {buttonObj.name} for prefab {prefab.name}: ItemSprite = {(itemSprite != null ? itemSprite.ItemSprite?.name : "null")}, Assigned Sprite = {sprite?.name}");
                        buttonImage.sprite = sprite;
                        buttonImage.preserveAspect = true;
                        buttonImage.color = Color.white;
                    }
                    else
                    {
                        Debug.LogWarning($"Button {buttonObj.name} n’a pas d’enfant nommé 'Icon' avec un composant Image !");
                    }

                    Button button = buttonObj.GetComponent<Button>();
                    button.onClick.AddListener(() => EquipPrefab(slotUI.slotType, prefab));
                }
            }
        }

        private void EquipPrefab(SlotType slotType, GameObject prefab)
        {
            var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotType);
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
                    instance.transform.SetParent(_characterGameObject.CharacterInstance.transform);
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    instance.SetActive(true);
                    _equippedObjects[slotType] = (prefab, instance); // Stocke le prefab original et l’instance
                    Debug.Log($"Equipped {slotType} with prefab {prefab.name}");
                }
                else
                {
                    Debug.LogWarning($"slot.Preview est null pour {slotType} avec prefab {prefab.name} !");
                }

                _characterGameObject.RefreshCustomization();
                UpdateEquippedItemsUI();
            }
        }

        private void UnequipPrefab(SlotType slotType)
        {
            if (_equippedObjects.ContainsKey(slotType))
            {
                GameObject equippedObject = _equippedObjects[slotType].instance;
                if (equippedObject != null)
                {
                    Destroy(equippedObject);
                }

                _equippedObjects.Remove(slotType);
                var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotType);
                if (slot != null)
                {
                    slot.Toggle(false);
                }

                UpdateEquippedItemsUI();
                buttonSupprimer.gameObject.SetActive(false);
                buttonChangeColor.gameObject.SetActive(false);
            }
        }

        private void OnChangeColor(SlotType slotType)
        {
            if (_equippedObjects.ContainsKey(slotType))
            {
                GameObject equippedObject = _equippedObjects[slotType].instance;
                Renderer renderer = equippedObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetTexture("_BaseMap", null);
                    Color currentColor = renderer.material.color;
                    ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                }
            }
        }

        private void OnColorChanged(Color color)
        {
            if (_equippedObjects.ContainsKey(_selectedSlotType))
            {
                GameObject equippedObject = _equippedObjects[_selectedSlotType].instance;
                Renderer renderer = equippedObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
        }

        private void OnColorSelected(Color color)
        {
            if (_equippedObjects.ContainsKey(_selectedSlotType))
            {
                GameObject equippedObject = _equippedObjects[_selectedSlotType].instance;
                Renderer renderer = equippedObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(renderer.material);
                    renderer.material.color = color;
                }
            }
            buttonSupprimer.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);
        }

        private void UpdateEquippedItemsUI()
        {
            foreach (Transform child in equippedItemsContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var equippedItem in _equippedObjects)
            {
                GameObject buttonObj = Instantiate(buttonPrefab, equippedItemsContent);
                if (buttonObj != null)
                {
                    var buttonImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>(); // Ou "Sprite" selon votre prefab
                    var textMesh = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();

                    if (textMesh != null)
                    {
                        textMesh.gameObject.SetActive(false);
                    }

                    if (buttonImage != null)
                    {
                        GameObject originalPrefab = equippedItem.Value.prefab; // Récupère directement le prefab original
                        ItemsSprite itemSprite = originalPrefab.GetComponent<ItemsSprite>();
                        if (itemSprite == null)
                        {
                            Debug.LogWarning($"Prefab {originalPrefab.name} n’a pas de composant ItemsSprite !");
                        }
                        else if (itemSprite.ItemSprite == null)
                        {
                            Debug.LogWarning($"Prefab {originalPrefab.name} a un composant ItemsSprite mais aucun sprite assigné !");
                        }
                        Sprite sprite = itemSprite != null ? itemSprite.ItemSprite : defaultSprite;
                        Debug.Log($"Equipped Button {buttonObj.name} for prefab {originalPrefab.name}: ItemSprite = {(itemSprite != null ? itemSprite.ItemSprite?.name : "null")}, Assigned Sprite = {sprite?.name}");
                        buttonImage.sprite = sprite;
                        buttonImage.preserveAspect = true;
                        buttonImage.color = Color.white;
                    }
                    else
                    {
                        Debug.LogWarning($"Equipped Button {buttonObj.name} n’a pas d’enfant nommé 'Icon' avec un composant Image !");
                    }

                    Button button = buttonObj.GetComponent<Button>();
                    button.onClick.AddListener(() => OnEquippedItemClicked(equippedItem.Key));
                }
            }
            buttonSupprimer.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);
        }

        private void OnEquippedItemClicked(SlotType slotType)
        {
            _selectedSlotType = slotType;
            buttonSupprimer.gameObject.SetActive(true);
            buttonChangeColor.gameObject.SetActive(true);

            buttonSupprimer.onClick.RemoveAllListeners();
            buttonSupprimer.onClick.AddListener(() => UnequipPrefab(_selectedSlotType));

            buttonChangeColor.onClick.RemoveAllListeners();
            buttonChangeColor.onClick.AddListener(() => OnChangeColor(_selectedSlotType));
        }
    }
}