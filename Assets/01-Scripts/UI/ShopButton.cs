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

        private void Start()
        {
            itemSorter = Object.FindFirstObjectByType<ItemSorter>();
            button = GetComponent<Button>();
            if (scriptable is Item item)
            {
                category = item.category.ToString();

                buttonImage = GetComponentInChildren<Image>();
                buttonImage.sprite = item.icon;
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = item.price.ToString();
            }
            else if (scriptable is Tab tab)
            {
                category = tab.category.ToString();

                buttonImage = GetComponentInChildren<Image>();
                buttonImage.sprite = Resources.Load<Sprite>($"Tabs_icons/{tab.category}Icon");
                button.onClick.AddListener(() => itemSorter.SortItemsByCategory(category));
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
