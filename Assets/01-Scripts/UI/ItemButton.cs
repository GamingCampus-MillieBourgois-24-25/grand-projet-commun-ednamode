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
        public void SetItem(Item item)
        {
            if (item == null)
            {
                Debug.LogError("SetItem a reçu un item null !");
                return;
            }

            scriptable = item;

            // Vérification de l'icône
            if (item.icon == null)
            {
                Debug.LogWarning($"L'item '{item.itemName}' n'a pas d'icône assignée !");
                buttonImage.sprite = null; // Vous pouvez définir une icône par défaut ici si nécessaire
            }
            else
            {
                buttonImage.sprite = item.icon;
            }

            // Vérification du nom
            if (string.IsNullOrEmpty(item.itemName))
            {
                Debug.LogWarning("Un item n'a pas de nom assigné !");
                buttonText.text = "Nom manquant"; // Texte par défaut si le nom est vide
            }
            else
            {
                buttonText.text = item.itemName;
            }
        }

    }
}
