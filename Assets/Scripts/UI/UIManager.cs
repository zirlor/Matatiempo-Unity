using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Matatiempo.Core;

namespace Matatiempo.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private MainMenuView _menuView;
        private GameView _gameView;
        private ResultView _resultView;
        private bool _landscape;
        private CanvasScaler _scaler;
        private float _lastLivesPoll;

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

        private void Start()
        {
            EnsureEventSystem();
            BuildUI();
            _landscape = View.IsLandscape;
            ApplyReferenceResolution();
            GameEvents.StateChanged += HandleStateChanged;
            HandleStateChanged(GameState.MainMenu, GameManager.Instance.State);
        }

        private void Update()
        {
            if (View.IsLandscape != _landscape)
            {
                _landscape = View.IsLandscape;
                ApplyReferenceResolution();
                RebuildAllViews();
            }
            if (Time.unscaledTime - _lastLivesPoll >= 1f)
            {
                _lastLivesPoll = Time.unscaledTime;
                LivesManager.GetLives();
            }
        }

        private void ApplyReferenceResolution()
        {
            _scaler.referenceResolution = _landscape ? new Vector2(1920, 1080) : new Vector2(540, 960);
        }

        public void RebuildAllViews()
        {
            _menuView.Rebuild();
            _gameView.Rebuild();
            _resultView.Rebuild();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                GameEvents.StateChanged -= HandleStateChanged;
            }
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(eventSystemGo);
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            _scaler = scaler;
            canvasGo.AddComponent<GraphicRaycaster>();

            Image background = UIUtils.CreateImage((RectTransform)canvas.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UIUtils.PanelColor);
            UIUtils.ApplySprite(background, "Sprites/UI/background");

            RectTransform safeArea = CreatePanel("SafeArea", canvas.transform);
            safeArea.gameObject.AddComponent<SafeAreaPanel>();

            RectTransform menuRoot = CreatePanel("MainMenu", safeArea);
            _menuView = menuRoot.gameObject.AddComponent<MainMenuView>();
            _menuView.Rebuild();

            RectTransform gameRoot = CreatePanel("GameView", safeArea);
            _gameView = gameRoot.gameObject.AddComponent<GameView>();
            _gameView.Rebuild();

            RectTransform resultRoot = CreatePanel("ResultView", safeArea);
            _resultView = resultRoot.gameObject.AddComponent<ResultView>();
            _resultView.Rebuild();
        }

        private static RectTransform CreatePanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            bool result = current == GameState.LevelCompleted || current == GameState.GameOver;
            _menuView.gameObject.SetActive(current == GameState.MainMenu);
            _gameView.gameObject.SetActive(current == GameState.Playing || current == GameState.Paused);
            _resultView.gameObject.SetActive(result);
        }
    }
}