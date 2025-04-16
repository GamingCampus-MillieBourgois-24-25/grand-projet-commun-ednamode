using System;
using Unity.Services.Core;
using Unity.Services.Mediation;
using UnityEngine;
using UnityEngine.UI;

public class RewardedAdsButton : MonoBehaviour
{
    [SerializeField] private Button _showAdButton;
    private IRewardedAd _rewardedAd;

    private void Start()
    {
        // Initialize Unity Mediation
        UnityServices.InitializeAsync().ContinueWith(task =>
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                Debug.Log("Unity Mediation Initialized");
                InitializeRewardedAd();
            }
            else
            {
                Debug.LogError("Failed to initialize Unity Mediation");
            }
        });

        // Disable the button until the ad is ready
        _showAdButton.interactable = false;
    }

    private void InitializeRewardedAd()
    {
        // Create a rewarded ad instance
        _rewardedAd = MediationService.Instance.CreateRewardedAd("Rewarded_Ad_Unit_Id");

        // Subscribe to events
        _rewardedAd.OnLoaded += OnAdLoaded;
        _rewardedAd.OnFailedLoad += OnAdFailedToLoad;
        _rewardedAd.OnShowed += OnAdShowed;
        _rewardedAd.OnFailedShow += OnAdFailedToShow;
        _rewardedAd.OnUserRewarded += OnUserRewarded;

        // Load the ad
        _rewardedAd.Load();
    }

    private void OnAdLoaded(object sender, EventArgs e)
    {
        Debug.Log("Ad Loaded");
        _showAdButton.interactable = true;
    }

    private void OnAdFailedToLoad(object sender, LoadErrorEventArgs e)
    {
        Debug.LogError($"Ad Failed to Load: {e.Message}");
    }

    private void OnAdShowed(object sender, EventArgs e)
    {
        Debug.Log("Ad Showed");
    }

    private void OnAdFailedToShow(object sender, ShowErrorEventArgs e)
    {
        Debug.LogError($"Ad Failed to Show: {e.Message}");
    }

    private void OnUserRewarded(object sender, RewardEventArgs e)
    {
        Debug.Log($"User Rewarded: {e.Type} - {e.Amount}");
        // Grant the reward to the user
    }

    public void ShowAd()
    {
        if (_rewardedAd.AdState == AdState.Loaded)
        {
            _rewardedAd.Show();
        }
        else
        {
            Debug.LogWarning("Ad is not ready to show");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (_rewardedAd != null)
        {
            _rewardedAd.OnLoaded -= OnAdLoaded;
            _rewardedAd.OnFailedLoad -= OnAdFailedToLoad;
            _rewardedAd.OnShowed -= OnAdShowed;
            _rewardedAd.OnFailedShow -= OnAdFailedToShow;
            _rewardedAd.OnUserRewarded -= OnUserRewarded;
        }
    }
}
