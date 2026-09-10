using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>Lista de cenários exibida no menu principal (RF01).</summary>
    [CreateAssetMenu(fileName = "ScenarioCatalog", menuName = "SafeZone VR/Scenario Catalog", order = 1)]
    public class ScenarioCatalogSO : ScriptableObject
    {
        [SerializeField]
        string m_MenuSceneName = "MainMenu";

        [SerializeField]
        List<ScenarioSO> m_Scenarios = new List<ScenarioSO>();

        public string menuSceneName => m_MenuSceneName;
        public IReadOnlyList<ScenarioSO> scenarios => m_Scenarios;

        /// <summary>Catálogo carregado de Resources para que qualquer cena consiga voltar ao menu.</summary>
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
