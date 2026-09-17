using UnityEngine;
using UnityEngine.SceneManagement;

namespace SafeZoneVR
{
    public static class SceneFlow
    {
        public const string k_DefaultMenuScene = "MainMenu";

        static bool s_Loading;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            s_Loading = false;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            s_Loading = false;
        }

        public static void LoadScene(string sceneName)
        {
            LoadScene(sceneName, null);
        }

        public static void LoadScene(string sceneName, string label)
        {
            if (string.IsNullOrEmpty(sceneName) || s_Loading)
                return;

            s_Loading = true;
            PlayerLocator.Invalidate();

            var loading = LoadingScreen.instance;
            if (loading != null)
            {
                loading.Load(sceneName, label);
                return;
            }

            var fader = ScreenFader.instance;
            if (fader != null)
                fader.FadeOutAndLoad(sceneName);
            else
                SceneManager.LoadScene(sceneName);
        }

        public static void LoadScenario(ScenarioSO scenario)
        {
            if (scenario == null || !scenario.isAvailable)
                return;
            LoadScene(scenario.sceneName, scenario.displayName);
        }

        public static void ReloadCurrent()
        {
            var scene = SceneManager.GetActiveScene();
            var manager = ScenarioManager.instance;
            var label = manager != null && manager.scenario != null ? manager.scenario.displayName : null;
            LoadScene(scene.name, label);
        }

        public static void LoadMenu()
        {
            var catalog = ScenarioCatalogSO.Load();
            LoadScene(catalog != null && !string.IsNullOrEmpty(catalog.menuSceneName) ? catalog.menuSceneName : k_DefaultMenuScene, "menu");
        }
    }
}
