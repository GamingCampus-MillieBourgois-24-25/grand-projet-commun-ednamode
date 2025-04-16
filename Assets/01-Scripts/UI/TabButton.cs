using System.Collections.Generic;
using UnityEngine;

namespace CharacterCustomization
{
    public class TabButton : ShopButton
    {
        private Dictionary<SlotType, ChooseCamPoint.CamPointType> TabToCamType;

        protected override void Start()
        {
            base.Start();

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

            if (scriptable is Tab tab)
            {
                var chooseCamPoint = Object.FindFirstObjectByType<ChooseCamPoint>();
                category = tab.category.ToString();
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
    }
}
