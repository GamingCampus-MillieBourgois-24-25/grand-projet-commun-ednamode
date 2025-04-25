using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EasyBattlePass
{
    public class PassTierProgressBar : MonoBehaviour
    {
        public Slider tierSliderBar;
        public int tierLevel;
        public TMP_Text textLevel;
        public GameObject lockedColor;
        public GameObject unlockedColor;
        public GameObject payProgressButton;


        private void Start()
        {
            if (textLevel != null)
                textLevel.text = tierLevel.ToString();
        }

        public void UnlockedTier()
        {
            lockedColor.SetActive(false);
            payProgressButton.SetActive(false);
            tierSliderBar.value = 1;
        }

        public void LockedTier()
        {
            lockedColor.SetActive(true);
            payProgressButton.SetActive(false);
            tierSliderBar.value = 0;
        }

        public void UnlockEffectsTier()
        {
            // Here you can set your own logic for some flare such as animations or effects to play when the player unlocks a new tier on the pass progress bar.
        }

    }
}
