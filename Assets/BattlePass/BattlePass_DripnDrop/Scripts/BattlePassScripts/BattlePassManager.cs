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
        // The currency system to save rewards/ Just for an example can remove this and implement your own reward system
        public SimpleCurrencySystem currencySystem;
        // the currency and amount used to buy the battle pass
        [SerializeField] private SimpleCurrencySystem.Currency passCurrency;
        //// the currency and amount used to skip tiers with
        [SerializeField] private SimpleCurrencySystem.Currency skipTierCurrency;
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

        //Just if needed to call in other scripts or when diasbling the battle pass canvas and then enabling it again.
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
            if (extraLevel == maxExtraLevel )
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
                if (currencySystem.SpendCurrency(passCurrency.name, passCurrency.amount))
                {
                    // Set the paid version if bought
                    EncryptionManager.SaveInt("_paidVersion", 1);

                    // Set the paid version button to inactive
                    paidVersionButton.SetActive(false);
                    paidUnlocked = true;
                    UnlockPaidRewards();
                }
                else
                {
                    Debug.Log("Not enough Gems to spend.");
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

            //Load();
        }


        // Unlock the rewards for the current level in the paid battle pass
        private void UnlockPaidRewards()
        {
            // Cycle through the rewards
            for (int i = 0; i < paidRewardTiers.Length; i++)
            {
                // Unlock all rewards up until the players battle pass level
                if (i <= level)
                {
                    // Unlock the reward
                    paidRewardTiers[i].Unlocked();
                    paidRewardTiers[i].UnlockEffects();
                }
            }
        }

        // Unlock the rewards for the current level in the free battle pass
        private void UnlockFreeRewards()
        {
            // Cycle through the rewards
            for (int i = 0; i < freeRewardTiers.Length; i++)
            {
                // If the reward is for the current level
                if (i <= level)
                {
                    // Unlock the reward
                    freeRewardTiers[i].Unlocked();
                    freeRewardTiers[i].UnlockEffects();

                }
            }

            if (tierScroller != null)
                tierScroller.ScrollToSection(level == 0 ? level : (level > 0 && level != maxLevelPaid ? level : level - 1));
        }

        // Unlock the extra rewards after the pass is completed after reaching the xp amount required
        private void UnlockAfterPassReward()
        {
            afterPassReward.UnlockAfterPassReward();

            // can use this function for other features such as playing a sound or effects, etc.
        }

        // Skip a tier called from other scripts
        public void UnlockNextTier()
        {
            if (EncryptionManager.LoadInt("SeasonEnded", 0) != 1)
            {
                if (currencySystem.SpendCurrency(skipTierCurrency.name, skipTierCurrency.amount))
                {
                    buyingTier = true;
                    var gainXp = xpToNextLevel[level] - xp;
                    AddXP(gainXp);
                }
                else
                {
                    Debug.Log("Not enough Gems to spend.");
                }
            }           
            
        }

        // Update the UI
        private void UpdateUI()
        {
            StartCoroutine(AnimateProgressBar());
            StartCoroutine(AnimatePassProgressBar());

            if (levelText != null)
            {
                levelText.text = level.ToString();
            }

            if (extraLevelText != null)
            {
                extraLevelText.text = extraLevel.ToString();
            }

            bool showAfterPassRewardIcon = level == maxLevelPaid && hasAfterPassRewards;

            if (afterPassRewardIcon != null)
            {
                afterPassRewardIcon.SetActive(showAfterPassRewardIcon);
            }

            if (level == maxLevelPaid)
            {
                if (xpText != null)
                {
                    xpText.text = showAfterPassRewardIcon ? $"{xp} / {xpToExtraReward}" : "0 / 0";
                }

                if (nextLevelText != null)
                {
                    nextLevelText.text = "";
                }
            }
            else
            {
                if (xpText != null)
                {
                    xpText.text = $"{xp} / {xpToNextLevel[level]}";
                }

                if (nextLevelText != null)
                {
                    nextLevelText.text = nextLevel.ToString();
                }
            }
        }

        // Progress bar for Pass Level
        IEnumerator AnimateProgressBar()
        {
            float time = 0f;
            float duration = 1f; // The duration of the animation in seconds

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                if (level == maxLevelPaid && hasAfterPassRewards)
                {
                    levelProgressBar.value = Mathf.Lerp(levelProgressBar.value, (float)(xp) / xpToExtraReward, t);
                }
                else
                {
                    levelProgressBar.value = Mathf.Lerp(levelProgressBar.value, (float)(xp) / xpToNextLevel[level], t);
                }
                
                yield return null;
            }

        }

        IEnumerator AnimatePassProgressBar()
        {
            float time = 0f;
            float duration = 1f; // The duration of the animation in seconds

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

                if (level == maxLevelPaid && hasAfterPassRewards)
                {
                    currentTier.tierSliderBar.value = Mathf.Lerp(currentTier.tierSliderBar.value, (float)(xp) / xpToExtraReward, t);
                }
                else
                {
                    currentTier.tierSliderBar.value = Mathf.Lerp(currentTier.tierSliderBar.value, (float)(xp) / xpToNextLevel[level], t);
                }

                if (seasonEndedPass)
                {
                    currentTier.payProgressButton.SetActive(false);
                }

                yield return null;
            }
        }

        // Function to claim the extra reward after player gets the extra XP required to unlock the after pass reward. It then resets once claimed
        public void ClaimAfterPassReward()
        {
            if (extraLevel != maxExtraLevel)
                return;

            afterPassReward.ClaimAfterPassRewardEffects();
            extraLevel = 0;

            int randomReward = UnityEngine.Random.Range(0, afterPassReward.rewardCurrencies.Length);

            // picks a random reward from the set rewards
            rewardPopup.ShowPopup(afterPassReward.rewardCurrencies[randomReward].icon, afterPassReward.rewardCurrencies[randomReward].amount.ToString());

            // Currency system claim and save the reward, can implement your own items/ currency system here
            currencySystem.RewardCurrency(afterPassReward.rewardCurrencies[randomReward].name, afterPassReward.rewardCurrencies[randomReward].amount);

            Save();
            Load();
        }

        /// <summary>
        //// A function to claim a free reward called from other scripts or buttons. This is where
        //// you would add your own rewarding logic if you'd like. This includes rewarding
        //// with the currency system script
        /// </summary>
        public void ClaimFreeReward(PassReward reward)
        {
            // Check if the reward has already been claimed
            if (!freeClaimedRewards.TryGetValue(reward, out bool isClaimed) || isClaimed)
            {
                Debug.Log("This free reward has already been claimed.");
                return;
            }

            Debug.Log("Claiming reward: " + reward.name);

            // Show Reward Popup and set the data of the reward to the popup;
            rewardPopup.ShowPopup(reward.Icon, reward.itemAmount.ToString());

            // Currency system claim and save the reward, can implement your own items/ currency system here
            foreach (var tierReward in reward.passTierRewards)
            {
                currencySystem.RewardCurrency(tierReward.name, tierReward.amount);
            }

            // Set the reward's claimed status to true
            freeClaimedRewards[reward] = true;

            // Call the claimed function of the pass reward script
            reward.Claimed();

            // Save the claimed status of rewards
            SaveFreeClaimedRewards();
        }

        /// <summary>
        //// A function to claim a paid reward called from other scripts or buttons. This is where
        //// you would add your own rewarding logic if you'd like. This includes rewarding
        //// with the currency system script
        /// </summary>
        public void ClaimPaidReward(PassReward reward)
        {
            // Check if the reward has already been claimed
            if (!paidClaimedRewards.TryGetValue(reward, out bool isClaimed) || isClaimed)
            {
                Debug.Log("This free reward has already been claimed.");
                return;
            }

            Debug.Log("Claiming reward: " + reward.name);

            // Show Reward Popup and set the data of the reward to the popup;
            rewardPopup.ShowPopup(reward.Icon, reward.itemAmount.ToString());

            // An example way to reward a currencies, can remove and add your own system
            foreach (var tierReward in reward.passTierRewards)
            {
                currencySystem.RewardCurrency(tierReward.name, tierReward.amount);
            }
            
            // Set the reward's claimed status to true
            paidClaimedRewards[reward] = true;

            // Set the ui of the claimed reward
            reward.Claimed();

            // Save the claimed status of rewards
            SavePaidClaimedRewards();
        }

        // Saves the claimed status of the free rewards to Encryption Manager to secure. Can use your own logic
        private void SaveFreeClaimedRewards()
        {
            // Iterate through the dictionary and save the claimed status of each reward
            foreach (KeyValuePair<PassReward, bool> reward in freeClaimedRewards)
            {
                EncryptionManager.SaveInt(reward.Key.name + "_freeClaimed", reward.Value ? 1 : 0);
            }
        }

        // Saves the claimed status of the paid rewards to Encryption Manager to secure. Can use your own logic
        private void SavePaidClaimedRewards()
        {
            // Iterate through the dictionary and save the claimed status of each reward
            foreach (KeyValuePair<PassReward, bool> reward in paidClaimedRewards)
            {
                EncryptionManager.SaveInt(reward.Key.name + "_paidClaimed", reward.Value ? 1 : 0);
            }

        }

        // Loads the claimed status of free rewards
        private void LoadFreeClaimedRewards()
        {
            // Iterate through the free rewards and load their claimed status from PlayerPrefs
            for (int i = 0; i < freeRewardTiers.Length; i++)
            {
                int claimed = EncryptionManager.LoadInt(freeRewardTiers[i].name + "_freeClaimed", 0);

                bool isClaimed = claimed == 1;
                freeClaimedRewards.Add(freeRewardTiers[i], isClaimed);

                if (isClaimed)
                {
                    freeRewardTiers[i].Claimed();
                }
            }
        }

        // Loads the claimed status of paid rewards
        private void LoadPaidClaimedRewards()
        {
            // Iterate through the free rewards and load their claimed status from PlayerPrefs
            for (int i = 0; i < paidRewardTiers.Length; i++)
            {
                int claimed = EncryptionManager.LoadInt(paidRewardTiers[i].name + "_paidClaimed", 0);

                bool isClaimed = claimed == 1;
                paidClaimedRewards.Add(paidRewardTiers[i], isClaimed);

                if (isClaimed)
                {
                    paidRewardTiers[i].Claimed();
                }
            }
        }

        // Reset the claimed free rewards 
        private void ResetFreeClaimedRewards()
        {
            for (int i = 0; i < freeRewardTiers.Length; i++)
            {
                EncryptionManager.SaveInt(freeRewardTiers[i].name + "_freeClaimed", 0);
                freeClaimedRewards[freeRewardTiers[i]] = false;
            }
        }

        // Reset the claimed paid rewards 
        private void ResetPaidClaimedRewards()
        {
            for (int i = 0; i < paidRewardTiers.Length; i++)
            {
                EncryptionManager.SaveInt(paidRewardTiers[i].name + "_paidClaimed", 0);
                paidClaimedRewards[paidRewardTiers[i]] = false;
            }
        }

        // added a serializing and encryption saving
        private void Save()
        {
            // Using Encryption Manager. Can use just player prefs if you'd like or another saving and loading method you have
            EncryptionManager.SaveInt("currentLevel", level);
            EncryptionManager.SaveInt("nextLevel", nextLevel);
            EncryptionManager.SaveInt("currentXP", xp);
            EncryptionManager.SaveInt("totalXP", totalXp);
            EncryptionManager.SaveInt("extraLevel", extraLevel);

            if (reset)
                Load();               
        }

        // Added loading from the encrytion manager for protection
        // Also Loads the current status of the UI of whats unlocked and whats not
        private void Load()
        {
            // Using Encryption Manager.
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

            for (int i = 0; i < paidRewardTiers.Length; i++)
            {
                if (paidVersion == 0 || (paidVersion == 1 && i > level))
                {
                    paidRewardTiers[i].Locked();
                }
            }

            for (int i = 0; i < freeRewardTiers.Length; i++)
            {
                if (i > level)
                {
                    freeRewardTiers[i].Locked();
                }
            }

            for (int i = 0; i < passTierProgressBar.Length; i++)
            {
                if (i < level)
                {
                    passTierProgressBar[i].UnlockedTier();
                }
                else if (i > level - 1 || (i > level - 1 && reset))
                {
                    passTierProgressBar[i].LockedTier();
                }

                if (i == level)
                {
                    bool shouldActivatePayProgressButton = canSkipTier && ((level != maxLevelFree && !paidUnlocked) || (level != maxLevelPaid && paidUnlocked));
                    passTierProgressBar[i].payProgressButton.SetActive(shouldActivatePayProgressButton);
                    passTierProgressBar[i].lockedColor.SetActive(!shouldActivatePayProgressButton);
                }

                if (seasonEndedPass)
                {
                    passTierProgressBar[i].payProgressButton.SetActive(false);
                }
            }

            if (extraLevel == 0)
            {
                afterPassReward.LockedAfterPassReward();
            }
            else if (extraLevel == 1) // change to >= 1 if you want to stack
            {
                afterPassReward.UnlockAfterPassReward();
            }

            if (reset)
                reset = false;

            UpdateUI();
        }

        // End the pass if the season ends, so players can't unlock anymore rewards
        public void EndPassSeason()
        {
            seasonEndedPass = true;
            Load();       
        }

        // Called when the player loses connection to internet
        public void PassOffline()
        {
            paidVersionButton.SetActive(false);
            passDataScrollRect.SetActive(false);
            xpSliderInfo.SetActive(false);
        }

        // Called when player is connected to internet again. Both of these functions wont be needed, -->
        // if running the pass from your own server. This is for those who don't want to go that route
        public void PassOnline()
        {
            paidVersionButton.SetActive(true);
            passDataScrollRect.SetActive(true);
            xpSliderInfo.SetActive(true);
            Load();
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
            currencySystem.SetCurrency("Gems", 250);
            currencySystem.SetCurrency("Gold", 0);
            Save();
        }
    }
}