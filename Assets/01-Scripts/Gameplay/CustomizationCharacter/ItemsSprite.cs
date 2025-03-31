using UnityEngine;
using System.Collections.Generic;

namespace CharacterCustomization
{
    public class ItemsSprite : MonoBehaviour
    {
        [SerializeField]
        private Sprite itemsprite;

        [SerializeField]
        private List<string> tags = new List<string>(); 

        public Sprite ItemSprite => itemsprite;
        public List<string> Tags => tags; 
    }
}