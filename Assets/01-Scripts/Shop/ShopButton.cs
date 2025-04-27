using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterCustomization
{
    public abstract class ShopButton : MonoBehaviour
    {
        protected ScriptableObject scriptable;
        protected string category;
        protected UnityEngine.UI.Image buttonImage;
        protected TextMeshProUGUI buttonText;
        public ItemSorter itemSorter;
        protected UnityEngine.UI.Button button;

        protected Dictionary<SlotType, ChooseCamPoint.CamPointType> TabToCamType;

        protected ChooseCamPoint chooseCamPoint;

        protected virtual void Start()
        {
            chooseCamPoint = UnityEngine.Object.FindFirstObjectByType<ChooseCamPoint>();
            itemSorter = UnityEngine.Object.FindFirstObjectByType<ItemSorter>();
            button = GetComponent<UnityEngine.UI.Button>();
            buttonImage = GetComponent<UnityEngine.UI.Image>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

            if (button == null)
            {
                Debug.LogError("[ShopButton] Bouton manquant sur ce GameObject !");
            }
            if (buttonText == null)
            {
                Debug.LogWarning("[ShopButton] TextMeshProUGUI manquant, le texte ne sera pas affiché.");
            }

            TabToCamType = new Dictionary<SlotType, ChooseCamPoint.CamPointType>
            {
                { SlotType.Accessories, ChooseCamPoint.CamPointType.Face },
                { SlotType.Body, ChooseCamPoint.CamPointType.Torso },
                { SlotType.Faces, ChooseCamPoint.CamPointType.Face },
                { SlotType.FullBody, ChooseCamPoint.CamPointType.FullBody },
                { SlotType.Glasses, ChooseCamPoint.CamPointType.Face },
                { SlotType.Gloves, ChooseCamPoint.CamPointType.Torso },
                { SlotType.Hairstyle, ChooseCamPoint.CamPointType.Face },
                { SlotType.Hat, ChooseCamPoint.CamPointType.Face },
                { SlotType.Mustache, ChooseCamPoint.CamPointType.Face },
                { SlotType.Outerwear, ChooseCamPoint.CamPointType.Torso },
                { SlotType.Pants, ChooseCamPoint.CamPointType.Legs },
                { SlotType.Shoes, ChooseCamPoint.CamPointType.Shoe },
            };

            button.onClick.AddListener(() =>
            {
                SlotType? slotCategory = null;

                if (scriptable is Item item)
                {
                    slotCategory = item.category;
                }
                else if (scriptable is Tab tab)
                {
                    slotCategory = tab.category;
                }

                if (slotCategory.HasValue && TabToCamType.TryGetValue(slotCategory.Value, out var camPointType))
                {
                    chooseCamPoint?.SwitchToCamPoint(camPointType);
                }
                else
                {
                    Debug.LogWarning($"[ShopButton] Aucun CamPointType trouvé pour le SlotType : {slotCategory}");
                }
            });
        }

        public string GetCategory()
        {
            return category;
        }

        public void SetScriptable(ScriptableObject scriptable)
        {
            this.scriptable = scriptable;
        }

        public void SetScriptable(ScriptableObject scriptable, Action onClickCallback)
        {
            this.scriptable = scriptable;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SlotType? slotCategory = null;

                if (scriptable is Item item)
                {
                    slotCategory = item.category;
                }
                else if (scriptable is Tab tab)
                {
                    slotCategory = tab.category;
                }

                if (slotCategory.HasValue && TabToCamType.TryGetValue(slotCategory.Value, out var camPointType))
                {
                    chooseCamPoint?.SwitchToCamPoint(camPointType);
                }
                else
                {
                    Debug.LogWarning($"[ShopButton] Aucun CamPointType trouvé pour le SlotType : {slotCategory}");
                }

                onClickCallback?.Invoke();
            });
        }
    }
}