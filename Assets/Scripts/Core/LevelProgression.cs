namespace Matatiempo.Core
{
    public static class LevelProgression
    {
        public const int FirstLevel = 1;
        public const int MaxLevel = 5;
        public const int TimePerWordSeconds = 150;

        public static int WordsForLevel(int level)
        {
            if (level <= FirstLevel)
            {
                return 1;
            }
            return 1 << (level - 1);
        }

        public static int TimeSecondsForWord(int level, string word)
        {
            return TimePerWordSeconds;
        }
    }
}