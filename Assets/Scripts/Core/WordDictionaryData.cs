using System;

namespace Matatiempo.Core
{
    [Serializable]
    public class WordDictionaryData
    {
        public string schemaVersion;
        public string language;
        public string languageName;
        public ProgressionData progression;
        public WordLevelData[] levels;
    }

    [Serializable]
    public class ProgressionData
    {
        public string type;
        public string formula;
        public string description;
    }

    [Serializable]
    public class WordEntry
    {
        public string word;
        public string clue;
    }

    [Serializable]
    public class WordLevelData
    {
        public int level;
        public WordEntry[] words;
    }
}