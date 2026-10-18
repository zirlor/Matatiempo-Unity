using UnityEngine;

namespace Matatiempo.Core
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public const string MaxLevelKey = "MaxLevelUnlocked";
        public const int DefaultLevel = 1;
        public const int MaxProgressionLevel = 10;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public int GetMaxAvailableLevel()
        {
            return LevelProgression.MaxLevel;
        }

        public int GetMaxLevelUnlocked()
        {
            int unlocked = PlayerPrefs.GetInt(MaxLevelKey, DefaultLevel);
            int maxAvailable = GetMaxAvailableLevel();
            if (unlocked < DefaultLevel)
            {
                unlocked = DefaultLevel;
            }
            int cap = Mathf.Min(maxAvailable, MaxProgressionLevel);
            if (unlocked > cap)
            {
                unlocked = cap;
            }
            return unlocked;
        }

        public bool IsLevelUnlocked(int level)
        {
            return level >= DefaultLevel && level <= GetMaxLevelUnlocked();
        }

        public int CompleteLevel(int currentLevel)
        {
            int unlocked = GetMaxLevelUnlocked();
            int maxAvailable = GetMaxAvailableLevel();
            if (currentLevel >= unlocked && currentLevel < maxAvailable)
            {
                unlocked = currentLevel + 1;
                if (unlocked > MaxProgressionLevel)
                {
                    unlocked = MaxProgressionLevel;
                }
                PlayerPrefs.SetInt(MaxLevelKey, unlocked);
                PlayerPrefs.Save();
            }
            return unlocked;
        }
    }
}