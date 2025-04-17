using UnityEngine;

namespace CharacterCustomization
{
    public class ItemButton : ShopButton
    {
        private ShoppingScript shoppingScript;
        protected override void Start()
        {
            base.Start();
            shoppingScript = Object.FindFirstObjectByType<ShoppingScript>();
            if (scriptable is Item item)
            {
                category = item.category.ToString();
                buttonImage.sprite = item.icon;
                buttonText.text = item.price.ToString();
                Debug.Log($"Ajout de l'event onClick pour l'item : {item.name}");
                button.onClick.AddListener(() => shoppingScript.SetSelectedItemButton(this));

            }
            else
            {
                Debug.LogError("Le scriptable n'est pas un Item !");
            }
        }
        public Item GetItem()
        {
            if (scriptable is Item item)
            {
                if(item == null)
                {
                    Debug.LogError("GetItem a reçu un item null !");
                    return null;
                }
                return item;
            }
            else
            {
                Debug.LogError("Le scriptable n'est pas un Item !");
                return null;
            }
        }
    }
}
