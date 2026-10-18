using System;
using UnityEngine;

#if MATATIEMPO_UNITY_LEVELPLAY
using Unity.Services.LevelPlay;
#endif

namespace Matatiempo.Monetization
{
    public sealed class AdsManager : MonoBehaviour, IAdProvider
    {
        private const string DefaultAppKey = "YOUR_LEVELPLAY_APP_KEY";
        private const string DefaultRewardedAdUnitId = "5zjcfzq5np5mj6qg";
        private const string DefaultBannerAdUnitId = "1ji3b1u6hnzj923l";
        private const string DefaultInterstitialAdUnitId = "5zz05thtll04u9ui";

        private static AdsManager _instance;

        public static AdsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AdsManager");
                    _instance = go.AddComponent<AdsManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private bool _initialized;
        private bool _rewardedReady;
        private bool _interstitialReady;
        private bool _rewardGranted;
        private bool _eventsSubscribed;
        private bool _consent;
        private Action<bool> _pendingReward;
        private string _appKey = DefaultAppKey;
        private string _rewardedAdUnitId = DefaultRewardedAdUnitId;
        private string _bannerAdUnitId = DefaultBannerAdUnitId;
        private string _interstitialAdUnitId = DefaultInterstitialAdUnitId;

#if MATATIEMPO_UNITY_LEVELPLAY
        private LevelPlayRewardedAd _rewardedAd;
        private LevelPlayInterstitialAd _interstitialAd;
        private LevelPlayBannerAd _bannerAd;
#endif

        public bool IsReady
        {
            get { return _initialized && _rewardedReady; }
        }

        public bool IsInterstitialReady
        {
            get { return _initialized && _interstitialReady; }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            if (_instance == null)
            {
                _instance = this;
            }
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize(MonetizationConfig config)
        {
            if (config != null)
            {
                _appKey = string.IsNullOrEmpty(config.appKey) ? DefaultAppKey : config.appKey;
                _rewardedAdUnitId = string.IsNullOrEmpty(config.rewardedAdUnitId) ? DefaultRewardedAdUnitId : config.rewardedAdUnitId;
                _bannerAdUnitId = string.IsNullOrEmpty(config.bannerAdUnitId) ? DefaultBannerAdUnitId : config.bannerAdUnitId;
                _interstitialAdUnitId = string.IsNullOrEmpty(config.interstitialAdUnitId) ? DefaultInterstitialAdUnitId : config.interstitialAdUnitId;
            }

            Debug.Log($"[Matatiempo] LevelPlay inicializando (appKey='{_appKey}', rewarded='{_rewardedAdUnitId}', banner='{_bannerAdUnitId}', interstitial='{_interstitialAdUnitId}').");

            if (_appKey == DefaultAppKey)
            {
                Debug.LogWarning("[Matatiempo] App Key de LevelPlay sin configurar. Completa 'appKey' en Config/monetization.json.");
            }

#if MATATIEMPO_UNITY_LEVELPLAY
            if (_initialized)
            {
                return;
            }
            _eventsSubscribed = true;
            LevelPlay.OnInitSuccess += HandleSdkInitSuccess;
            LevelPlay.OnInitFailed += HandleSdkInitFailed;
            LevelPlay.SetConsent(_consent);
            LevelPlay.Init(_appKey);
#else
            Debug.LogWarning("[Matatiempo] Compilación sin LevelPlay: define MATATIEMPO_UNITY_LEVELPLAY e instala el paquete com.unity.services.levelplay para publicidad real.");
#endif
        }

        public void SetConsent(bool consent)
        {
            _consent = consent;
#if MATATIEMPO_UNITY_LEVELPLAY
            if (_initialized)
            {
                LevelPlay.SetConsent(consent);
            }
#endif
        }

        public void ShowBanner()
        {
#if MATATIEMPO_UNITY_LEVELPLAY
            if (_initialized && _bannerAd != null)
            {
                _bannerAd.ShowAd();
            }
#endif
        }

        public void HideBanner()
        {
#if MATATIEMPO_UNITY_LEVELPLAY
            if (_initialized && _bannerAd != null)
            {
                _bannerAd.HideAd();
            }
#endif
        }

        public void ShowInterstitial()
        {
#if MATATIEMPO_UNITY_LEVELPLAY
            if (_initialized && _interstitialReady && _interstitialAd != null)
            {
                _interstitialReady = false;
                _interstitialAd.ShowAd();
            }
#endif
        }

        public void ShowRewarded(Action<bool> onResult)
        {
            if (onResult == null)
            {
                return;
            }
#if MATATIEMPO_UNITY_LEVELPLAY
            if (!_rewardedReady || _rewardedAd == null)
            {
                onResult(false);
                return;
            }
            _pendingReward = onResult;
            _rewardGranted = false;
            _rewardedAd.ShowAd();
#else
            onResult(false);
#endif
        }

#if MATATIEMPO_UNITY_LEVELPLAY
        private void HandleSdkInitSuccess(LevelPlayConfiguration configuration)
        {
            _initialized = true;
            Debug.Log("[Matatiempo] LevelPlay SDK inicializado. Creando anuncios.");
            CreateAds();
        }

        private void HandleSdkInitFailed(LevelPlayInitError error)
        {
            Debug.LogWarning("[Matatiempo] Fallo al inicializar LevelPlay: " + (error != null ? error.ErrorMessage : "desconocido"));
        }

        private void CreateAds()
        {
            _rewardedAd = new LevelPlayRewardedAd(_rewardedAdUnitId);
            _rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
            _rewardedAd.OnAdLoadFailed += HandleRewardedAdLoadFailed;
            _rewardedAd.OnAdDisplayed += HandleRewardedAdDisplayed;
            _rewardedAd.OnAdDisplayFailed += HandleRewardedAdDisplayFailed;
            _rewardedAd.OnAdClosed += HandleRewardedAdClosed;
            _rewardedAd.OnAdRewarded += HandleRewardedAdRewarded;
            _rewardedAd.LoadAd();

            _interstitialAd = new LevelPlayInterstitialAd(_interstitialAdUnitId);
            _interstitialAd.OnAdLoaded += HandleInterstitialAdLoaded;
            _interstitialAd.OnAdLoadFailed += HandleInterstitialAdLoadFailed;
            _interstitialAd.OnAdDisplayFailed += HandleInterstitialAdDisplayFailed;
            _interstitialAd.OnAdClosed += HandleInterstitialAdClosed;
            _interstitialAd.LoadAd();

            LevelPlayBannerAd.Config bannerConfig = new LevelPlayBannerAd.Config.Builder()
                .SetSize(LevelPlayAdSize.BANNER)
                .SetPosition(LevelPlayBannerPosition.BottomCenter)
                .Build();

            _bannerAd = new LevelPlayBannerAd(_bannerAdUnitId, bannerConfig);
            _bannerAd.OnAdLoaded += HandleBannerAdLoaded;
            _bannerAd.OnAdLoadFailed += HandleBannerAdLoadFailed;
            _bannerAd.LoadAd();
        }

        private void OnDestroy()
        {
            if (!_eventsSubscribed)
            {
                return;
            }
            _eventsSubscribed = false;

            LevelPlay.OnInitSuccess -= HandleSdkInitSuccess;
            LevelPlay.OnInitFailed -= HandleSdkInitFailed;

            if (_rewardedAd != null)
            {
                _rewardedAd.OnAdLoaded -= HandleRewardedAdLoaded;
                _rewardedAd.OnAdLoadFailed -= HandleRewardedAdLoadFailed;
                _rewardedAd.OnAdDisplayed -= HandleRewardedAdDisplayed;
                _rewardedAd.OnAdDisplayFailed -= HandleRewardedAdDisplayFailed;
                _rewardedAd.OnAdClosed -= HandleRewardedAdClosed;
                _rewardedAd.OnAdRewarded -= HandleRewardedAdRewarded;
                _rewardedAd.DestroyAd();
            }

            if (_interstitialAd != null)
            {
                _interstitialAd.OnAdLoaded -= HandleInterstitialAdLoaded;
                _interstitialAd.OnAdLoadFailed -= HandleInterstitialAdLoadFailed;
                _interstitialAd.OnAdDisplayFailed -= HandleInterstitialAdDisplayFailed;
                _interstitialAd.OnAdClosed -= HandleInterstitialAdClosed;
                _interstitialAd.DestroyAd();
            }

            if (_bannerAd != null)
            {
                _bannerAd.OnAdLoaded -= HandleBannerAdLoaded;
                _bannerAd.OnAdLoadFailed -= HandleBannerAdLoadFailed;
                _bannerAd.DestroyAd();
            }
        }

        private void HandleRewardedAdLoaded(LevelPlayAdInfo info)
        {
            _rewardedReady = true;
            Debug.Log("[Matatiempo] Rewarded de LevelPlay listo para mostrarse.");
        }

        private void HandleRewardedAdLoadFailed(LevelPlayAdError error)
        {
            _rewardedReady = false;
            Debug.LogWarning("[Matatiempo] Rewarded no disponible: " + (error != null ? error.ErrorMessage : "desconocido"));
        }

        private void HandleRewardedAdDisplayed(LevelPlayAdInfo info)
        {
        }

        private void HandleRewardedAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error)
        {
            CompleteReward(false);
            _rewardedReady = false;
            if (_rewardedAd != null)
            {
                _rewardedAd.LoadAd();
            }
        }

        private void HandleRewardedAdClosed(LevelPlayAdInfo info)
        {
            if (_pendingReward != null && !_rewardGranted)
            {
                CompleteReward(false);
            }
            _rewardedReady = false;
            if (_rewardedAd != null)
            {
                _rewardedAd.LoadAd();
            }
        }

        private void HandleRewardedAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
        {
            _rewardGranted = true;
            CompleteReward(true);
        }

        private void HandleInterstitialAdLoaded(LevelPlayAdInfo info)
        {
            _interstitialReady = true;
            Debug.Log("[Matatiempo] Interstitial de LevelPlay listo para mostrarse.");
        }

        private void HandleInterstitialAdLoadFailed(LevelPlayAdError error)
        {
            _interstitialReady = false;
            Debug.LogWarning("[Matatiempo] Interstitial no disponible: " + (error != null ? error.ErrorMessage : "desconocido"));
        }

        private void HandleInterstitialAdClosed(LevelPlayAdInfo info)
        {
            _interstitialReady = false;
            if (_interstitialAd != null)
            {
                _interstitialAd.LoadAd();
            }
        }

        private void HandleInterstitialAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error)
        {
            _interstitialReady = false;
            if (_interstitialAd != null)
            {
                _interstitialAd.LoadAd();
            }
        }

        private void HandleBannerAdLoaded(LevelPlayAdInfo info)
        {
            Debug.Log("[Matatiempo] Banner de LevelPlay cargado.");
        }

        private void HandleBannerAdLoadFailed(LevelPlayAdError error)
        {
            Debug.LogWarning("[Matatiempo] Fallo al cargar banner: " + (error != null ? error.ErrorMessage : "desconocido"));
        }

        private void CompleteReward(bool rewarded)
        {
            Action<bool> callback = _pendingReward;
            _pendingReward = null;
            if (callback != null)
            {
                callback(rewarded);
            }
        }
#endif
    }
}