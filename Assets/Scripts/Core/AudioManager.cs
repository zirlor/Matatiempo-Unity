using System.Collections.Generic;
using UnityEngine;

namespace Matatiempo.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _source;
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _source = GetComponent<AudioSource>();
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            GameEvents.LetterGuessed += HandleLetterGuessed;
            GameEvents.WordCompleted += HandleWordCompleted;
            GameEvents.WordFailed += HandleWordFailed;
            GameEvents.TimeUp += HandleTimeUp;
            GameEvents.LevelCompleted += HandleLevelCompleted;
            GameEvents.LevelStarted += HandleLevelStarted;
        }

        private void OnDisable()
        {
            GameEvents.LetterGuessed -= HandleLetterGuessed;
            GameEvents.WordCompleted -= HandleWordCompleted;
            GameEvents.WordFailed -= HandleWordFailed;
            GameEvents.TimeUp -= HandleTimeUp;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
            GameEvents.LevelStarted -= HandleLevelStarted;
        }

        public void PlayClick() => Play("click");
        public void PlayCorrect() => Play("correct");
        public void PlayWrong() => Play("wrong");
        public void PlayWin() => Play("win");
        public void PlayLose() => Play("lose");
        public void PlayLevelUp() => Play("levelup");
        public void PlayTick() => Play("tick");
        public void PlayStart() => Play("start");

        public void Play(string soundName)
        {
            AudioClip clip = LoadClip(soundName);
            if (clip == null || _source == null)
            {
                return;
            }
            _source.PlayOneShot(clip, 1f);
        }

        private AudioClip LoadClip(string soundName)
        {
            if (_clips.TryGetValue(soundName, out AudioClip cached))
            {
                return cached;
            }
            AudioClip clip = Resources.Load<AudioClip>("Audio/SFX/" + soundName);
            if (clip != null)
            {
                _clips.Add(soundName, clip);
            }
            return clip;
        }

        private void HandleLetterGuessed(char letter, bool correct)
        {
            if (correct)
            {
                PlayCorrect();
            }
            else
            {
                PlayWrong();
            }
        }

        private void HandleWordCompleted(string word)
        {
            PlayCorrect();
        }

        private void HandleWordFailed(string word)
        {
            PlayLose();
        }

        private void HandleTimeUp()
        {
            PlayLose();
        }

        private void HandleLevelCompleted(int level)
        {
            PlayWin();
        }

        private void HandleLevelStarted(int level)
        {
            PlayStart();
        }
    }
}