using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterCustomization
{
    public abstract class ShopButton : MonoBehaviour
    {
        protected ScriptableObject scriptable;
        protected string category;
        protected Image buttonImage;
        protected TextMeshProUGUI buttonText;
        public ItemSorter itemSorter;
        protected Button button;

        protected virtual void Start()
        {
            itemSorter = Object.FindFirstObjectByType<ItemSorter>();
            button = GetComponent<Button>();
            buttonImage = GetComponentInChildren<Image>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public string GetCategory()
        {
            return category;
        }

        public void SetScriptable(ScriptableObject scriptable)
        {
            this.scriptable = scriptable;
            Debug.Log($"Scriptable assigné : {scriptable?.name}, Type : {scriptable?.GetType()}");
        }

    }
}
