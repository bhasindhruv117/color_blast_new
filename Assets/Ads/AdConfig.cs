using UnityEngine;
using System.Collections.Generic;
using com.unity3d.mediation;

[CreateAssetMenu(fileName = "AdConfig", menuName = "Ads/Ad Configuration")]
public class AdConfig : ScriptableObject
{
    [Header("LevelPlay Configuration")]
    [Tooltip("iOS App Key from IronSource Dashboard")]
    public string iOSAppKey = "YOUR_IOS_APP_KEY";
    
    [Tooltip("Android App Key from IronSource Dashboard")]
    public string androidAppKey = "YOUR_ANDROID_APP_KEY";
    
    [Tooltip("Enable test mode for ads")]
    public bool testMode = true;
    
    [Tooltip("Launch test suite on initialization")]
    public bool launchTestSuite = true;

    [Header("Ad Unit IDs")]
    [Tooltip("Interstitial Ad Unit ID")]
    public string interstitialAdUnitId = "YOUR_INTERSTITIAL_AD_UNIT_ID";
    
    [Tooltip("Banner Ad Unit ID")]
    public string bannerAdUnitId = "YOUR_BANNER_AD_UNIT_ID";
    
    [Tooltip("Rewarded Ad Unit ID")]
    public string rewardedAdUnitId = "YOUR_REWARDED_AD_UNIT_ID";

    [Header("Banner Ad Settings")]
    [Tooltip("Default banner position")]
    public LevelPlayBannerPosition bannerPosition = LevelPlayBannerPosition.BottomCenter;
    
    [Tooltip("Automatically load and show banner ad after initialization")]
    public bool autoLoadBannerOnInit = true;
    
    [Tooltip("Banner width in dp (density-independent pixels)")]
    public int bannerWidth = 320;
    
    [Tooltip("Banner height in dp (density-independent pixels)")]
    public int bannerHeight = 50;

    [Header("Ad Behavior")]
    [Tooltip("Automatically load interstitial ad after initialization")]
    public bool autoLoadInterstitialOnInit = true;
    
    [Tooltip("Automatically load interstitial ad after showing")]
    public bool autoLoadInterstitialAfterShow = true;
    
    [Tooltip("Automatically load rewarded ad after initialization")]
    public bool autoLoadRewardedOnInit = true;
    
    [Tooltip("Automatically load rewarded ad after showing")]
    public bool autoLoadRewardedAfterShow = true;
    
    [Tooltip("Show interstitial as fallback when rewarded ad is not available")]
    public bool useInterstitialAsFallback = true;

    [Header("Callbacks")]
    [Tooltip("Event name to broadcast when a rewarded ad is completed")]
    public string rewardedAdCompletedEventName = "OnRewardedAdCompleted";
}
