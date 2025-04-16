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
                button.onClick.AddListener(() => shoppingScript.SetSelectedItem(item));

            }
            else
            {
                Debug.LogError("Le scriptable n'est pas un Item !");
            }
        }
    }
}
