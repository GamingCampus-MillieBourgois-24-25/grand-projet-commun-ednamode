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
                    Debug.LogError("GetItem a re�u un item null !");
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
        public void SetItem(Item item)
        {
            if (item == null)
            {
                Debug.LogError("SetItem a re�u un item null !");
                return;
            }

            scriptable = item;

            // V�rification de l'ic�ne
            if (item.icon == null)
            {
                Debug.LogWarning($"L'item '{item.itemName}' n'a pas d'ic�ne assign�e !");
                buttonImage.sprite = null; // Vous pouvez d�finir une ic�ne par d�faut ici si n�cessaire
            }
            else
            {
                buttonImage.sprite = item.icon;
            }

            // V�rification du nom
            if (string.IsNullOrEmpty(item.itemName))
            {
                Debug.LogWarning("Un item n'a pas de nom assign� !");
                buttonText.text = "Nom manquant"; // Texte par d�faut si le nom est vide
            }
            else
            {
                buttonText.text = item.itemName;
            }
        }

    }
}
