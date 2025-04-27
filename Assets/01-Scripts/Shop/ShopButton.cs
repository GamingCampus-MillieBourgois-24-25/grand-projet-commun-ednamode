using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
            chooseCamPoint = Object.FindFirstObjectByType<ChooseCamPoint>();
            itemSorter = Object.FindFirstObjectByType<ItemSorter>();
            button = GetComponent<UnityEngine.UI.Button>();
            buttonImage = GetComponent<UnityEngine.UI.Image>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

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
                    chooseCamPoint.SwitchToCamPoint(camPointType);
                }
                else
                {
                    Debug.LogWarning($"Aucun CamPointType trouvé pour le SlotType : {slotCategory}");
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

    }
}
