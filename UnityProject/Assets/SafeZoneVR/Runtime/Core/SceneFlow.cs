using UnityEngine;
using UnityEngine.SceneManagement;

namespace SafeZoneVR
{
    /// <summary>Navegação entre cenas com fade (conforto em VR).</summary>
    public static class SceneFlow
    {
        public const string k_DefaultMenuScene = "MainMenu";

        // Uma troca de cena por vez: dois cliques seguidos encadeariam dois carregamentos.
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
            if (string.IsNullOrEmpty(sceneName) || s_Loading)
                return;

            s_Loading = true;
            PlayerLocator.Invalidate();

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
            LoadScene(scenario.sceneName);
        }

        public static void ReloadCurrent()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public static void LoadMenu()
        {
            var catalog = ScenarioCatalogSO.Load();
            LoadScene(catalog != null && !string.IsNullOrEmpty(catalog.menuSceneName) ? catalog.menuSceneName : k_DefaultMenuScene);
        }
    }
}
