using System.Collections.Generic;
using UnityEngine;

namespace Matatiempo.Core
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        private readonly Dictionary<string, WordDictionaryData> _dictionaries =
            new Dictionary<string, WordDictionaryData>();

        private readonly Dictionary<string, UIStringTable> _uiTables =
            new Dictionary<string, UIStringTable>();

        private WordDictionaryData _active;

        public WordDictionaryData Active => _active;

        public string ActiveLanguage => _active != null ? _active.language : string.Empty;

        public string GetText(string key)
        {
            UIStringTable table = GetUiTable(_active != null ? _active.language : "es");
            if (table != null && table.entries != null)
            {
                for (int i = 0; i < table.entries.Length; i++)
                {
                    if (table.entries[i] != null && table.entries[i].key == key)
                    {
                        return table.entries[i].value;
                    }
                }
            }
            return key;
        }

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

        public bool SetLanguage(string languageCode)
        {
            WordDictionaryData dictionary = GetDictionary(languageCode);
            if (dictionary == null)
            {
                return false;
            }
            _active = dictionary;
            return true;
        }

        public WordDictionaryData GetDictionary(string languageCode)
        {
            if (_dictionaries.TryGetValue(languageCode, out WordDictionaryData cached))
            {
                return cached;
            }

            TextAsset asset = Resources.Load<TextAsset>(JsonPathFor(languageCode));
            if (asset == null)
            {
                return null;
            }

            WordDictionaryData parsed;
            try
            {
                parsed = JsonUtility.FromJson<WordDictionaryData>(asset.text);
            }
            catch
            {
                return null;
            }

            if (parsed == null || parsed.levels == null || parsed.levels.Length == 0)
            {
                return null;
            }

            _dictionaries[languageCode] = parsed;
            return parsed;
        }

        private static string JsonPathFor(string languageCode)
        {
            return $"Localization/JSON/dictionary_{languageCode}";
        }

        private UIStringTable GetUiTable(string languageCode)
        {
            if (_uiTables.TryGetValue(languageCode, out UIStringTable cached))
            {
                return cached;
            }

            UIStringTable table = LoadUiTable(languageCode);
            _uiTables[languageCode] = table;
            return table;
        }

        private static UIStringTable LoadUiTable(string languageCode)
        {
            TextAsset asset = Resources.Load<TextAsset>($"Localization/UI/ui_{languageCode}");
            if (asset == null)
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<UIStringTable>(asset.text);
            }
            catch
            {
                return null;
            }
        }
    }

    [System.Serializable]
    public class UIStringTable
    {
        public string language;
        public UIStringEntry[] entries;
    }

    [System.Serializable]
    public class UIStringEntry
    {
        public string key;
        public string value;
    }
}