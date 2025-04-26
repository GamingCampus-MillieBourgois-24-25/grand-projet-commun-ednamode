using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyBattlePass
{
    public class BattlePassManager : MonoBehaviour
    {
        [Header("-PASS UI ELEMENTS-")]
        //// the players progress for each level  
        [SerializeField] private Slider levelProgressBar;
        //// The current level text
        [SerializeField] private TMP_Text levelText;
        //// The extra level text 
        [SerializeField] private TMP_Text extraLevelText;
        //// The next level text
        [SerializeField] private TMP_Text nextLevelText;
        //// The current Xp
        [SerializeField] private TMP_Text xpText;
        //// Just an icon to show afteR the pass has been completed if after pass rewards enabled
        [SerializeField] private GameObject afterPassRewardIcon;
        //// The Ui element that will show when a player claims a reward
        public RewardPopUp rewardPopup;
        // The button for the paid version of the battle pass
        public GameObject paidVersionButton;
        // This will be disabled when the season ends
        public GameObject xpSliderInfo;
        // This is for if player is not connected to internet then disables the battle pass
        public GameObject passDataScrollRect;
        [SerializeField] PaginatedTiersScrollView tierScroller;

        [Header("-PASS DATA-")]
        //// The amount of XP required to reach the next level
        [SerializeField] private int[] xpToNextLevel;
        //// The amount of Xp required to reach the after pass rewards if enabled
        [SerializeField] private int xpToExtraReward = 500;

        /// <summary>
        //// Set the max level tier of free rewards and paid rewards, you will need to increase this value, 
        //// for each reward tier you add to each version of pass
        /// </summary>
        [SerializeField] private int maxLevelFree;
        [SerializeField] private int maxLevelPaid;
        // the progress bar of the battle pass for each Tier segmented
        [SerializeField] private PassTierProgressBar[] passTierProgressBar;
        // The rewards that the player can unlock/ Free and Paid
        [SerializeField] private PassReward[] freeRewardTiers;
        [SerializeField] private PassReward[] paidRewardTiers;
        //// The pass reward after its been completed/ optional
        public AfterPassReward afterPassReward;

        [Header("-CURRENCY SETTINGS-")]
        // the currency and amount used to buy the battle pass
        [SerializeField] private SimpleCurrencySystem.Currency passCurrency;
        //// the currency and amount used to skip tiers with
        [SerializeField] private SimpleCurrencySystem.Currency skipTierCurrency;

        // Accès au singleton DataSaver pour toutes les opérations de coins/jewels
        private DataSaver dataSaver => DataSaver.Instance;

        // Whether the paid battle pass has been unlocked
        [SerializeField] private bool paidUnlocked;

        [Header("-OPTIONAL FEATURES-")]
        [SerializeField] private bool hasAfterPassRewards;
        [SerializeField] private bool canSkipTier;

        [HideInInspector] public bool seasonOffline;
        [HideInInspector] public bool reset;
        [HideInInspector] public bool seasonEndedPass;

        // The player's current pass level
        private int level = 0;
        // The player's next level
        private int nextLevel = 1;
        // The players's level after the pass is completed for paid version
        private int extraLevel = 0;
        // The max after pass rewards the player can unlock at once
        private int maxExtraLevel = 1;
        // The player's current XP
        private int xp;
        // The players's total XP gained
        private int totalXp;
        private bool buyingTier;

        /// <summary>
        //// A dictionary to store the claimed status of rewards
        /// </summary>
        Dictionary<PassReward, bool> freeClaimedRewards = new Dictionary<PassReward, bool>();
        Dictionary<PassReward, bool> paidClaimedRewards = new Dictionary<PassReward, bool>();

        void Start()
        {
            // Load the battle pass data
            Load();
            // Unlock all the rewards already unlocked
            UnlockRewards();
            // Load all of the claimed rewards
            LoadFreeClaimedRewards();
            LoadPaidClaimedRewards();
            // Update the UI to show current position in the battle pass
            UpdateUI();
        }

        // Just if needed to call in other scripts
        public void LoadPass()
        {
            Load();
        }

        // Adding Xp method
        public void AddXP(int amount)
        {
            if (seasonEndedPass || seasonOffline)
                return;

            if (level == maxLevelPaid && paidUnlocked)
            {
                if (hasAfterPassRewards)
                    AddXpAfterPass(amount);
            }
            else if (level != maxLevelFree || paidUnlocked)
            {
                xp += buyingTier ? amount : amount;
                buyingTier = false;
            }

            totalXp += amount;

            while (xp >= xpToNextLevel[level] && level != maxLevelPaid)
            {
                xp -= xpToNextLevel[level];
                if ((level != maxLevelFree && !paidUnlocked) || (paidUnlocked && level != maxLevelPaid))
                    level++;
                if (level == maxLevelPaid && !hasAfterPassRewards)
                    xp = 0;
                nextLevel = (nextLevel == maxLevelPaid) ? maxLevelPaid : nextLevel + 1;
                UpdateUI();
                UnlockRewards();
            }

            UpdateUI();
            Save();
        }

        private void AddXpAfterPass(int amount)
        {
            if (extraLevel < maxExtraLevel)
                xp += amount;
            if (xp >= xpToExtraReward)
            {
                xp = 0;
                extraLevel++;
                afterPassReward.UnlockAfterPassReward();
            }
        }

        // Purchase the paid battle pass
        public void PurchasePaidVersion()
        {
            if (EncryptionManager.LoadInt("SeasonEnded", 0) != 1)
            {
                bool canPay = passCurrency.name == "Coins"
                    ? dataSaver.dts.totalCoins >= passCurrency.amount
                    : dataSaver.dts.totalJewels >= passCurrency.amount;
                if (canPay)
                {
                    if (passCurrency.name == "Coins") dataSaver.removeCoins(passCurrency.amount);
                    else dataSaver.removeJewels(passCurrency.amount);
                    EncryptionManager.SaveInt("_paidVersion", 1);
                    paidVersionButton.SetActive(false);
                    paidUnlocked = true;
                    UnlockPaidRewards();
                }
                else Debug.Log("Not enough " + passCurrency.name);
            }
            Load();
        }

        private void UnlockRewards()
        {
            if (paidUnlocked)
            {
                UnlockPaidRewards();
                UnlockFreeRewards();
            }
            else UnlockFreeRewards();
        }

        private void UnlockPaidRewards()
        {
            for (int i = 0; i < paidRewardTiers.Length; i++)
                if (i <= level) { paidRewardTiers[i].Unlocked(); paidRewardTiers[i].UnlockEffects(); }
        }

        private void UnlockFreeRewards()
        {
            for (int i = 0; i < freeRewardTiers.Length; i++)
                if (i <= level) { freeRewardTiers[i].Unlocked(); freeRewardTiers[i].UnlockEffects(); }
            tierScroller?.ScrollToSection(level == 0 ? level : level - 1);
        }

        public void UnlockNextTier()
        {
            if (EncryptionManager.LoadInt("SeasonEnded", 0) != 1)
            {
                bool canSkip = skipTierCurrency.name == "Coins"
                    ? dataSaver.dts.totalCoins >= skipTierCurrency.amount
                    : dataSaver.dts.totalJewels >= skipTierCurrency.amount;
                if (canSkip)
                {
                    if (skipTierCurrency.name == "Coins") dataSaver.removeCoins(skipTierCurrency.amount);
                    else dataSaver.removeJewels(skipTierCurrency.amount);
                    buyingTier = true;
                    AddXP(xpToNextLevel[level] - xp);
                }
                else Debug.Log("Not enough " + skipTierCurrency.name);
            }
        }

        private void UpdateUI()
        {
            StartCoroutine(AnimateProgressBar());
            StartCoroutine(AnimatePassProgressBar());
            levelText.text = level.ToString();
            extraLevelText.text = extraLevel.ToString();
            bool showIcon = level == maxLevelPaid && hasAfterPassRewards;
            afterPassRewardIcon.SetActive(showIcon);
            if (level == maxLevelPaid)
            {
                xpText.text = showIcon ? $"{xp}/{xpToExtraReward}" : "0/0";
                nextLevelText.text = "";
            }
            else
            {
                xpText.text = $"{xp}/{xpToNextLevel[level]}";
                nextLevelText.text = nextLevel.ToString();
            }
        }

        IEnumerator AnimateProgressBar()
        {
            float t = 0f;
            const float dur = 1f;
            while (t < dur)
            {
                t += Time.deltaTime;
                levelProgressBar.value = Mathf.Lerp(levelProgressBar.value, (float)xp / xpToNextLevel[level], t / dur);
                yield return null;
            }
        }

        IEnumerator AnimatePassProgressBar()
        {
            float t = 0f;
            const float dur = 1f;
            while (t < dur)
            {
                t += Time.deltaTime;
                PassTierProgressBar bar = passTierProgressBar[level];
                bool showBtn = canSkipTier && ((level != maxLevelFree && !paidUnlocked) || (level != maxLevelPaid && paidUnlocked));
                if (level > 0) { var prev = passTierProgressBar[level - 1]; prev.UnlockedTier(); prev.UnlockEffectsTier(); }
                bar.payProgressButton.SetActive(showBtn);
                bar.lockedColor.SetActive(false);
                bar.tierSliderBar.value = Mathf.Lerp(bar.tierSliderBar.value, (float)xp / xpToNextLevel[level], t / dur);
                if (seasonEndedPass) bar.payProgressButton.SetActive(false);
                yield return null;
            }
        }

        public void ClaimAfterPassReward()
        {
            if (extraLevel != maxExtraLevel) return;
            afterPassReward.ClaimAfterPassRewardEffects();
            extraLevel = 0;
            int idx = UnityEngine.Random.Range(0, afterPassReward.rewardCurrencies.Length);
            var r = afterPassReward.rewardCurrencies[idx];
            rewardPopup.ShowPopup(r.icon, r.amount.ToString());
            if (r.name == "Coins") dataSaver.addCoins(r.amount); else dataSaver.addJewels(r.amount);
            Save(); Load();
        }

        public void ClaimFreeReward(PassReward reward)
        {
            if (!freeClaimedRewards.TryGetValue(reward, out bool cl) || cl) return;
            rewardPopup.ShowPopup(reward.Icon, reward.itemAmount.ToString());
            foreach (var tr in reward.passTierRewards)
                if (tr.name == "Coins") dataSaver.addCoins(tr.amount); else dataSaver.addJewels(tr.amount);
            freeClaimedRewards[reward] = true; reward.Claimed(); SaveFreeClaimedRewards();
        }

        public void ClaimPaidReward(PassReward reward)
        {
            if (!paidClaimedRewards.TryGetValue(reward, out bool cl) || cl) return;
            rewardPopup.ShowPopup(reward.Icon, reward.itemAmount.ToString());
            foreach (var tr in reward.passTierRewards)
                if (tr.name == "Coins") dataSaver.addCoins(tr.amount); else dataSaver.addJewels(tr.amount);
            paidClaimedRewards[reward] = true; reward.Claimed(); SavePaidClaimedRewards();
        }

        private void SaveFreeClaimedRewards() { foreach (var kv in freeClaimedRewards) EncryptionManager.SaveInt(kv.Key.name + "_freeClaimed", kv.Value ? 1 : 0); }
        private void SavePaidClaimedRewards() { foreach (var kv in paidClaimedRewards) EncryptionManager.SaveInt(kv.Key.name + "_paidClaimed", kv.Value ? 1 : 0); }
        private void LoadFreeClaimedRewards() { for (int i = 0; i < freeRewardTiers.Length; i++) { int c = EncryptionManager.LoadInt(freeRewardTiers[i].name + "_freeClaimed", 0); bool cl = c == 1; freeClaimedRewards.Add(freeRewardTiers[i], cl); if (cl) freeRewardTiers[i].Claimed(); } }
        private void LoadPaidClaimedRewards() { for (int i = 0; i < paidRewardTiers.Length; i++) { int c = EncryptionManager.LoadInt(paidRewardTiers[i].name + "_paidClaimed", 0); bool cl = c == 1; paidClaimedRewards.Add(paidRewardTiers[i], cl); if (cl) paidRewardTiers[i].Claimed(); } }
        private void ResetFreeClaimedRewards() { for (int i = 0; i < freeRewardTiers.Length; i++) { EncryptionManager.SaveInt(freeRewardTiers[i].name + "_freeClaimed", 0); freeClaimedRewards[freeRewardTiers[i]] = false; } }
        private void ResetPaidClaimedRewards() { for (int i = 0; i < paidRewardTiers.Length; i++) { EncryptionManager.SaveInt(paidRewardTiers[i].name + "_paidClaimed", 0); paidClaimedRewards[paidRewardTiers[i]] = false; } }

        // Reset the entire Battle Pass, call this when the season starts over
        public void ResetPass()
        {
            // Reset pass progression
            level = 0;
            nextLevel = 1;
            extraLevel = 0;
            xp = 0;
            totalXp = 0;

            // Cancel paid pass purchase
            EncryptionManager.SaveInt("_paidVersion", 0);
            paidUnlocked = false;

            xpSliderInfo.SetActive(true);
            seasonEndedPass = false;

            // Reset free tiers: unlock only up to current level (0)
            for (int i = 0; i < freeRewardTiers.Length; i++)
            {
                freeRewardTiers[i].claimed = false;
                if (i <= level)
                    freeRewardTiers[i].Unlocked();
                else
                    freeRewardTiers[i].Locked();
            }

            // Lock all paid tiers
            for (int i = 0; i < paidRewardTiers.Length; i++)
            {
                paidRewardTiers[i].claimed = false;
                paidRewardTiers[i].Locked();
            }

            ResetFreeClaimedRewards();
            ResetPaidClaimedRewards();

            if (tierScroller != null)
                tierScroller.ScrollToSection(level);

            reset = true;

            // Refresh save and load to apply UI changes
            Save();
            Load();
        }

        public void EndPassSeason() { seasonEndedPass = true; Load(); }
        public void PassOffline() { paidVersionButton.SetActive(false); passDataScrollRect.SetActive(false); xpSliderInfo.SetActive(false); }
        public void PassOnline() { paidVersionButton.SetActive(true); passDataScrollRect.SetActive(true); xpSliderInfo.SetActive(true); Load(); }
        public void ResetPassSeason() { ResetPass(); }

        private void Save() { EncryptionManager.SaveInt("currentLevel", level); EncryptionManager.SaveInt("nextLevel", nextLevel); EncryptionManager.SaveInt("currentXP", xp); EncryptionManager.SaveInt("totalXP", totalXp); EncryptionManager.SaveInt("extraLevel", extraLevel); if (reset) Load(); }
        private void Load()
        {
            level = EncryptionManager.LoadInt("currentLevel", level);
            nextLevel = EncryptionManager.LoadInt("nextLevel", nextLevel);
            extraLevel = EncryptionManager.LoadInt("extraLevel", extraLevel);
            xp = EncryptionManager.LoadInt("currentXP", xp);
            totalXp = EncryptionManager.LoadInt("totalXP", totalXp);
            tierScroller?.ScrollToSection(level == 0 ? level : level - 1);
            int pv = EncryptionManager.LoadInt("_paidVersion", 0); paidUnlocked = (pv == 1); paidVersionButton.SetActive(pv == 0);
            if (seasonEndedPass || seasonOffline) { paidVersionButton.SetActive(false); xpSliderInfo.SetActive(false); }
            afterPassReward.gameObject.SetActive(hasAfterPassRewards);
            for (int i = 0; i < passTierProgressBar.Length; i++)
            {
                if (i < level) passTierProgressBar[i].UnlockedTier(); else passTierProgressBar[i].LockedTier();
                bool showBtn = canSkipTier && ((level != maxLevelFree && !paidUnlocked) || (level != maxLevelPaid && paidUnlocked));
                if (i == level) { passTierProgressBar[i].payProgressButton.SetActive(showBtn); passTierProgressBar[i].lockedColor.SetActive(!showBtn); }
                if (seasonEndedPass) passTierProgressBar[i].payProgressButton.SetActive(false);
            }
            if (extraLevel == 0) afterPassReward.LockedAfterPassReward(); else afterPassReward.UnlockAfterPassReward(); if (reset) reset = false; UpdateUI();
        }
    }
}
