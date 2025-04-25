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
                icon = transform.GetChild(0).GetComponent<Image>();
                var chooseCamPoint = Object.FindFirstObjectByType<ChooseCamPoint>();
                category = tab.category.ToString();
                buttonImage.sprite = defaultImg;
                icon.sprite = Resources.Load<Sprite>($"Tabs_icons/{tab.category}Icon");
                if(icon.sprite==null)
                {
                    Debug.LogWarning($"Aucun sprite trouvé pour le type de catégorie : {tab.category}");
                }
                else
                {
                    Debug.Log($"Sprite trouvé pour le type de catégorie : {tab.category}");
                }
                button.onClick.AddListener(() => itemSorter.SortItemsByCategory(category));
            }
        }
    }
}
