using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    [CreateAssetMenu(fileName = "ScenarioCatalog", menuName = "SafeZone VR/Scenario Catalog", order = 1)]
    public class ScenarioCatalogSO : ScriptableObject
    {
        [SerializeField]
        string m_MenuSceneName = "MainMenu";

        [SerializeField]
        List<ScenarioSO> m_Scenarios = new List<ScenarioSO>();

        public string menuSceneName => m_MenuSceneName;
        public IReadOnlyList<ScenarioSO> scenarios => m_Scenarios;

        public static ScenarioCatalogSO Load()
        {
            return Resources.Load<ScenarioCatalogSO>("ScenarioCatalog");
        }

#if UNITY_EDITOR
        public void EditorInitialize(string menuSceneName, List<ScenarioSO> scenarios)
        {
            m_MenuSceneName = menuSceneName;
            m_Scenarios = scenarios;
        }
#endif
    }
}
