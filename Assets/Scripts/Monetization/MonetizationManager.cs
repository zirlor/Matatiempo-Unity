using System;
using UnityEngine;
using Matatiempo.Core;

namespace Matatiempo.Monetization
{
    public class MonetizationManager : MonoBehaviour
    {
        private const string ConfigPath = "Config/monetization";

        public const string ConsentKey = "matatiempo_consent";

        private static MonetizationManager _instance;

        public static MonetizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("MonetizationManager");
                    _instance = go.AddComponent<MonetizationManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private IAdProvider _provider;
        private bool _initialized;
        private float _cooldownSeconds;
        private float _lastRewardTime = float.MinValue;

        public bool Enabled { get; private set; }

        public bool IsRewardedReady
        {
            get
            {
                return Enabled && _initialized && _provider != null && _provider.IsReady;
            }
        }

        public bool IsRewardedAvailable
        {
            get
            {
                return IsRewardedReady && IsCooldownElapsed();
            }
        }

        public bool IsInterstitialReady
        {
            get
            {
                return Enabled && _initialized && _provider != null && _provider.IsInterstitialReady;
            }
        }

        public static bool HasConsentOfUser
        {
            get { return PlayerPrefs.HasKey(ConsentKey); }
        }

        public static bool GetConsent()
        {
            return PlayerPrefs.GetInt(ConsentKey, 0) == 1;
        }

        public static void SetConsent(bool consent)
        {
            PlayerPrefs.SetInt(ConsentKey, consent ? 1 : 0);
            PlayerPrefs.Save();
            if (_instance != null && _instance._provider != null)
            {
                _instance._provider.SetConsent(consent);
            }
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

        private void Start()
        {
            ApplyConfig(LoadConfig());
            GameEvents.StateChanged += HandleStateChanged;
            GameState current = GameManager.Instance != null ? GameManager.Instance.State : GameState.MainMenu;
            HandleStateChanged(GameState.MainMenu, current);
        }

        private void OnDestroy()
        {
            GameEvents.StateChanged -= HandleStateChanged;
        }

        private static MonetizationConfig LoadConfig()
        {
            TextAsset asset = Resources.Load<TextAsset>(ConfigPath);
            if (asset == null)
            {
                Debug.LogWarning("[Matatiempo] Config/monetization.json no encontrado. Monetización desactivada.");
                return null;
            }
            return JsonUtility.FromJson<MonetizationConfig>(asset.text);
        }

        private void ApplyConfig(MonetizationConfig config)
        {
            Enabled = config != null && config.enabled;
            _cooldownSeconds = config != null ? Mathf.Max(0f, config.rewardCooldownSeconds) : 0f;
            _initialized = true;
            _provider = config != null ? CreateProvider(config) : null;
            if (_provider != null)
            {
                _provider.SetConsent(GetConsent());
            }
            if (Enabled && _provider != null)
            {
                _provider.Initialize(config);
            }
        }

        private static IAdProvider CreateProvider(MonetizationConfig config)
        {
            if (config.provider == "levelplay")
            {
                return AdsManager.Instance;
            }
            if (config.provider == "unity_ads")
            {
#if MATATIEMPO_UNITY_ADS
                return new UnityAdsAdProvider();
#else
                Debug.LogWarning("[Matatiempo] Para Unity Ads: define MATATIEMPO_UNITY_ADS, instala com.unity.services.mediation y añade un Game ID.");
                return new NoOpAdProvider(false);
#endif
            }
            if (config.provider == "none")
            {
                return new NoOpAdProvider(true);
            }
            return new NoOpAdProvider(true);
        }

        public void ShowRewardedAd(Action<bool> onResult)
        {
            if (onResult == null)
            {
                return;
            }
            if (!IsRewardedReady || !IsCooldownElapsed())
            {
                onResult(false);
                return;
            }
            _provider.ShowRewarded(rewarded =>
            {
                _lastRewardTime = Time.unscaledTime;
                onResult(rewarded);
            });
        }

        private bool IsCooldownElapsed()
        {
            return Time.unscaledTime - _lastRewardTime >= _cooldownSeconds;
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            if (current == GameState.MainMenu || current == GameState.Playing)
            {
                ShowBanner();
            }
            else
            {
                HideBanner();
            }
            if (current == GameState.LevelCompleted || current == GameState.GameOver)
            {
                ShowInterstitial();
            }
        }

        public void ShowBanner()
        {
            if (Enabled && _initialized && _provider != null)
            {
                _provider.ShowBanner();
            }
        }

        public void HideBanner()
        {
            if (Enabled && _initialized && _provider != null)
            {
                _provider.HideBanner();
            }
        }

        public void ShowInterstitial()
        {
            if (Enabled && _initialized && _provider != null && _provider.IsInterstitialReady)
            {
                _provider.ShowInterstitial();
            }
        }
    }
}