using System;
using UnityEngine;

namespace Matatiempo.Monetization
{
    public sealed class NoOpAdProvider : IAdProvider
    {
        private readonly bool _rewardResult;

        public NoOpAdProvider(bool rewardResult = true)
        {
            _rewardResult = rewardResult;
        }

        public bool IsReady
        {
            get { return true; }
        }

        public bool IsInterstitialReady
        {
            get { return true; }
        }

        public void Initialize(MonetizationConfig config)
        {
            Debug.Log("[Matatiempo] NoOpAdProvider activo. rewardResult=" + _rewardResult);
        }

        public void ShowBanner()
        {
        }

        public void HideBanner()
        {
        }

        public void ShowInterstitial()
        {
        }

        public void ShowRewarded(Action<bool> onResult)
        {
            if (onResult != null)
            {
                onResult(_rewardResult);
            }
        }

        public void SetConsent(bool consent)
        {
        }
    }
}