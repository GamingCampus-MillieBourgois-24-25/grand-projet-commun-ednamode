using UnityEngine;

namespace CharacterCustomization
{
    public class ItemsSprite : MonoBehaviour
    {
        [SerializeField]
        private Sprite itemsprite;

        public Sprite ItemSprite => itemsprite;
    }
}