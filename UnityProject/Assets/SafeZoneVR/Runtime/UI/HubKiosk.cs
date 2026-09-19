using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class HubKiosk : MonoBehaviour
    {
        [SerializeField]
        ScenarioSO m_Scenario;

        [SerializeField]
        Button m_PlayButton;

        [SerializeField]
        TextMeshProUGUI m_PlayLabel;

        [SerializeField]
        TextMeshProUGUI m_TitleLabel;

        [SerializeField]
        TextMeshProUGUI m_DescriptionLabel;

        [SerializeField]
        TextMeshProUGUI m_StatusLabel;

        [SerializeField]
        [Tooltip("Placa 3D opcional, usada quando o texto gravado no modelo não corresponde ao cenário.")]
        TextMeshPro m_BoardLabel;

        [SerializeField]
        [Tooltip("Título da placa 3D. Vazio: usa o nome do cenário em maiúsculas.")]
        string m_BoardTitle;

        [SerializeField]
        [Tooltip("Categoria mostrada na placa 3D, abaixo do título.")]
        string m_BoardSubtitle;

        public ScenarioSO scenario { get => m_Scenario; set => m_Scenario = value; }
        public Button playButton { get => m_PlayButton; set => m_PlayButton = value; }
        public TextMeshProUGUI playLabel { get => m_PlayLabel; set => m_PlayLabel = value; }
        public TextMeshProUGUI titleLabel { get => m_TitleLabel; set => m_TitleLabel = value; }
        public TextMeshProUGUI descriptionLabel { get => m_DescriptionLabel; set => m_DescriptionLabel = value; }
        public TextMeshProUGUI statusLabel { get => m_StatusLabel; set => m_StatusLabel = value; }
        public TextMeshPro boardLabel { get => m_BoardLabel; set => m_BoardLabel = value; }
        public string boardTitle { get => m_BoardTitle; set => m_BoardTitle = value; }
        public string boardSubtitle { get => m_BoardSubtitle; set => m_BoardSubtitle = value; }

        void Awake()
        {
            if (m_PlayButton != null)
                m_PlayButton.onClick.AddListener(Play);
        }

        void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            var available = m_Scenario != null && m_Scenario.isAvailable;

            if (m_TitleLabel != null)
                m_TitleLabel.text = m_Scenario != null ? m_Scenario.displayName : "Em breve";

            if (m_DescriptionLabel != null)
                m_DescriptionLabel.text = m_Scenario != null ? m_Scenario.description : string.Empty;

            if (m_StatusLabel != null)
                m_StatusLabel.text = Status();

            if (m_PlayLabel != null)
                m_PlayLabel.text = available ? "Jogar" : "Em breve";

            if (m_PlayButton != null)
                m_PlayButton.interactable = available && SafetyNoticePanel.hasAccepted;

            if (m_BoardLabel != null)
            {
                var title = m_BoardTitle;
                if (string.IsNullOrEmpty(title))
                    title = m_Scenario != null ? m_Scenario.displayName.ToUpperInvariant() : "EM BREVE";
                m_BoardLabel.text = string.IsNullOrEmpty(m_BoardSubtitle)
                    ? title
                    : $"{title}\n<size=45%>{m_BoardSubtitle}</size>";
            }
        }

        string Status()
        {
            if (m_Scenario == null)
                return string.Empty;
            if (!m_Scenario.isAvailable)
                return "Cenário ainda não disponível";

            var steps = m_Scenario.steps != null ? m_Scenario.steps.Count : 0;
            var best = PerformanceEvaluator.GetBestScore(m_Scenario);
            var score = best >= 0 ? $"Melhor: {best} pontos ({PerformanceEvaluator.Classify(best)})" : "Ainda não jogado";
            return steps > 0 ? $"{score}  ·  {steps} passos" : score;
        }

        public void Play()
        {
            SceneFlow.LoadScenario(m_Scenario);
        }
    }
}
