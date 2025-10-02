using System;
using com.unity3d.mediation;
using UnityEngine;
using LevelPlayBannerAd = Unity.Services.LevelPlay.LevelPlayBannerAd;
using LevelPlayReward = Unity.Services.LevelPlay.LevelPlayReward;
using LevelPlayRewardedAd = Unity.Services.LevelPlay.LevelPlayRewardedAd;

/// <summary>
/// Generic AdsManager that handles LevelPlay ad integration.
/// Uses ScriptableObject configuration for easy customization across different games.
/// </summary>
public class AdsManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AdConfig adConfig;
    public  event Action OnRewardedAdWatchSuccessFull;
    public  event Action OnRewardedAdWatchFailed;
    public event Action OnInterstitialAdClosedEvent;
    
    // Ad instances
    private LevelPlayInterstitialAd _interstitialAd;
    private LevelPlayBannerAd _bannerAd;
    private LevelPlayRewardedAd _rewardedAd;
    private bool _isBannerVisible = false;
    private bool _rewardedAdSeenCompletely = false;
    
    // Ad formats
    private readonly LevelPlayAdFormat[] _adFormats = new[] { 
        LevelPlayAdFormat.REWARDED, 
        LevelPlayAdFormat.INTERSTITIAL, 
        LevelPlayAdFormat.BANNER 
    };
    
    // Properties
    public bool IsRewardedAdAvailable => (bool)_rewardedAd?.IsAdReady();
    public bool IsInterstitialAdAvailable => _interstitialAd != null && _interstitialAd.IsAdReady();
    public bool IsBannerAdVisible => _isBannerVisible;

    public bool AreBannerAdsEnabled => true;
    
    // Singleton instance
    private static AdsManager _instance;
    public static AdsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AdsManager>();
                if (_instance == null)
                {
                    Debug.LogError("No AdsManager found in the scene. Please add one.");
                }
            }
            return _instance;
        }
    }
    
    #region Unity Lifecycle Methods
    
    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Validate configuration
        if (adConfig == null)
        {
            Debug.LogError("AdConfig is not assigned to AdsManager. Please assign a configuration in the inspector.");
            return;
        }
        
        // Set test mode if enabled
        if (adConfig.testMode)
        {
            IronSource.Agent.setMetaData("is_test_suite", "enable");
        }
    }
    
    private void Start()
    {
#if DEVELOPMENT_BUILD
        // Validate integration
        IronSource.Agent.validateIntegration();
#endif
        IronSource.Agent.setConsent(true);
        IronSource.Agent.setMetaData("do_not_sell","false");
        IronSource.Agent.setMetaData("is_child_directed","false");
        Initialize();
    }

    private void Initialize()
    {
        // Initialize LevelPlay with the appropriate app key
        string appKey = GetAppKey();
        LevelPlay.Init(appKey, GetUserId(), _adFormats);
    }

    private void OnEnable()
    {
        // Subscribe to SDK initialization events
        LevelPlay.OnInitSuccess += OnSdkInitializationCompleted;
        LevelPlay.OnInitFailed += OnSdkInitializationFailed;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from SDK initialization events
        LevelPlay.OnInitSuccess -= OnSdkInitializationCompleted;
        LevelPlay.OnInitFailed -= OnSdkInitializationFailed;
        
        // Unsubscribe from interstitial events and destroy the ad
        CleanupInterstitialAd();
        CleanupRewardedAd();
        CleanupBannerAd();
    }
    
    private void OnApplicationPause(bool paused)
    {
        IronSource.Agent.onApplicationPause(paused);
    }
    
    #endregion
    
    #region Initialization
    
    private void OnSdkInitializationCompleted(LevelPlayConfiguration levelPlayConfiguration)
    {
        Debug.Log("LevelPlay SDK initialized successfully");
        
        // Launch test suite if enabled
        if (adConfig.launchTestSuite)
        {
            IronSource.Agent.launchTestSuite();
        }
        
        // Initialize ads
        InitializeInterstitialAd();
        InitializeRewardedAd();
        InitializeBannerAd();
        
        // Auto-load ads if configured
        if (adConfig.autoLoadInterstitialOnInit)
        {
            LoadInterstitialAd();
        }
        
        if (adConfig.autoLoadRewardedOnInit)
        {
            LoadRewardedAd();
        }
        
        // Auto-load and show banner if configured
        if (adConfig.autoLoadBannerOnInit)
        {
            LoadBannerAd();
        }
    }

    private void OnSdkInitializationFailed(LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay SDK initialization failed: {error.ErrorMessage}");
    }
    
    private string GetAppKey()
    {
        #if UNITY_IPHONE
            return adConfig.iOSAppKey;
        #elif UNITY_ANDROID
            return adConfig.androidAppKey;
        #else
            return "unknown_platform";
        #endif
    }
    
    private string GetUserId()
    {
        return SystemInfo.deviceUniqueIdentifier;
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Shows a rewarded ad if available, otherwise shows an interstitial ad if configured to do so.
    /// </summary>
    /// <returns>True if an ad was shown, false otherwise.</returns>
    public bool ShowAd()
    {
        if (IsRewardedAdAvailable)
        {
            ShowRewardedAd();
            return true;
        }
        else if (adConfig.useInterstitialAsFallback && IsInterstitialAdAvailable)
        {
            ShowInterstitialAd();
            return true;
        }
        
        Debug.Log("No ads available to show");
        return false;
    }
    
    /// <summary>
    /// Shows a rewarded ad if available.
    /// </summary>
    /// <returns>True if the ad was shown, false otherwise.</returns>
    public bool ShowRewardedAd()
    {
        if (IsRewardedAdAvailable)
        {
            _rewardedAdSeenCompletely = false;
            _rewardedAd?.ShowAd();
            return true;
        }
        
        Debug.Log("Rewarded ad not available");
        return false;
    }
    
    /// <summary>
    /// Shows an interstitial ad if available.
    /// </summary>
    /// <returns>True if the ad was shown, false otherwise.</returns>
    public bool ShowInterstitialAd()
    {
        if (IsInterstitialAdAvailable)
        {
            _interstitialAd.ShowAd();
            return true;
        }
        
        Debug.Log("Interstitial ad not available");
        return false;
    }
    
    /// <summary>
    /// Loads a rewarded ad.
    /// </summary>
    public void LoadRewardedAd()
    {
        if (_rewardedAd != null) {
            _rewardedAd.LoadAd();
        }else {
            Debug.LogWarning("Tried to load rewarded ad before initialization");
        }
    }
    
    /// <summary>
    /// Loads an interstitial ad.
    /// </summary>
    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.LoadAd();
        }
        else
        {
            Debug.LogWarning("Tried to load interstitial ad before initialization");
        }
    }
    
    /// <summary>
    /// Shows a banner ad at the position specified in the configuration.
    /// </summary>
    public void LoadBannerAd()
    {
        if (_bannerAd != null) {
            _bannerAd.LoadAd();
        }else {
            Debug.LogWarning("Tried to show banner ad before initialization");
        }
        
        Debug.Log($"Banner ad requested with position: {adConfig.bannerPosition}");
    }
    
    public void ShowBannerAd() {
        if (_bannerAd != null) {
            _bannerAd.ShowAd();
        }else {
            Debug.LogWarning("Tried to show banner ad before initialization");
        }
    }
    
    /// <summary>
    /// Hides the banner ad if it's visible.
    /// </summary>
    public void HideBannerAd()
    {
        if (_bannerAd == null)
        {
            Debug.Log("Banner ad is not visible");
            return;
        }
        _bannerAd.HideAd();
        Debug.Log("Banner ad hidden");
    }
    
    #endregion
    
    #region Interstitial Ad Methods
    
    private void InitializeInterstitialAd()
    {
        // Clean up existing interstitial ad if any
        CleanupInterstitialAd();
        
        // Create new interstitial ad
        _interstitialAd = new LevelPlayInterstitialAd(adConfig.interstitialAdUnitId);
        
        // Subscribe to events
        _interstitialAd.OnAdLoaded += OnInterstitialAdLoaded;
        _interstitialAd.OnAdLoadFailed += OnInterstitialAdLoadFailed;
        _interstitialAd.OnAdDisplayed += OnInterstitialAdDisplayed;
        _interstitialAd.OnAdDisplayFailed += OnInterstitialAdDisplayFailed;
        _interstitialAd.OnAdClicked += OnInterstitialAdClicked;
        _interstitialAd.OnAdClosed += OnInterstitialAdClosed;
        _interstitialAd.OnAdInfoChanged += OnInterstitialAdInfoChanged;
    }
    
    private void CleanupInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            // Unsubscribe from events
            _interstitialAd.OnAdLoaded -= OnInterstitialAdLoaded;
            _interstitialAd.OnAdLoadFailed -= OnInterstitialAdLoadFailed;
            _interstitialAd.OnAdDisplayed -= OnInterstitialAdDisplayed;
            _interstitialAd.OnAdDisplayFailed -= OnInterstitialAdDisplayFailed;
            _interstitialAd.OnAdClicked -= OnInterstitialAdClicked;
            _interstitialAd.OnAdClosed -= OnInterstitialAdClosed;
            _interstitialAd.OnAdInfoChanged -= OnInterstitialAdInfoChanged;
            
            // Destroy the ad
            _interstitialAd.DestroyAd();
            _interstitialAd = null;
        }
    }
    
    #endregion
    
    #region Rewarded Ad Methods
    private void InitializeRewardedAd()
    {
        CleanupRewardedAd();
        
        // Create new rewarded ad
        _rewardedAd = new LevelPlayRewardedAd(adConfig.rewardedAdUnitId);
        
        // Subscribe to events
        _rewardedAd.OnAdLoaded += OnRewardedAdLoaded;
        _rewardedAd.OnAdLoadFailed += OnRewardedAdLoadFailed;
        _rewardedAd.OnAdDisplayed += OnRewardedAdDisplayed;
        _rewardedAd.OnAdDisplayFailed += OnRewardedAdDisplayFailed;
        _rewardedAd.OnAdRewarded += OnRewardedVideoAdRewarded; 
        _rewardedAd.OnAdClosed += OnRewardedVideoAdClosed;
// Optional 
        _rewardedAd.OnAdClicked += OnRewardedOnAdClickedEvent;
        _rewardedAd.OnAdInfoChanged += OnRewardedOnAdInfoChangedEvent;
    }

    private void CleanupRewardedAd()
    {
        if (_rewardedAd != null)
        {
            // Unsubscribe from events
            _rewardedAd.OnAdLoaded -= OnRewardedAdLoaded;
            _rewardedAd.OnAdLoadFailed -= OnRewardedAdLoadFailed;
            _rewardedAd.OnAdDisplayed -= OnRewardedAdDisplayed;
            _rewardedAd.OnAdDisplayFailed -= OnRewardedAdDisplayFailed;
            _rewardedAd.OnAdRewarded -= OnRewardedVideoAdRewarded; 
            _rewardedAd.OnAdClosed -= OnRewardedVideoAdClosed;
// Optional 
            _rewardedAd.OnAdClicked -= OnRewardedOnAdClickedEvent;
            _rewardedAd.OnAdInfoChanged -= OnRewardedOnAdInfoChangedEvent;
            
            // Destroy the ad
            _rewardedAd.DestroyAd();
            _rewardedAd = null;
        }
    }

    #endregion

    #region Banner Ad Methods
    
    private void InitializeBannerAd()
    {
        // Clean up existing banner ad if any
        CleanupBannerAd();
        
        // Create new banner ad
        _bannerAd = new LevelPlayBannerAd(adConfig.bannerAdUnitId,position:adConfig.bannerPosition);
        
        // Subscribe to events
        _bannerAd.OnAdLoaded += OnBannerAdLoaded;
        _bannerAd.OnAdLoadFailed += OnBannerAdLoadFailed;
        _bannerAd.OnAdClicked += OnBannerAdClicked;
        _bannerAd.OnAdLeftApplication += OnBannerAdLeftApplication;
        _bannerAd.OnAdDisplayFailed += OnBannerAdDisplayFailed;
    }

    private void OnBannerAdDisplayFailed(LevelPlayAdDisplayInfoError obj)
    {
        Debug.Log($"Banner ad display failed: {obj.LevelPlayError.ErrorMessage}");
        if (_bannerAd != null) {
            if (!_isBannerVisible) {
                _bannerAd.LoadAd();
            }
            
        }
    }

    private void CleanupBannerAd()
    {
        if (_bannerAd != null)
        {
            // Unsubscribe from events
            _bannerAd.OnAdLoaded -= OnBannerAdLoaded;
            _bannerAd.OnAdLoadFailed -= OnBannerAdLoadFailed;
            _bannerAd.OnAdClicked -= OnBannerAdClicked;
            _bannerAd.OnAdLeftApplication -= OnBannerAdLeftApplication;
            
            // Destroy the ad
            _bannerAd.DestroyAd();
            _bannerAd = null;
        }
    }

    #endregion
    
    #region Interstitial Ad Event Handlers
    
    private void OnInterstitialAdLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Interstitial ad loaded: {adInfo.AdNetwork}");
    }
    
    private void OnInterstitialAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning($"Interstitial ad load failed: {error.ErrorMessage}");
        LoadInterstitialAd();
    }
    
    private void OnInterstitialAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Interstitial ad displayed: {adInfo.AdNetwork}");
    }
    
    private void OnInterstitialAdDisplayFailed(LevelPlayAdDisplayInfoError error)
    {
        Debug.LogWarning($"Interstitial ad display failed: {error.LevelPlayError.ErrorMessage}");
        LoadInterstitialAd();
    }
    
    private void OnInterstitialAdClicked(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Interstitial ad clicked: {adInfo.AdNetwork}");
    }
    
    private void OnInterstitialAdClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Interstitial ad closed: {adInfo.AdNetwork}");
        
        // Auto-load another interstitial ad if configured
        if (adConfig.autoLoadInterstitialAfterShow)
        {
            LoadInterstitialAd();
        }
        OnInterstitialAdClosedEvent?.Invoke();
    }
    
    private void OnInterstitialAdInfoChanged(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Interstitial ad info changed: {adInfo.AdNetwork}");
    }
    
    #endregion
    
    #region Rewarded Ad Event Handlers
    
    private void OnRewardedVideoAdRewarded(Unity.Services.LevelPlay.LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log($"Rewarded ad completed: {adInfo.adNetwork}");
        
        // Invoke the Unity Event for reward completion
        _rewardedAdSeenCompletely = true;
        OnRewardedAdWatchSuccessFull?.Invoke();
        
        // Broadcast the event for other scripts to listen to
        if (!string.IsNullOrEmpty(adConfig.rewardedAdCompletedEventName))
        {
            BroadcastMessage(adConfig.rewardedAdCompletedEventName, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    private void OnRewardedAdClicked(LevelPlayAdInfo obj)
    {
        Debug.Log($"Rewarded ad clicked: {obj.AdNetwork}");
    }

    private void OnRewardedAdDisplayFailed(LevelPlayAdDisplayInfoError obj)
    {
        OnRewardedAdWatchFailed?.Invoke();
        LoadRewardedAd();
        Debug.Log($"Rewarded ad display failed: {obj.LevelPlayError.ErrorMessage}");
    }

    private void OnRewardedAdDisplayed(LevelPlayAdInfo obj)
    {
        Debug.Log($"Rewarded ad displayed: {obj.AdNetwork}");
    }

    private void OnRewardedAdLoadFailed(LevelPlayAdError obj)
    {
        LoadRewardedAd();
        Debug.Log($"Rewarded ad load failed: {obj.ErrorMessage}");      
    }

    private void OnRewardedAdLoaded(LevelPlayAdInfo obj)
    {
        Debug.Log($"Rewarded ad loaded: {obj.AdNetwork}");
    }
    
    private void OnRewardedVideoAdClosed(LevelPlayAdInfo obj)
    {
        if (!_rewardedAdSeenCompletely) {
            OnRewardedAdWatchFailed?.Invoke();
        }
        
        LoadRewardedAd();
        Debug.Log($"Rewarded ad closed: {obj.AdNetwork}");
    }
    
    private void OnRewardedOnAdInfoChangedEvent(LevelPlayAdInfo obj)
    {
        Debug.Log($"Rewarded ad info changed: {obj.AdNetwork}");
    }

    private void OnRewardedOnAdClickedEvent(LevelPlayAdInfo obj)
    {
        Debug.Log($"Rewarded ad clicked: {obj.AdNetwork}");
    }
    
    #endregion
    
    #region Banner Ad Event Handlers
    
    private void OnBannerAdLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Banner ad loaded: {adInfo.adNetwork}");
        ShowBannerAd();
        _isBannerVisible = true;
    }
    
    private void OnBannerAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning($"Banner ad load failed: {error.ErrorMessage}");
        _bannerAd.LoadAd();
    }
    
    private void OnBannerAdClicked(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Banner ad clicked: {adInfo.adNetwork}");
    }
    
    private void OnBannerAdScreenPresented(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Banner ad screen presented: {adInfo.adNetwork}");
    }
    
    private void OnBannerAdScreenDismissed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Banner ad screen dismissed: {adInfo.adNetwork}");
    }
    
    private void OnBannerAdLeftApplication(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Banner ad left application: {adInfo.adNetwork}");
    }
    
    #endregion
}