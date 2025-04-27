using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CharacterCustomization
{
    public class ItemButton : ShopButton
    {
        private ShoppingScript shoppingScript;
        private ItemEquipper itemEquipper;
        private CharacterItemManager characterItemManager;

        protected override void Start()
        {
            base.Start();
            shoppingScript = UnityEngine.Object.FindFirstObjectByType<ShoppingScript>();
            itemEquipper = UnityEngine.Object.FindFirstObjectByType<ItemEquipper>();
            characterItemManager = UnityEngine.Object.FindFirstObjectByType<CharacterItemManager>();

            if (characterItemManager == null)
            {
                Debug.LogError("[ItemButton] CharacterItemManager introuvable dans la scène !");
                return;
            }
        }

        /*public override void SetScriptable(ScriptableObject scriptable, System.Action onClickCallback)
        {
            base.SetScriptable(scriptable, onClickCallback);

            if (scriptable == null)
            {
                Debug.LogError($"[ItemButton] Scriptable est null sur {gameObject.name} !");
                return;
            }

            if (scriptable is Item item)
            {
                Debug.Log($"[ItemButton] Configuration de l'item {item.name} sur {gameObject.name}");
                ConfigureButton(item);
            }
            else
            {
                Debug.LogError($"[ItemButton] Le scriptable n'est pas un Item sur {gameObject.name}, type reçu : {scriptable.GetType().Name} !");
            }
        }
*/
        public void SetItem(Item item)
        {
            if (item == null)
            {
                Debug.LogError("[ItemButton] SetItem a reçu un item null !");
                return;
            }

            scriptable = item;
            Debug.Log($"[ItemButton] SetItem pour {item.name} sur {gameObject.name}");
            ConfigureButton(item);
        }

        private void ConfigureButton(Item item)
        {
            category = item.category.ToString();

            if (item.icon == null)
            {
                Debug.LogWarning($"[ItemButton] L'item '{item.itemName}' n'a pas d'icône assignée !");
                buttonImage.sprite = null;
            }
            else
            {
                if (buttonImage == null)
                {
                    Debug.LogError("[ItemButton] Le composant Image n'a pas été trouvé !");
                    return;
                }
                buttonImage.sprite = item.icon;
            }

            if (buttonText == null)
            {
                Debug.LogError("[ItemButton] Le composant TextMeshProUGUI n'a pas été trouvé !");
                return;
            }
            buttonText.text = item.price.ToString();

            if (button == null)
            {
                Debug.LogError("[ItemButton] Le composant Button n'a pas été trouvé !");
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                Debug.Log($"[ItemButton] Clic sur {item.itemName}");
                shoppingScript?.SetSelectedItemButton(this);
                characterItemManager.EquipSingleItemForShop(item);
            });
        }

        public Item GetItem()
        {
            if (scriptable is Item item)
            {
                if (item == null)
                {
                    Debug.LogError("[ItemButton] GetItem a reçu un item null !");
                    return null;
                }
                return item;
            }
            else
            {
                Debug.LogError("[ItemButton] Le scriptable n'est pas un Item !");
                return null;
            }
        }
    }
}