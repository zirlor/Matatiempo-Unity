using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Matatiempo.Core;
using Matatiempo.Gameplay;
using Matatiempo.Monetization;
using Matatiempo.UI;

namespace Matatiempo.EditorTools
{
    public static class SceneSetupEditor
    {
        [MenuItem("Tools/Matatiempo/Setup Game Scene")]
        public static void SetupGameScene()
        {
            EnsureBootstrap();
            EnsureUI();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[Matatiempo] Escena configurada: Bootstrap + UI creados y guardados.");
        }

        private static void EnsureBootstrap()
        {
            GameObject bootstrap = GameObject.Find("Bootstrap");
            if (bootstrap == null)
            {
                bootstrap = new GameObject("Bootstrap");
            }
            AddOrUpdate<GameManager>(bootstrap);
            AddOrUpdate<LevelManager>(bootstrap);
            AddOrUpdate<LocalizationManager>(bootstrap);
            AddOrUpdate<WordManager>(bootstrap);
            AddOrUpdate<HangmanGameManager>(bootstrap);
            AddOrUpdate<TimerManager>(bootstrap);
            AddOrUpdate<AudioManager>(bootstrap);
            AddOrUpdate<MonetizationManager>(bootstrap);
            AddOrUpdate<AdsManager>(bootstrap);
        }

        private static void EnsureUI()
        {
            GameObject ui = GameObject.Find("UI");
            if (ui == null)
            {
                ui = new GameObject("UI");
            }
            AddOrUpdate<UIManager>(ui);
        }

        private static T AddOrUpdate<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                component = target.AddComponent<T>();
            }
            return component;
        }
    }
}