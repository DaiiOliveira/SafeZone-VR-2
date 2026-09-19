using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        ScenarioCatalogSO m_Catalog;

        [SerializeField]
        List<HubKiosk> m_Kiosks = new List<HubKiosk>();

        [SerializeField]
        Button m_OptionsButton;

        [SerializeField]
        OptionsPanelController m_OptionsPanel;

        [SerializeField]
        Button m_QuitButton;

        public ScenarioCatalogSO catalog { get => m_Catalog; set => m_Catalog = value; }
        public List<HubKiosk> kiosks { get => m_Kiosks; set => m_Kiosks = value; }
        public Button optionsButton { get => m_OptionsButton; set => m_OptionsButton = value; }
        public OptionsPanelController optionsPanel { get => m_OptionsPanel; set => m_OptionsPanel = value; }
        public Button quitButton { get => m_QuitButton; set => m_QuitButton = value; }

        void Start()
        {
            if (m_Catalog == null)
                m_Catalog = ScenarioCatalogSO.Load();

            BindKiosks();

            if (m_OptionsButton != null && m_OptionsPanel != null)
                m_OptionsButton.onClick.AddListener(m_OptionsPanel.Toggle);

            if (m_QuitButton != null)
                m_QuitButton.onClick.AddListener(Quit);
        }

        public void RefreshKiosks()
        {
            for (var i = 0; i < m_Kiosks.Count; i++)
                if (m_Kiosks[i] != null)
                    m_Kiosks[i].Refresh();
        }

        void BindKiosks()
        {
            var scenarios = m_Catalog != null ? m_Catalog.scenarios : null;
            for (var i = 0; i < m_Kiosks.Count; i++)
            {
                var kiosk = m_Kiosks[i];
                if (kiosk == null)
                    continue;

                if (kiosk.scenario == null && scenarios != null && i < scenarios.Count)
                    kiosk.scenario = scenarios[i];

                kiosk.Refresh();
            }
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
