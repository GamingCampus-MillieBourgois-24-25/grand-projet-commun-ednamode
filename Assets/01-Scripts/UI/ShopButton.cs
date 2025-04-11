using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CharacterCustomization
{
    public class ShopButton : MonoBehaviour
    {
        private ScriptableObject scriptable;
        private string category;
        private Image buttonImage;
        private TextMeshProUGUI buttonText;
        public ItemSorter itemSorter;
        private Button button;
        private Dictionary<SlotType, ChooseCamPoint.CamPointType> TabToCamType;
        private void Start()
        {
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
    { SlotType.TShirt, ChooseCamPoint.CamPointType.Torso }
};

            itemSorter = Object.FindFirstObjectByType<ItemSorter>();
            button = GetComponent<Button>();

            if (scriptable is Item item)
            {
                category = item.category.ToString();

                buttonImage = GetComponentInChildren<Image>();
                buttonImage.sprite = item.icon;
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = item.price.ToString();
                button.onClick.AddListener(() => itemSorter.SortItemsByCategory(category));
            }
            else if (scriptable is Tab tab)
            {
                var chooseCamPoint = Object.FindFirstObjectByType<ChooseCamPoint>();
                category = tab.category.ToString();

                buttonImage = GetComponentInChildren<Image>();
                buttonImage.sprite = Resources.Load<Sprite>($"Tabs_icons/{tab.category}Icon");
                button.onClick.AddListener(() => itemSorter.SortItemsByCategory(category));

                button.onClick.AddListener(() =>
                {
                    if (TabToCamType.TryGetValue(tab.category, out var camPointType))
                    {
                        chooseCamPoint.SwitchToCamPoint(camPointType);
                    }
                    else
                    {
                        Debug.LogWarning($"Aucun CamPointType trouvé pour le SlotType : {tab.category}");
                    }
                });
            }
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
