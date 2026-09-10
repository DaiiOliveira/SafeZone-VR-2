using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SafeZoneVR
{
    /// <summary>
    /// Menu principal (RF01): lista os cenários do catálogo, mostra a melhor pontuação de cada um,
    /// abre o painel de conforto e carrega a fase escolhida.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        ScenarioCatalogSO m_Catalog;

        [SerializeField]
        Transform m_ButtonContainer;

        [SerializeField]
        Button m_ButtonTemplate;

        [SerializeField]
        TextMeshProUGUI m_DescriptionText;

        [SerializeField]
        Button m_OptionsButton;

        [SerializeField]
        OptionsPanelController m_OptionsPanel;

        [SerializeField]
        Button m_QuitButton;

        readonly List<Button> m_Spawned = new List<Button>();

        public ScenarioCatalogSO catalog { get => m_Catalog; set => m_Catalog = value; }
        public Transform buttonContainer { get => m_ButtonContainer; set => m_ButtonContainer = value; }
        public Button buttonTemplate { get => m_ButtonTemplate; set => m_ButtonTemplate = value; }
        public TextMeshProUGUI descriptionText { get => m_DescriptionText; set => m_DescriptionText = value; }
        public Button optionsButton { get => m_OptionsButton; set => m_OptionsButton = value; }
        public OptionsPanelController optionsPanel { get => m_OptionsPanel; set => m_OptionsPanel = value; }
        public Button quitButton { get => m_QuitButton; set => m_QuitButton = value; }

        void Start()
        {
            if (m_Catalog == null)
                m_Catalog = ScenarioCatalogSO.Load();

            if (m_ButtonTemplate != null)
                m_ButtonTemplate.gameObject.SetActive(false);

            BuildList();

            if (m_OptionsButton != null && m_OptionsPanel != null)
                m_OptionsButton.onClick.AddListener(m_OptionsPanel.Toggle);

            if (m_QuitButton != null)
                m_QuitButton.onClick.AddListener(Application.Quit);

            if (m_DescriptionText != null)
                m_DescriptionText.text = "Escolha um cenário. Aponte e aperte o gatilho para selecionar.";
        }

        void BuildList()
        {
            for (var i = 0; i < m_Spawned.Count; i++)
                if (m_Spawned[i] != null) Destroy(m_Spawned[i].gameObject);
            m_Spawned.Clear();

            if (m_Catalog == null || m_ButtonTemplate == null || m_ButtonContainer == null)
                return;

            var scenarios = m_Catalog.scenarios;
            for (var i = 0; i < scenarios.Count; i++)
            {
                var scenario = scenarios[i];
                if (scenario == null) continue;

                var button = Instantiate(m_ButtonTemplate, m_ButtonContainer);
                button.gameObject.SetActive(true);
                button.name = "Button_" + scenario.scenarioId;

                var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    if (scenario.isAvailable)
                    {
                        var best = PerformanceEvaluator.GetBestStars(scenario);
                        label.text = best > 0
                            ? $"{scenario.displayName}\n<size=70%>Melhor: {best} de 3 estrelas</size>"
                            : scenario.displayName;
                    }
                    else
                    {
                        label.text = $"{scenario.displayName}\n<size=70%>Em breve</size>";
                    }
                }

                button.interactable = scenario.isAvailable;
                var captured = scenario;
                button.onClick.AddListener(() => OnScenarioClicked(captured));
                m_Spawned.Add(button);
            }
        }

        void OnScenarioClicked(ScenarioSO scenario)
        {
            if (m_DescriptionText != null)
                m_DescriptionText.text = scenario.description;
            SceneFlow.LoadScenario(scenario);
        }
    }
}
