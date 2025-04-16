using System.Collections.Generic;
using UnityEngine;

namespace CharacterCustomization
{
    public class TabButton : ShopButton
    {

        protected override void Start()
        {
            base.Start();

            if (scriptable is Tab tab)
            {
                var chooseCamPoint = Object.FindFirstObjectByType<ChooseCamPoint>();
                category = tab.category.ToString();
                buttonImage.sprite = Resources.Load<Sprite>($"Tabs_icons/{tab.category}Icon");
                button.onClick.AddListener(() => itemSorter.SortItemsByCategory(category));
            }
        }
    }
}
