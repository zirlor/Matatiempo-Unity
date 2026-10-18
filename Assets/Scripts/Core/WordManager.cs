using System.Collections.Generic;
using UnityEngine;

namespace Matatiempo.Core
{
    public class WordManager : MonoBehaviour
    {
        public static WordManager Instance { get; private set; }

        public int TotalWords { get; private set; }
        public int SolvedWords { get; private set; }
        public int CurrentWordIndex { get; private set; }
        public string CurrentWord { get; private set; }
        public string CurrentClue { get; private set; }

        private List<WordEntry> _words;

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

        public bool PrepareLevel(int level)
        {
            _words = new List<WordEntry>();
            TotalWords = 0;
            SolvedWords = 0;
            CurrentWordIndex = 0;
            CurrentWord = string.Empty;
            CurrentClue = string.Empty;

            WordDictionaryData dictionary = LocalizationManager.Instance.Active;
            if (dictionary == null || dictionary.levels == null)
            {
                return false;
            }

            foreach (WordLevelData levelData in dictionary.levels)
            {
                if (levelData.level != level || levelData.words == null)
                {
                    continue;
                }

                _words.AddRange(levelData.words);
                break;
            }

            TotalWords = _words.Count;
            return TotalWords > 0;
        }

        public bool HasMoreWords => CurrentWordIndex < _words.Count;

        public string NextWord()
        {
            if (!HasMoreWords)
            {
                return null;
            }
            CurrentWord = _words[CurrentWordIndex].word;
            CurrentClue = _words[CurrentWordIndex].clue;
            CurrentWordIndex++;
            return CurrentWord;
        }

        public void CompleteCurrentWord()
        {
            SolvedWords++;
        }
    }
}