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
        [SerializeField] private Slider levelProgressBar;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text extraLevelText;
        [SerializeField] private TMP_Text nextLevelText;
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private GameObject afterPassRewardIcon;
        public RewardPopUp rewardPopup;
        public GameObject paidVersionButton;
        public GameObject xpSliderInfo;
        public GameObject passDataScrollRect;
        [SerializeField] private PaginatedTiersScrollView tierScroller;

        [Header("-PASS DATA-")]
        [SerializeField] private int[] xpToNextLevel;
        [SerializeField] private int xpToExtraReward = 500;
        [SerializeField] private int maxLevelFree;
        [SerializeField] private int maxLevelPaid;
        [SerializeField] private PassTierProgressBar[] passTierProgressBar;
        [SerializeField] private PassReward[] freeRewardTiers;
        [SerializeField] private PassReward[] paidRewardTiers;
        public AfterPassReward afterPassReward;

        [Header("-CURRENCY SETTINGS-")]
        [SerializeField] private SimpleCurrencySystem.Currency passCurrency;
        [SerializeField] private SimpleCurrencySystem.Currency skipTierCurrency;

        private DataSaver dataSaver => DataSaver.Instance;
        [SerializeField] private bool paidUnlocked;

        [Header("-OPTIONAL FEATURES-")]
        [SerializeField] private bool hasAfterPassRewards;
        [SerializeField] private bool canSkipTier;

        [HideInInspector] public bool seasonOffline;
        [HideInInspector] public bool reset;
        [HideInInspector] public bool seasonEndedPass;

        private int level = 0;
        private int nextLevel = 1;
        private int extraLevel = 0;
        private int maxExtraLevel = 1;
        private int xp = 0;
        private int totalXp = 0;
        private bool buyingTier;

        private readonly Dictionary<PassReward, bool> freeClaimedRewards = new Dictionary<PassReward, bool>();
        private readonly Dictionary<PassReward, bool> paidClaimedRewards = new Dictionary<PassReward, bool>();

        void Start()
        {
            Load();
            UnlockRewards();
            LoadFreeClaimedRewards();
            LoadPaidClaimedRewards();
            UpdateUI();
        }

        public void LoadPass() => Load();

        public void AddXP(int amount)
        {
            if (seasonEndedPass || seasonOffline) return;

            if (level == maxLevelPaid && paidUnlocked)
            {
                if (hasAfterPassRewards) AddXpAfterPass(amount);
            }
            else if (level < maxLevelFree || paidUnlocked)
            {
                xp += amount;
                buyingTier = false;
            }

            totalXp += amount;

            while (level < xpToNextLevel.Length && xp >= xpToNextLevel[level] && level < maxLevelPaid)
            {
                xp -= xpToNextLevel[level];
                level++;
                nextLevel = Mathf.Min(maxLevelPaid, level + 1);
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

        public void PurchasePaidVersion()
        {
            if (EncryptionManager.LoadInt("SeasonEnded", 0) == 1) return;

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
            else Debug.Log($"Not enough {passCurrency.name}");

            Load();
        }

        public void UnlockNextTier()
        {
            if (EncryptionManager.LoadInt("SeasonEnded", 0) == 1) return;

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
            else Debug.Log($"Not enough {skipTierCurrency.name}");
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
                if (i <= level)
                {
                    paidRewardTiers[i].Unlocked();
                    paidRewardTiers[i].UnlockEffects();
                }
        }

        private void UnlockFreeRewards()
        {
            for (int i = 0; i < freeRewardTiers.Length; i++)
                if (i <= level)
                {
                    freeRewardTiers[i].Unlocked();
                    freeRewardTiers[i].UnlockEffects();
                }

            tierScroller?.ScrollToSection(level);
        }

        private void UpdateUI()
        {
            StartCoroutine(AnimateProgressBar());
            StartCoroutine(AnimatePassProgressBar());

            levelText.text = level.ToString();
            extraLevelText.text = extraLevel.ToString();
            bool showAfter = level == maxLevelPaid && hasAfterPassRewards;
            afterPassRewardIcon.SetActive(showAfter);

            if (level == maxLevelPaid)
            {
                xpText.text = showAfter ? $"{xp}/{xpToExtraReward}" : "0/0";
                nextLevelText.text = string.Empty;
            }
            else
            {
                xpText.text = $"{xp}/{xpToNextLevel[level]}";
                nextLevelText.text = nextLevel.ToString();
            }
        }

        IEnumerator AnimateProgressBar()
        {
            float t = 0f, dur = 1f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float frac = Mathf.Clamp01(t / dur);
                levelProgressBar.value = xpToNextLevel[level] > 0
                    ? Mathf.Lerp(levelProgressBar.value, (float)xp / xpToNextLevel[level], frac)
                    : 0;
                yield return null;
            }
        }

        IEnumerator AnimatePassProgressBar()
        {
            float t = 0f, dur = 1f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float frac = Mathf.Clamp01(t / dur);

                var bar = passTierProgressBar[level];
                if (level > 0)
                {
                    var prev = passTierProgressBar[level - 1];
                    prev.UnlockedTier();
                    prev.UnlockEffectsTier();
                }

                bool showBtn = canSkipTier && (level < maxLevelPaid);
                bar.payProgressButton.SetActive(showBtn);
                bar.lockedColor.SetActive(false);
                bar.tierSliderBar.value = xpToNextLevel[level] > 0
                    ? Mathf.Lerp(bar.tierSliderBar.value, (float)xp / xpToNextLevel[level], frac)
                    : 0;
                yield return null;
            }
        }

        public void ClaimFreeReward(PassReward reward)
        {
            if (freeClaimedRewards.TryGetValue(reward, out bool claimed) && claimed) return;
            freeClaimedRewards[reward] = false;

            rewardPopup.ShowPopup(reward.Icon, reward.itemAmount.ToString());
            foreach (var cr in reward.passTierRewards)
            {
                if (cr.name == "Coins") dataSaver.addCoins(cr.amount);
                else dataSaver.addJewels(cr.amount);
            }
            freeClaimedRewards[reward] = true;
            reward.Claimed();
            SaveFreeClaimedRewards();
            UpdateUI();
        }

        public void ClaimPaidReward(PassReward reward)
        {
            if (paidClaimedRewards.TryGetValue(reward, out bool claimed) && claimed) return;
            paidClaimedRewards[reward] = false;

            rewardPopup.ShowPopup(reward.Icon, reward.itemAmount.ToString());
            foreach (var cr in reward.passTierRewards)
            {
                if (cr.name == "Coins") dataSaver.addCoins(cr.amount);
                else dataSaver.addJewels(cr.amount);
            }
            paidClaimedRewards[reward] = true;
            reward.Claimed();
            SavePaidClaimedRewards();
            UpdateUI();
        }

        public void ClaimAfterPassReward()
        {
            if (extraLevel != maxExtraLevel) return;
            afterPassReward.ClaimAfterPassRewardEffects();
            extraLevel = 0;
            int idx = UnityEngine.Random.Range(0, afterPassReward.rewardCurrencies.Length);
            var rc = afterPassReward.rewardCurrencies[idx];
            rewardPopup.ShowPopup(rc.icon, rc.amount.ToString());
            if (rc.name == "Coins") dataSaver.addCoins(rc.amount);
            else dataSaver.addJewels(rc.amount);
            Save();
            Load();
        }

        private void SaveFreeClaimedRewards()
        {
            foreach (var kv in freeClaimedRewards)
                EncryptionManager.SaveInt(kv.Key.name + "_freeClaimed", kv.Value ? 1 : 0);
        }

        private void SavePaidClaimedRewards()
        {
            foreach (var kv in paidClaimedRewards)
                EncryptionManager.SaveInt(kv.Key.name + "_paidClaimed", kv.Value ? 1 : 0);
        }

        // Ajout des méthodes manquantes pour Reset
        private void LoadFreeClaimedRewards()
        {
            for (int i = 0; i < freeRewardTiers.Length; i++)
            {
                int claimed = EncryptionManager.LoadInt(freeRewardTiers[i].name + "_freeClaimed", 0);
                bool isClaimed = claimed == 1;
                freeClaimedRewards[freeRewardTiers[i]] = isClaimed;
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
                paidClaimedRewards[paidRewardTiers[i]] = isClaimed;
                if (isClaimed)
                    paidRewardTiers[i].Claimed();
            }
        }

        private void ResetFreeClaimedRewards()
        {
            foreach (var tier in freeRewardTiers)
                EncryptionManager.SaveInt(tier.name + "_freeClaimed", 0);
            freeClaimedRewards.Clear();
        }

        private void ResetPaidClaimedRewards()
        {
            foreach (var tier in paidRewardTiers)
                EncryptionManager.SaveInt(tier.name + "_paidClaimed", 0);
            paidClaimedRewards.Clear();
        }

        public void ResetPass()
        {
            level = 0; nextLevel = 1; extraLevel = 0; xp = 0; totalXp = 0;
            EncryptionManager.SaveInt("_paidVersion", 0);
            seasonEndedPass = false;
            xpSliderInfo.SetActive(true);
            for (int i = 0; i < freeRewardTiers.Length; i++) freeRewardTiers[i].Locked();
            for (int i = 0; i < paidRewardTiers.Length; i++) paidRewardTiers[i].Locked();
            ResetFreeClaimedRewards();
            ResetPaidClaimedRewards();
            tierScroller?.ScrollToSection(0);
            reset = true;
            Save();
            UpdateUI();
        }

        public void EndPassSeason() { seasonEndedPass = true; Save(); }
        public void PassOffline() { paidVersionButton.SetActive(false); passDataScrollRect.SetActive(false); xpSliderInfo.SetActive(false); }
        public void PassOnline() { paidVersionButton.SetActive(true); passDataScrollRect.SetActive(true); xpSliderInfo.SetActive(true); Load(); }
        public void ResetPassSeason() => ResetPass();

        private void Save()
        {
            EncryptionManager.SaveInt("currentLevel", level);
            EncryptionManager.SaveInt("nextLevel", nextLevel);
            EncryptionManager.SaveInt("currentXP", xp);
            EncryptionManager.SaveInt("totalXP", totalXp);
            EncryptionManager.SaveInt("extraLevel", extraLevel);
            if (reset) reset = false;
        }

        private void Load()
        {
            level = EncryptionManager.LoadInt("currentLevel", level);
            nextLevel = EncryptionManager.LoadInt("nextLevel", nextLevel);
            extraLevel = EncryptionManager.LoadInt("extraLevel", extraLevel);
            xp = EncryptionManager.LoadInt("currentXP", xp);
            totalXp = EncryptionManager.LoadInt("totalXP", totalXp);
            tierScroller?.ScrollToSection(level);
            int pv = EncryptionManager.LoadInt("_paidVersion", 0);
            paidUnlocked = pv == 1;
            paidVersionButton.SetActive(pv == 0);
            if (seasonEndedPass || seasonOffline)
            {
                paidVersionButton.SetActive(false);
                xpSliderInfo.SetActive(false);
            }
            afterPassReward.gameObject.SetActive(hasAfterPassRewards);
            UpdateUI();
        }
    }
}
