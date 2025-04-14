using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

namespace EasyBattlePass
{
    public class SeasonManager : MonoBehaviour
    {
        [SerializeField] private string SeasonEndTimeKey = "Season1EndTime"; // Change this to whatever key you want to save the season pass time on. Change this for each new season as well
        [SerializeField] private string OldSeasonEndTimeKey = "Nothing"; // Change this to whatever SeasonEndTimeKey used to be when you start a new season past season 1

        [Header("-Public World Time API URL-")]
        [SerializeField] private string WorldTimeAPIURL = "https://timeapi.io/api/Time/current/ip"; // "http://worldtimeapi.org/api/ip";

        // Uses set time for countdown timers, mostly here for testing but can be used for other needs of rewarding
        [Header("-USE SET TIME-")]
        [SerializeField] private int seasonDurationInDays = 30;
        [SerializeField] private int seasonDurationInHours = 0;
        [SerializeField] private int seasonDurationInMinutes = 0;

        // Use this for when you launch with your game, this ensures all players season ends at the same time.
        [Header("-USE SET DATE-")]
        [SerializeField] private int seasonEndYear;
        [SerializeField] private int seasonEndMonth;
        [SerializeField] private int seasonEndDay;

        [Header("-SEASON UI ELEMENTS-")]
        [SerializeField] private int currentSeason;
        [SerializeField] private TMP_Text seasonNameText;
        [SerializeField] private TMP_Text seasonTimeText;

        [SerializeField] private BattlePassManager battlePassManager;
        [SerializeField] private XPBooster xpBoosterManager;

        private DateTime seasonEndTime;
        private DateTime worldTimeNow;

        private TimeSpan remainingTime;

        private Coroutine updateWorldTimeCoroutine;

        public bool useSetTime;
        public bool useEndDate;

        [HideInInspector] public bool seasonEnded;
        private bool newSeason;

        struct TimeData
        {
            public string datetime;
        }

        private void Start()
        {

            if (seasonNameText != null)
                seasonNameText.text = "Season " + currentSeason.ToString();

            if (EncryptionManager.LoadInt("SeasonEnded", 0) == 1)
            {
                seasonEnded = true;
                seasonTimeText.text = "Season has ended";
                LoadBattlePassEndStatus();
            }
            else
            {
                GetTimeNow();
            }     
        }

        // Check if there is a time saved already, if not start the new season.
        private void GetTimeNow()
        {
            if (!seasonEnded)
            {
                if (PlayerPrefs.HasKey(SeasonEndTimeKey))
                {
                    Debug.Log("Has player prefs key");
                    string savedEndTime = EncryptionManager.Load<string>(SeasonEndTimeKey);
                    seasonEndTime = DateTime.Parse(savedEndTime);

                    if (updateWorldTimeCoroutine == null)
                    {
                        updateWorldTimeCoroutine = StartCoroutine(UpdateWorldTime());
                    }

                }
                else
                {
                    StartCoroutine(StartNewSeason());
                }
            }

        }

        // Call this in buttons when enabling or disabling the canvas or object the Season Manager is attached to.
        public void CheckSeasonTime()
        {
            if (!seasonEnded)
            {

                if (updateWorldTimeCoroutine != null)
                {
                    StopCoroutine(updateWorldTimeCoroutine);
                    updateWorldTimeCoroutine = null; // Set updateWorldTimeCoroutine to null after stopping the coroutine
                }

                if (updateWorldTimeCoroutine == null)
                {
                    updateWorldTimeCoroutine = StartCoroutine(UpdateWorldTime());
                }
            }

        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveSeasonEndTime();
            }
            else
            {
                if (PlayerPrefs.HasKey(SeasonEndTimeKey))
                {
                    string savedEndTime = EncryptionManager.Load<string>(SeasonEndTimeKey);
                    DateTime savedSeasonEndTime = DateTime.Parse(savedEndTime);
                    if (savedSeasonEndTime != seasonEndTime)
                    {
                        seasonEndTime = savedSeasonEndTime;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (seasonEndTime != DateTime.MinValue)
            {
                SaveSeasonEndTime();
            }

            if (updateWorldTimeCoroutine != null)
            {
                StopCoroutine(updateWorldTimeCoroutine);
                updateWorldTimeCoroutine = null;
            }

        }

        private void Update()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                seasonTimeText.text = "Offline";
                battlePassManager.seasonOffline = true;
                battlePassManager.PassOffline();
                return;
            }
            else if (battlePassManager.seasonOffline)
            {
                battlePassManager.seasonOffline = false;
                battlePassManager.PassOnline();
                UpdateTimeCountDown();
            }

            if (seasonEnded || worldTimeNow == DateTime.MinValue || newSeason)
            {
                if (newSeason && seasonTimeText != null)
                    seasonTimeText.text = "";
                return;
            }

            if (seasonEndTime != DateTime.MinValue)
            {
                TimeSpan timeLeft = seasonEndTime - worldTimeNow;

                if (timeLeft != remainingTime)
                {
                    remainingTime = timeLeft;
                    UpdateTimeCountDown();
                }

                if (timeLeft <= TimeSpan.Zero)
                {
                    if (seasonTimeText != null)
                    {
                        if (newSeason)
                            seasonTimeText.text = "";
                        else
                            seasonTimeText.text = "Season has ended";
                    }

                    EndSeason();
                    seasonEnded = true;
                }
            }
        }

        // Updates the UI time text
        private void UpdateTimeCountDown()
        {
            if (seasonEnded || seasonTimeText == null) return;

            string remainingTimeText;

            if (remainingTime <= TimeSpan.FromHours(1))
            {
                remainingTimeText = string.Format("{0}m {1}s", remainingTime.Minutes, remainingTime.Seconds);
            }
            else if (remainingTime <= TimeSpan.FromDays(1))
            {
                remainingTimeText = string.Format("{0}h {1}m", remainingTime.Hours, remainingTime.Minutes);
            }
            else
            {
                remainingTimeText = string.Format("{0}d {1}h", remainingTime.Days, remainingTime.Hours);
            }

            seasonTimeText.text = "Season ends in " + remainingTimeText;
        }

        // Checks for the world time as long as the season has not ended;
        private IEnumerator UpdateWorldTime()
        {
            while (!seasonEnded)
            {
                UnityWebRequest request = UnityWebRequest.Get(WorldTimeAPIURL);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    //Debug.Log("Success got Time");
                    string responseText = request.downloadHandler.text;
                    DateTime worldTime = DateTime.Parse(JsonUtility.FromJson<TimeData>(responseText).datetime);
                    worldTimeNow = worldTime.ToUniversalTime();
                }
                else if (request.result != UnityWebRequest.Result.Success)
                {
                    //Debug.LogError("Failed to get world time from API: " + request.error);
                    worldTimeNow = DateTime.UtcNow;
                }

                if (remainingTime <= TimeSpan.FromHours(1))
                    yield return new WaitForSeconds(1f);

                if (remainingTime > TimeSpan.FromHours(1))
                    yield return new WaitForSeconds(60f);
            }
        }
        // For Testing
        public void TestNewSeason()
        {
            StartCoroutine(StartNewSeason());
        }

        private IEnumerator StartNewSeason()
        {
            PlayerPrefs.DeleteKey("SeasonEnded");
            seasonTimeText.text = "";
            seasonEnded = false;
            newSeason = true;

            // Gets the world time
            if (updateWorldTimeCoroutine != null)
            {
                StopCoroutine(updateWorldTimeCoroutine);
            }
            updateWorldTimeCoroutine = StartCoroutine(UpdateWorldTime());

            // Wait for 1 second to receive the world time
            yield return new WaitForSeconds(1);

            PlayerPrefs.DeleteKey(OldSeasonEndTimeKey);

            DateTime currentTime = worldTimeNow;

            if (useSetTime)
            {
                seasonEndTime = currentTime.AddDays(seasonDurationInDays).AddHours(seasonDurationInHours).AddMinutes(seasonDurationInMinutes).AddSeconds(2);
            }

            if (useEndDate)
            {
                DateTime endDate = new DateTime(seasonEndYear, seasonEndMonth, seasonEndDay);
                TimeSpan remainingTime = endDate - currentTime;
                seasonEndTime = currentTime.Add(remainingTime);
            }

            battlePassManager.ResetPass();
            SaveSeasonEndTime();
            Update();
            newSeason = false;
        }

        private void EndSeason()
        {
            Debug.Log("Season has ended.");
            seasonEndTime = DateTime.UtcNow.AddDays(0).AddHours(0).AddMinutes(0);
            SaveSeasonEndTime();
            EncryptionManager.SaveInt("SeasonEnded", 1);           
            LoadBattlePassEndStatus();
            EndBoostedXp();
        }

        //calls the end season function in the battle pass
        private void LoadBattlePassEndStatus()
        {
            battlePassManager.EndPassSeason();
        }

        // makes any remaing Xp boost end if the season has ended
        private void EndBoostedXp()
        {
            xpBoosterManager.EndXpBoost();
        }

        // Saves the seasons current time;
        private void SaveSeasonEndTime()
        {
            string endTimeString = seasonEndTime.ToString();
            EncryptionManager.Save(SeasonEndTimeKey, endTimeString);
        }

    }
}


