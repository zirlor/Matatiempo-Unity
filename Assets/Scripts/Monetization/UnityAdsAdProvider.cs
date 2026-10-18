#if MATATIEMPO_UNITY_ADS
using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Mediation;

namespace Matatiempo.Monetization
{
    public sealed class UnityAdsAdProvider : IAdProvider
    {
        private IRewardedAd _rewardedAd;
        private bool _ready;
        private Action<bool> _pendingResult;

        public bool IsReady
        {
            get { return _ready; }
        }

        public bool IsInterstitialReady
        {
            get { return false; }
        }

        public void ShowBanner()
        {
            Debug.Log("[Matatiempo] El provider unity_ads no gestiona banners. Usa provider \"levelplay\".");
        }

        public void HideBanner()
        {
        }

        public void ShowInterstitial()
        {
            Debug.Log("[Matatiempo] El provider unity_ads no gestiona interstitials. Usa provider \"levelplay\".");
        }

        public void SetConsent(bool consent)
        {
        }

        public void Initialize(MonetizationConfig config)
        {
            _ = InitializeAsync(config.rewardedAdUnitId);
        }

        private async Task InitializeAsync(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
            {
                Debug.LogError("[Matatiempo] rewardedAdUnitId vacío en monetization.json.");
                return;
            }
            try
            {
                InitializationOptions options = new InitializationOptions();
                await UnityServices.InitializeAsync(options);
                MediationService.Instance.Initialize(options);
                _rewardedAd = MediationService.Instance.CreateRewardedAd(adUnitId);
                _rewardedAd.OnLoaded += HandleLoaded;
                _rewardedAd.OnFailedLoad += HandleLoadFailed;
                _rewardedAd.OnShowed += HandleShowed;
                _rewardedAd.OnFailedShow += HandleShowFailed;
                _rewardedAd.OnUserRewarded += HandleUserRewarded;
                _rewardedAd.OnClosed += HandleClosed;
                await _rewardedAd.LoadAsync();
            }
            catch (Exception e)
            {
                Debug.LogError("[Matatiempo] Inicialización de anuncios falló: " + e.Message);
                _ready = false;
            }
        }

        public void ShowRewarded(Action<bool> onResult)
        {
            if (onResult == null)
            {
                return;
            }
            if (_rewardedAd == null || !_ready)
            {
                onResult(false);
                return;
            }
            _pendingResult = onResult;
            ShowAsync();
        }

        private async void ShowAsync()
        {
            try
            {
                await _rewardedAd.ShowAsync();
            }
            catch (Exception e)
            {
                Debug.LogError("[Matatiempo] Error al mostrar anuncio: " + e.Message);
                Complete(false);
            }
        }

        private void HandleLoaded(object sender, EventArgs e)
        {
            _ready = true;
        }

        private void HandleLoadFailed(object sender, LoadErrorEventArgs e)
        {
            _ready = false;
        }

        private void HandleShowed(object sender, EventArgs e)
        {
        }

        private void HandleShowFailed(object sender, ShowErrorEventArgs e)
        {
            Complete(false);
        }

        private void HandleUserRewarded(object sender, RewardEventArgs e)
        {
            Complete(true);
        }

        private void HandleClosed(object sender, EventArgs e)
        {
            Complete(false);
            if (_rewardedAd != null)
            {
                _ = _rewardedAd.LoadAsync();
            }
        }

        private void Complete(bool rewarded)
        {
            Action<bool> callback = _pendingResult;
            _pendingResult = null;
            if (callback != null)
            {
                callback(rewarded);
            }
        }
    }
}
#endif