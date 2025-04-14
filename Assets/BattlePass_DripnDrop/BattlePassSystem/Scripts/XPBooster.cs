using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

namespace EasyBattlePass
{
    public class XPBooster : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string xpBoostDurationKey = "XpBoostTime";
        [SerializeField] private string xpBoostMultiplierKey = "XpBoostMultiplier";
        [SerializeField] private string WorldTimeAPIURL = "http://worldtimeapi.org/api/ip";

        // Only if you want to apply the XpBoosters here in the script, otherwise you'd call the 'ActivateXpBoostWithToken(XPBoosterToken token)' in other scripts
        /*
        [Header("XP Booster Tokens")]
        [SerializeField] private XPBoosterToken xpX2Token30Mins;
        [SerializeField] private XPBoosterToken xpX2Token1Hour;
        [SerializeField] private XPBoosterToken xpX3Token30Mins;
        [SerializeField] private XPBoosterToken xpX3Token1Hour;
        */


        [Header("UI Elements")]
        [SerializeField] private TMP_Text xpBoostTimeText;

        [Header("Dependencies")]
        [SerializeField] private BattlePassManager battlePassManager;


        private int currentTokenMultiplier;
        private int xpBoostMinutes;
        private int xpBoostHours;

        private DateTime xpBoostDuration;
        private DateTime timeNow;
        private DateTime extraTime;

        private TimeSpan remainingXpBoostTime;
        private Coroutine updateTimeCoroutine;
        public bool xpBoosted;
        private bool newBoostStarting = false;

        private struct TimeData
        {
            public string datetime;
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey(xpBoostDurationKey))
            {
                // Check if their is saved multiplier
                if (PlayerPrefs.HasKey(xpBoostMultiplierKey))
                {
                    currentTokenMultiplier = EncryptionManager.LoadInt(xpBoostMultiplierKey);
                }
                string savedXpDuration = EncryptionManager.Load<string>(xpBoostDurationKey);
                xpBoostDuration = DateTime.Parse(savedXpDuration);
                if (EncryptionManager.LoadInt("SeasonEnded", 0) == 1)
                {
                    EndXpBoost();
                    xpBoosted = false;
                }
                else
                {
                    xpBoosted = true;
                }
                
                if (updateTimeCoroutine == null)
                {
                    updateTimeCoroutine = StartCoroutine(UpdateWorldTime());
                }
            }
        }

        private void Update()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                xpBoostTimeText.text = "";
                return;
            }

            if (xpBoosted)
            {
                if (timeNow == DateTime.MinValue) return;

                if (xpBoostDuration != DateTime.MinValue)
                {
                    UpdateXpBoostState();
                    UpdateRemainingTime();
                }
            }
            else
            {
                xpBoostTimeText.text = "";
            }
        }

        public void CheckXpTime()
        {
            if (xpBoosted)
            {
                if (updateTimeCoroutine != null)
                {
                    StopCoroutine(updateTimeCoroutine);
                    updateTimeCoroutine = null;
                }

                if (updateTimeCoroutine == null)
                {
                    updateTimeCoroutine = StartCoroutine(UpdateWorldTime());
                }
            }
        }

        private void UpdateXpBoostState()
        {
            battlePassManager.xpBoostMultiplier = currentTokenMultiplier;
            battlePassManager.xpBooster = true;
        }

        private void UpdateRemainingTime()
        {
            TimeSpan timeLeft = xpBoostDuration - timeNow;

            if (timeLeft != remainingXpBoostTime)
            {
                remainingXpBoostTime = timeLeft;
                UpdateBoostTimeCountDown();
            }

            if (!newBoostStarting && timeLeft <= TimeSpan.Zero)
            {
                EndXpBoost();
                xpBoosted = false;
            }
        }

        private void UpdateBoostTimeCountDown()
        {
            string remainingTimeText = "";

            if (remainingXpBoostTime > TimeSpan.FromDays(1))
                remainingTimeText = string.Format("{0}h {1}m", remainingXpBoostTime.Hours, remainingXpBoostTime.Minutes);
            else if (remainingXpBoostTime <= TimeSpan.FromDays(1) && remainingXpBoostTime > TimeSpan.FromHours(1))
                remainingTimeText = string.Format("{0}h {1}m", remainingXpBoostTime.Hours, remainingXpBoostTime.Minutes);
            else if (remainingXpBoostTime <= TimeSpan.FromHours(1))
                remainingTimeText = string.Format("{0}m {1}s", remainingXpBoostTime.Minutes, remainingXpBoostTime.Seconds);

            if (xpBoostTimeText != null)
                xpBoostTimeText.text = $"Xp x {currentTokenMultiplier} ends in {remainingTimeText}";
        }

        public void ActivateXpBoostWithToken(XPBoosterToken token)
        {
            if(EncryptionManager.LoadInt("SeasonEnded", 0) == 0)
            {
                ActivateXpBoostToken(token);
            }           
        }

        private void ActivateXpBoostToken(XPBoosterToken token)
        {
            if (battlePassManager.currencySystem.SpendCurrency(token.tokenCost.name, token.tokenCost.amount))
            {
                if (!xpBoosted || (xpBoosted && currentTokenMultiplier == token.xpMultiplier))
                {
                    currentTokenMultiplier = token.xpMultiplier;
                    xpBoostHours = token.boostHours;
                    xpBoostMinutes = token.boostMinutes;                  
                    ActivateXpBoost(xpBoostHours, xpBoostMinutes);
                }
                else
                {
                    Debug.Log("Cannot activate a different multiplier token while another is active.");
                    // add your own Popup UI notification
                }
            }
            else
            {
                Debug.Log("Not enough currency to activate the token.");
                // add your own Popup UI notification
            }
        }

        private void ActivateXpBoost(int xpBoostHours, int xpBoostMinutes)
        {
            xpBoosted = true;
            newBoostStarting = true;

            if (updateTimeCoroutine != null)
            {
                StopCoroutine(updateTimeCoroutine);
            }
            updateTimeCoroutine = StartCoroutine(UpdateWorldTime());
            StartCoroutine(WaitToGetWorldTime(xpBoostHours, xpBoostMinutes));
        }

        

        private IEnumerator WaitToGetWorldTime(int xpBoostHours, int xpBoostMinutes)
        {
            yield return new WaitForSeconds(1);
            xpBoosted = true;

            extraTime = PlayerPrefs.HasKey(xpBoostDurationKey) ? xpBoostDuration : timeNow;
            xpBoostDuration = extraTime.AddHours(xpBoostHours).AddMinutes(xpBoostMinutes).AddSeconds(2);

            SaveXpBoostTime();
            newBoostStarting = false;
            Update();
        }

        private IEnumerator UpdateWorldTime()
        {
            while (xpBoosted)
            {
                UnityWebRequest request = UnityWebRequest.Get(WorldTimeAPIURL);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    DateTime worldTime = DateTime.Parse(JsonUtility.FromJson<TimeData>(responseText).datetime);
                    timeNow = worldTime.ToUniversalTime();
                }
                else
                {
                    //Debug.LogError("Failed to get world time from API: " + request.error);
                    timeNow = DateTime.UtcNow;
                }

                float waitTime = remainingXpBoostTime <= TimeSpan.FromHours(1) ? 1f : 60f;
                yield return new WaitForSeconds(waitTime);
            }
        }

        public void EndXpBoost()
        {
            xpBoostDuration = DateTime.UtcNow.AddDays(0).AddHours(0).AddMinutes(0);
            battlePassManager.xpBoostMultiplier = 0;
            battlePassManager.xpBooster = false;
            currentTokenMultiplier = 1;
            SaveXpBoostTime();
            PlayerPrefs.DeleteKey(xpBoostDurationKey);
        }

        private void SaveXpBoostTime()
        {
            string boostTimeString = xpBoostDuration.ToString();
            EncryptionManager.Save(xpBoostDurationKey, boostTimeString);
            EncryptionManager.SaveInt(xpBoostMultiplierKey, currentTokenMultiplier);
        }
    }
}

