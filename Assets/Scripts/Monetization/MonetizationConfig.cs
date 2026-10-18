using System;
using UnityEngine;

namespace Matatiempo.Monetization
{
    [Serializable]
    public class MonetizationConfig
    {
        public bool enabled;
        public string provider;
        public string appKey;
        public string rewardedAdUnitId;
        public string bannerAdUnitId;
        public string interstitialAdUnitId;
        public float rewardCooldownSeconds;
    }
}