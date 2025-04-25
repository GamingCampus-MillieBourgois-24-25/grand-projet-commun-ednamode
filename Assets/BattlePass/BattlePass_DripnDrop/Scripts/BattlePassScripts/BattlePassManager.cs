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

        // the currency and amount used to buy the battle pass
        [SerializeField] private SimpleCurrencySystem.Currency passCurrency;
        //// the currency and amount used to skip tiers with
        [SerializeField] private SimpleCurrencySystem.Currency skipTierCurrency;

        // Accès au singleton DataSaver pour toutes les opérations de coins/jewels
        private DataSaver dataSaver => DataSaver.Instance;

        // Whether the paid battle pass has been unlocked // Serializd to show when the pass has already been bought in inspector
        [SerializeField] private bool paidUnlocked;

        [Header("-OPTIONAL FEATURES-")]
        [SerializeField] private bool hasAfterPassRewards;
        [SerializeField] private bool canSkipTier;

        // This is to check if the players device isn't connected to the internet and if so they cant progress the BattlePass/ To prevent time cheating
        [HideInInspector] public bool seasonOffline;

        [HideInInspector] public bool reset;
        [HideInInspector] public bool seasonEndedPass;
        [HideInInspector] public bool xpBooster;
        [HideInInspector] public int xpBoostMultiplier;

        // The player's current pass level
        private int level = 0;
        // The player's next level
        private int nextLevel = 1;
        // The players's level after the pass is completed for paid version
        private int extraLevel = 0;
        // The max after pass rewards the player can unlock at once before claiming them, can change this to as much as you want for them to stack
        private int maxExtraLevel = 1;
        // The player's current XP
        private int xp;
        // The players's total XP gained, optional value if you want to use for anything.
        private int totalXp;

        private bool buyingTier;

        /// <summary>
        //// A dictionary to store the claimed status of rewards so that they can't be claimed twice. This can be changed for your own method, 
        //// just change the saving and loding method of these as well
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

        //Just if needed to call in other scripts or when diasabling the battle pass canvas and then enabling it again.
        public void LoadPass()
        {
            Load();
        }

        // Adding Xp method
        public void AddXP(int amount)
        {
            if (seasonEndedPass || seasonOffline)
            {
                return;
            }

            int boostedXP = xpBooster ? amount * xpBoostMultiplier : amount;

            if (level == maxLevelPaid && paidUnlocked)
            {
                if (hasAfterPassRewards)
                {
                    AddXpAfterPass(boostedXP);
                }
            }
            else if (level != maxLevelFree || paidUnlocked)
            {
                xp += buyingTier ? amount : boostedXP;
                buyingTier = false;
            }

            totalXp += amount;

            while (xp >= xpToNextLevel[level] && level != maxLevelPaid)
            {
                xp -= xpToNextLevel[level];

                if ((level != maxLevelFree && !paidUnlocked) || (paidUnlocked && level != maxLevelPaid))
                {
                    level++;
                }

                if (level == maxLevelPaid && !hasAfterPassRewards)
                {
                    xp = 0;
                }

                nextLevel = (nextLevel == maxLevelPaid) ? maxLevelPaid : nextLevel + 1;

                UpdateUI();
                UnlockRewards();
            }

            UpdateUI();
            Save();
        }

        // Method to add xp for the extra rewards after the pass has been completed
        private void AddXpAfterPass(int amount)
        {
            if (extraLevel == maxExtraLevel)
                xp += 0;
            else
                xp += amount;

            // if the xp has reached the amount needed for the extra reward then set to 0/ can change this to stack rewards 
            if (xp >= xpToExtraReward)
            {
                xp = 0;
                extraLevel++;

                UnlockAfterPassReward();
            }
        }

        // Method to purchase the paid battle pass
        public void PurchasePaidVersion()
        {
            // check if the season has ended, if so then do not allow to buy
            if (EncryptionManager.LoadInt("SeasonEnded", 0) != 1)
            {
                bool canPay = (passCurrency.name == "Coins")
                    ? dataSaver.dts.totalCoins >= passCurrency.amount
                    : dataSaver.dts.totalJewels >= passCurrency.amount;

                if (canPay)
                {
                    if (passCurrency.name == "Coins")
                        dataSaver.removeCoins(passCurrency.amount);
                    else
                        dataSaver.removeJewels(passCurrency.amount);

                    // Set the paid version if bought
                    EncryptionManager.SaveInt("_paidVersion", 1);

                    // Set the paid version button to inactive
                    paidVersionButton.SetActive(false);
                    paidUnlocked = true;
                    UnlockPaidRewards();
                }
                else
                {
                    Debug.Log("Not enough " + passCurrency.name + " to spend.");
                }
            }

            Load();
        }

        // Unlock the rewards for the current level
        private void UnlockRewards()
        {
            // If the paid battle pass has been unlocked
            if (paidUnlocked)
            {
                // Unlock the rewards for the current level in the paid battle pass
                UnlockPaidRewards();
                UnlockFreeRewards();
            }
            // If the player is only using the free battle pass
            else
            {
                // Unlock the rewards for the current level in the free battle pass
                UnlockFreeRewards();
            }
        }

        // Unlock the rewards for the current level in the paid battle pass
        private void UnlockPaidRewards()
        {
            for (int i = 0; i < paidRewardTiers.Length; i++)
            {
                if (i <= level)
                {
                    paidRewardTiers[i].Unlocked();
                    paidRewardTiers[i].UnlockEffects();
                }
            }
        }

        // Unlock the rewards for the current level in the free battle pass
        private void UnlockFreeRewards()
        {
            for (int i = 0; i < freeRewardTiers.Length; i++)
            {
                if (i <= level)
                {
                    freeRewardTiers[i].Unlocked();
                    freeRewardTiers[i].UnlockEffects();
                }
            }

            if (tierScroller != null)
                tierScroller.ScrollToSection(level == 0 ? level : (level > 0 && level != maxLevelPaid ? level : level - 1));
        }

        // Unlock the extra rewards after the pass is completed
        private void UnlockAfterPassReward()
        {
            afterPassReward.UnlockAfterPassReward();
        }

        // Skip a tier called from other scripts
        public void UnlockNextTier()
        {
            if (EncryptionManager.LoadInt("SeasonEnded", 0) != 1)
            {
                bool canSkip = (skipTierCurrency.name == "Coins")
                    ? dataSaver.dts.totalCoins >= skipTierCurrency.amount
                    : dataSaver.dts.totalJewels >= skipTierCurrency.amount;

                if (canSkip)
                {
                    if (skipTierCurrency.name == "Coins")
                        dataSaver.removeCoins(skipTierCurrency.amount);
                    else
                        dataSaver.removeJewels(skipTierCurrency.amount);

                    buyingTier = true;
                    var gainXp = xpToNextLevel[level] - xp;
                    AddXP(gainXp);
                }
                else
                {
                    Debug.Log("Not enough " + skipTierCurrency.name + " to spend.");
                }
            }
        }

        // Update the UI
        private void UpdateUI()
        {
            StartCoroutine(AnimateProgressBar());
            StartCoroutine(AnimatePassProgressBar());

            if (levelText != null)
                levelText.text = level.ToString();

            if (extraLevelText != null)
                extraLevelText.text = extraLevel.ToString();

            bool showAfterPassRewardIcon = level == maxLevelPaid && hasAfterPassRewards;
            if (afterPassRewardIcon != null)
                afterPassRewardIcon.SetActive(showAfterPassRewardIcon);

            if (level == maxLevelPaid)
            {
                if (xpText != null)
                    xpText.text = showAfterPassRewardIcon ? $"{xp} / {xpToExtraReward}" : "0 / 0";

                if (nextLevelText != null)
                    nextLevelText.text = "";
            }
            else
            {
                if (xpText != null)
                    xpText.text = $"{xp} / {xpToNextLevel[level]}";

                if (nextLevelText != null)
                    nextLevelText.text = nextLevel.ToString();
            }
        }

        IEnumerator AnimateProgressBar()
        {
            float time = 0f;
            float duration = 1f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                levelProgressBar.value = Mathf.Lerp(levelProgressBar.value, (float)xpToNextLevel[level] == 0 ? 0 : (float)xp / xpToNextLevel[level], t);
                yield return null;
            }
        }

        IEnumerator AnimatePassProgressBar()
        {
            float time = 0f;
            float duration = 1f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                PassTierProgressBar currentTier = passTierProgressBar[level];
                bool shouldActivatePayProgressButton = canSkipTier && ((level != maxLevelFree && !paidUnlocked) || (level != maxLevelPaid && paidUnlocked));

                if (level - 1 >= 0)
                {
                    passTierProgressBar[level - 1].UnlockedTier();
                    passTierProgressBar[level - 1].UnlockEffectsTier();
                }

                currentTier.payProgressButton.SetActive(shouldActivatePayProgressButton);
                currentTier.lockedColor.SetActive(false);
                currentTier.tierSliderBar.value = Mathf.Lerp(currentTier.tierSliderBar.value, (float)xpToNextLevel[level] == 0 ? 0 : (float)xp / xpToNextLevel[level], t);

                if (seasonEndedPass)
                    currentTier.payProgressButton.SetActive(false);

                yield return null;
            }
        }

        public void ClaimAfterPassReward()
        {
            if (extraLevel != maxExtraLevel)
                return;

            afterPassReward.ClaimAfterPassRewardEffects();
            extraLevel = 0;

            int randomReward = UnityEngine.Random.Range(0, afterPassReward.rewardCurrencies.Length);
            var r = afterPassReward.rewardCurrencies[randomReward];
            rewardPopup.ShowPopup(r.icon, r.amount.ToString());

            if (r.name == "Coins")
                dataSaver.addCoins(r.amount);
            else if (r.name == "Gems")
                dataSaver.addJewels(r.amount);

            Save();
            Load();
        }

        public void ClaimFreeReward(PassReward reward)
        {
            if (!freeClaimedRewards.TryGetValue(reward, out bool isClaimed) || isClaimed)
                return;

            rewardPopup.ShowPopup(reward.Icon, reward.itemAmount.ToString());
            foreach (var tierReward in reward.passTierRewards)
            {
                if (tierReward.name == "Coins")
                    dataSaver.addCoins(tierReward.amount);
                else if (tierReward.name == "Gems")
                    dataSaver.addJewels(tierReward.amount);
            }

            freeClaimedRewards[reward] = true;
            reward.Claimed();
            SaveFreeClaimedRewards();
        }

        public void ClaimPaidReward(PassReward reward)
        {
            if (!paidClaimedRewards.TryGetValue(reward, out bool isClaimed) || isClaimed)
                return;

            rewardPopup.ShowPopup(reward.Icon, reward.itemAmount.ToString());
            foreach (var tierReward in reward.passTierRewards)
            {
                if (tierReward.name == "Coins")
                    dataSaver.addCoins(tierReward.amount);
                else if (tierReward.name == "Gems")
                    dataSaver.addJewels(tierReward.amount);
            }

            paidClaimedRewards[reward] = true;
            reward.Claimed();
            SavePaidClaimedRewards();
        }

        private void SaveFreeClaimedRewards()
        {
            foreach (KeyValuePair<PassReward, bool> reward in freeClaimedRewards)
                EncryptionManager.SaveInt(reward.Key.name + "_freeClaimed", reward.Value ? 1 : 0);
        }

        private void SavePaidClaimedRewards()
        {
            foreach (KeyValuePair<PassReward, bool> reward in paidClaimedRewards)
                EncryptionManager.SaveInt(reward.Key.name + "_paidClaimed", reward.Value ? 1 : 0);
        }

        private void LoadFreeClaimedRewards()
        {
            for (int i = 0; i < freeRewardTiers.Length; i++)
            {
                int claimed = EncryptionManager.LoadInt(freeRewardTiers[i].name + "_freeClaimed", 0);
                bool isClaimed = claimed == 1;
                freeClaimedRewards.Add(freeRewardTiers[i], isClaimed);
                if (isClaimed)
                    freeRewardTiers[i].Claimed();
            }
        }

        private void LoadPaidClaimedRewards()
        {
            for (int i = 0; i < paidRewardTiers.Length; i++)
            {
                int claimed = EncryptionManager.LoadInt(paidRewardTiers[i].name + "_paidClaimed", 0);
                bool isClaimed = claimed == 1;
                paidClaimedRewards.Add(paidRewardTiers[i], isClaimed);
                if (isClaimed)
                    paidRewardTiers[i].Claimed();
            }
        }

        private void ResetFreeClaimedRewards()
        {
            for (int i = 0; i < freeRewardTiers.Length; i++)
            {
                EncryptionManager.SaveInt(freeRewardTiers[i].name + "_freeClaimed", 0);
                freeClaimedRewards[freeRewardTiers[i]] = false;
            }
        }

        private void ResetPaidClaimedRewards()
        {
            for (int i = 0; i < paidRewardTiers.Length; i++)
            {
                EncryptionManager.SaveInt(paidRewardTiers[i].name + "_paidClaimed", 0);
                paidClaimedRewards[paidRewardTiers[i]] = false;
            }
        }

        // Reset the entire Battle Pass, call this when the season starts over
        public void ResetPass()
        {
            level = 0;
            nextLevel = 1;
            extraLevel = 0;
            xp = 0;
            totalXp = 0;

            EncryptionManager.SaveInt("_paidVersion", 0);

            xpSliderInfo.SetActive(true);
            seasonEndedPass = false;

            for (int i = 0; i < Math.Max(freeRewardTiers.Length, paidRewardTiers.Length); i++)
            {
                if (i < freeRewardTiers.Length && i >= level - 1)
                {
                    freeRewardTiers[i].claimed = false;
                    freeRewardTiers[i].Unlocked();
                }

                if (i < paidRewardTiers.Length && i >= level - 1)
                {
                    paidRewardTiers[i].claimed = false;
                    paidRewardTiers[i].Unlocked();
                }
            }

            ResetFreeClaimedRewards();
            ResetPaidClaimedRewards();

            if (tierScroller != null)
                tierScroller.ScrollToSection(level);

            reset = true;

            // ??? Affectation de 250 Gems et 0 Coins via DataSaver pour tester le pass payant ???
            dataSaver.dts.totalJewels = 250;
            dataSaver.dts.totalCoins = 0;
            DataSaver.Instance.SaveDataFn();
            // ????????????????????????????????????????????????????????????????????????????????????

            Save();
        }

        public void EndPassSeason()
        {
            seasonEndedPass = true;
            Load();
        }

        public void PassOffline()
        {
            paidVersionButton.SetActive(false);
            passDataScrollRect.SetActive(false);
            xpSliderInfo.SetActive(false);
        }

        public void PassOnline()
        {
            paidVersionButton.SetActive(true);
            passDataScrollRect.SetActive(true);
            xpSliderInfo.SetActive(true);
            Load();
        }

        public void ResetPassSeason()
        {
            // alias pour ResetPass
            ResetPass();
        }

        // remaining methods Save(), Load(), etc.
        private void Save()
        {
            EncryptionManager.SaveInt("currentLevel", level);
            EncryptionManager.SaveInt("nextLevel", nextLevel);
            EncryptionManager.SaveInt("currentXP", xp);
            EncryptionManager.SaveInt("totalXP", totalXp);
            EncryptionManager.SaveInt("extraLevel", extraLevel);

            if (reset)
                Load();
        }

        private void Load()
        {
            level = EncryptionManager.LoadInt("currentLevel", level);
            nextLevel = EncryptionManager.LoadInt("nextLevel", nextLevel);
            extraLevel = EncryptionManager.LoadInt("extraLevel", extraLevel);
            xp = EncryptionManager.LoadInt("currentXP", xp);
            totalXp = EncryptionManager.LoadInt("totalXP", totalXp);

            if (tierScroller != null)
                tierScroller.ScrollToSection(level == 0 ? level : (level > 0 && level != maxLevelPaid ? level : level - 1));

            int paidVersion = EncryptionManager.LoadInt("_paidVersion", 0);
            paidUnlocked = paidVersion == 1;
            paidVersionButton.SetActive(paidVersion == 0);

            if (seasonEndedPass || seasonOffline)
            {
                paidVersionButton.SetActive(false);
                xpSliderInfo.SetActive(false);
            }

            afterPassReward.gameObject.SetActive(hasAfterPassRewards);

            // lock/unlock UI tiers
            for (int i = 0; i < passTierProgressBar.Length; i++)
            {
                if (i < level)
                    passTierProgressBar[i].UnlockedTier();
                else
                    passTierProgressBar[i].LockedTier();

                bool shouldActivatePayButton = canSkipTier && ((level != maxLevelFree && !paidUnlocked) || (level != maxLevelPaid && paidUnlocked));
                if (i == level)
                {
                    passTierProgressBar[i].payProgressButton.SetActive(shouldActivatePayButton);
                    passTierProgressBar[i].lockedColor.SetActive(!shouldActivatePayButton);
                }
                if (seasonEndedPass)
                    passTierProgressBar[i].payProgressButton.SetActive(false);
            }

            if (extraLevel == 0)
                afterPassReward.LockedAfterPassReward();
            else
                afterPassReward.UnlockAfterPassReward();

            if (reset)
                reset = false;

            UpdateUI();
        }
    }
}
