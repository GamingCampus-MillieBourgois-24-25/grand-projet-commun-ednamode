using Unity.VisualScripting;
using UnityEngine;

namespace CharacterCustomization
{
    public class ItemButton : ShopButton
    {
        private ShoppingScript shoppingScript;
        private ItemEquipper itemEquipper;
        private CharacterItemManager characterItemManager;

        private ShopEquippedHandler shopEquippedHandler;

        protected override void Start()
        {
            base.Start();
            shoppingScript = Object.FindFirstObjectByType<ShoppingScript>();
            itemEquipper = Object.FindFirstObjectByType<ItemEquipper>();
            characterItemManager = Object.FindFirstObjectByType<CharacterItemManager>();
            shopEquippedHandler = Object.FindFirstObjectByType<ShopEquippedHandler>(); 

            if (scriptable is Item item)
            {
                category = item.category.ToString();
                buttonImage.sprite = item.icon;
                buttonText.text = item.price.ToString();
                button.onClick.AddListener(() => shoppingScript.SetSelectedItemButton(this));
                button.onClick.AddListener(() => itemEquipper.OnItemButtonClicked(item));
                button.onClick.AddListener(() => characterItemManager.EquipItem(item));
                button.onClick.AddListener(() => shopEquippedHandler.EquipItem(item)); 
            }
            
        }

        public Item GetItem()
        {
            if (scriptable is Item item)
            {
                if (item == null)
                {
                    return null;
                }
                return item;
            }
            else
            {
                return null;
            }
        }
        public void SetItem(Item item)
        {
            if (item == null)
            {
                return;
            }

            scriptable = item;

            if (item.icon == null)
            {
                buttonImage.sprite = null; 
            }
            else
            {
                if (buttonImage == null)
                {
                    return;
                }
                buttonImage.sprite = item.icon;
            }

            if (string.IsNullOrEmpty(item.itemName))
            {
                buttonText.text = "Nom manquant"; 
            }
            else
            {
                buttonText.text = item.itemName;
            }
        }

    }
}