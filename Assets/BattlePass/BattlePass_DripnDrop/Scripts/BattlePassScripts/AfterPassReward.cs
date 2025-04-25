using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EasyBattlePass
{
    public class AfterPassReward : MonoBehaviour
    {
        // This is an optional script you can set your own logic to claim after the pass is completed rewards, these are examples

        public SimpleCurrencySystem.Currency[] rewardCurrencies;

        [SerializeField] private GameObject buttonClaim;
        [SerializeField] private GameObject lockedButton;


        public void UnlockAfterPassReward()
        {
            lockedButton.SetActive(false);
        }

        public void LockedAfterPassReward()
        {
            lockedButton.SetActive(true);
        }

        public void ClaimAfterPassRewardEffects()
        {
            // set your own logic for effects you'd like to happen here when the reward is claimed
        }
    }
}

