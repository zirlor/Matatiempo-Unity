using System;
using UnityEngine;

namespace Matatiempo.Core
{
    public static class LivesManager
    {
        public const string LivesKey = "matatiempo_lives";
        public const int DefaultLives = 3;
        public const int MaxLives = 9999;
        public const int MaxRegenLives = 3;

        public const string RegenTimestampKey = "matatiempo_lives_regen_ts";
        public const int RegenIntervalSeconds = 1800;

        public static int GetLives()
        {
            int lives = Mathf.Max(0, Mathf.Min(MaxLives, PlayerPrefs.GetInt(LivesKey, DefaultLives)));
            if (lives >= MaxRegenLives)
            {
                ClearRegenTimestamp();
                return lives;
            }

            long now = NowSeconds();
            long last = GetRegenTimestamp();
            if (last <= 0 || now < last)
            {
                SetRegenTimestamp(now);
                return lives;
            }

            long elapsed = now - last;
            if (elapsed < RegenIntervalSeconds)
            {
                return lives;
            }

            long grant = elapsed / RegenIntervalSeconds;
            int newLives = (int)Math.Min((long)MaxRegenLives, (long)lives + grant);
            PlayerPrefs.SetInt(LivesKey, newLives);
            if (newLives >= MaxRegenLives)
            {
                ClearRegenTimestamp();
            }
            else
            {
                SetRegenTimestamp(last + grant * RegenIntervalSeconds);
            }
            PlayerPrefs.Save();
            if (newLives > lives)
            {
                GameEvents.NotifyLivesChanged(newLives);
            }
            return newLives;
        }

        public static bool HasLives()
        {
            return GetLives() > 0;
        }

        public static int AddLives(int amount)
        {
            int lives = Mathf.Max(0, Mathf.Min(MaxLives, GetLives() + amount));
            PlayerPrefs.SetInt(LivesKey, lives);
            if (lives >= MaxLives)
            {
                ClearRegenTimestamp();
            }
            else
            {
                EnsureRegenTimestamp();
            }
            PlayerPrefs.Save();
            return lives;
        }

        public static int GetSecondsUntilNextLife()
        {
            if (GetLives() >= MaxRegenLives)
            {
                return 0;
            }
            long last = GetRegenTimestamp();
            if (last <= 0)
            {
                return RegenIntervalSeconds;
            }
            long remain = RegenIntervalSeconds - (NowSeconds() - last);
            if (remain < 0)
            {
                remain = 0;
            }
            if (remain > RegenIntervalSeconds)
            {
                remain = RegenIntervalSeconds;
            }
            return (int)remain;
        }

        private static long NowSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private static long GetRegenTimestamp()
        {
            long value;
            if (!long.TryParse(PlayerPrefs.GetString(RegenTimestampKey, "0"), out value) || value < 0)
            {
                return 0;
            }
            return value;
        }

        private static void SetRegenTimestamp(long timestamp)
        {
            PlayerPrefs.SetString(RegenTimestampKey, timestamp.ToString());
            PlayerPrefs.Save();
        }

        private static void EnsureRegenTimestamp()
        {
            if (GetRegenTimestamp() <= 0)
            {
                SetRegenTimestamp(NowSeconds());
            }
        }

        private static void ClearRegenTimestamp()
        {
            if (PlayerPrefs.HasKey(RegenTimestampKey) && GetRegenTimestamp() > 0)
            {
                SetRegenTimestamp(0);
            }
        }
    }
}
