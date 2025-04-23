using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EasyBattlePass
{
    public class RewardPopUp : MonoBehaviour
    {
        public GameObject popupWindow;
        public Image popupImage;
        public TMP_Text popupText;

        public void ShowPopup(Sprite image, string text)
        {
            popupWindow.SetActive(true);
            popupImage.sprite = image;
            popupText.text = text;
        }

        public void HidePopup()
        {
            popupWindow.SetActive(false);
        }
    }
}
