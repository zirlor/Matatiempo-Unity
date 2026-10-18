using System;

namespace Matatiempo.Monetization
{
    public interface IAdProvider
    {
        bool IsReady { get; }
        bool IsInterstitialReady { get; }
        void Initialize(MonetizationConfig config);
        void ShowBanner();
        void HideBanner();
        void ShowInterstitial();
        void ShowRewarded(Action<bool> onResult);
        void SetConsent(bool consent);
    }
}