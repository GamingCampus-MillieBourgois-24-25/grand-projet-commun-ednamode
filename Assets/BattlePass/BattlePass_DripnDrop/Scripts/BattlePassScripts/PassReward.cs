using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyBattlePass
{
    
    public class PassReward : MonoBehaviour
    {
        public enum PassRewardType { Free, Paid }

        public PassRewardType passRewardType;

        [SerializeField] private GameObject rewardHolder;
        public SimpleCurrencySystem.Currency[] passTierRewards;
        public Sprite Icon;
        public int itemAmount;
        public TMP_Text textAmount;
        [SerializeField] private GameObject claimButton;
        [SerializeField] private GameObject LockedBackground;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject claimedIcon;

        public bool claimed = false;
        public bool skipReward = false;

        private void Start()
        {
            textAmount.text = itemAmount.ToString();
            rewardHolder.SetActive(!skipReward);
        }

        public void Unlocked()
        {
            claimButton.SetActive(!claimed);
            LockedBackground?.SetActive(false);
            lockIcon?.SetActive(false);
            claimedIcon.SetActive(claimed);
        }

        public void Locked()
        {
            claimed = false;
            claimButton?.SetActive(false);
            lockIcon?.SetActive(true);
            LockedBackground?.SetActive(true);
            claimedIcon?.SetActive(false);
        }

        public void Claimed()
        {
            claimed = true;
            claimButton?.SetActive(false);
            claimedIcon.SetActive(claimed);
        }

        public void UnlockEffects()
        {
            // Here you can set your own logic for some flare or animations or effects to play when the player unlocks a new tier
        }
    }
}

