using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CharacterCustomization
{
    public class TabButton : ShopButton
    {
        [SerializeField] private Sprite defaultImg;
        private Image icon;
        protected override void Start()
        {
            base.Start();

            if (scriptable is Tab tab)
            {
                icon = GetComponentInChildren<Image>();
                var chooseCamPoint = Object.FindFirstObjectByType<ChooseCamPoint>();
                category = tab.category.ToString();
                buttonImage.sprite = defaultImg;
                Debug.Log(buttonImage != null ? "buttonImage trouvé !" : "buttonImage est null !");

                icon.sprite = Resources.Load<Sprite>($"Tabs_icons/{tab.category}Icon");
                button.onClick.AddListener(() => itemSorter.SortItemsByCategory(category));
            }
        }
    }
}
