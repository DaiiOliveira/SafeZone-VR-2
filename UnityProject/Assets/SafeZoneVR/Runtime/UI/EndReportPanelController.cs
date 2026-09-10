using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SafeZoneVR
{
    /// <summary>
    /// Relatório de fim de fase (RF03): estrelas, tempo, acertos, erros, melhor resultado e cards educativos
    /// com as orientações da Defesa Civil. Aparece em frente ao jogador ao concluir o cenário.
    /// </summary>
    public class EndReportPanelController : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        GameObject m_Root;

        [SerializeField]
        TextMeshProUGUI m_TitleText;

        [SerializeField]
        List<Image> m_StarImages = new List<Image>();

        [SerializeField]
        Color m_StarOnColor = new Color(1f, 0.82f, 0.2f);

        [SerializeField]
        Color m_StarOffColor = new Color(0.35f, 0.35f, 0.4f);

        [SerializeField]
        TextMeshProUGUI m_SummaryText;

        [SerializeField]
        TextMeshProUGUI m_DetailsText;

        [SerializeField]
        TextMeshProUGUI m_CardTitleText;

        [SerializeField]
        TextMeshProUGUI m_CardBodyText;

        [SerializeField]
        TextMeshProUGUI m_CardCounterText;

        [SerializeField]
        Button m_PrevCardButton;

        [SerializeField]
        Button m_NextCardButton;

        [SerializeField]
        Button m_ReplayButton;

        [SerializeField]
        Button m_MenuButton;

        [SerializeField]
        float m_ShowDelaySeconds = 1.2f;

        int m_CardIndex;
        IReadOnlyList<EducationCardSO> m_Cards;

        public ScenarioManager scenarioManager { get => m_ScenarioManager; set => m_ScenarioManager = value; }
        public GameObject root { get => m_Root; set => m_Root = value; }
        public TextMeshProUGUI titleText { get => m_TitleText; set => m_TitleText = value; }
        public List<Image> starImages => m_StarImages;
        public TextMeshProUGUI summaryText { get => m_SummaryText; set => m_SummaryText = value; }
        public TextMeshProUGUI detailsText { get => m_DetailsText; set => m_DetailsText = value; }
        public TextMeshProUGUI cardTitleText { get => m_CardTitleText; set => m_CardTitleText = value; }
        public TextMeshProUGUI cardBodyText { get => m_CardBodyText; set => m_CardBodyText = value; }
        public TextMeshProUGUI cardCounterText { get => m_CardCounterText; set => m_CardCounterText = value; }
        public Button prevCardButton { get => m_PrevCardButton; set => m_PrevCardButton = value; }
        public Button nextCardButton { get => m_NextCardButton; set => m_NextCardButton = value; }
        public Button replayButton { get => m_ReplayButton; set => m_ReplayButton = value; }
        public Button menuButton { get => m_MenuButton; set => m_MenuButton = value; }

        void Awake()
        {
            if (m_Root == null)
                m_Root = gameObject;

            if (m_PrevCardButton != null) m_PrevCardButton.onClick.AddListener(() => ShowCard(m_CardIndex - 1));
            if (m_NextCardButton != null) m_NextCardButton.onClick.AddListener(() => ShowCard(m_CardIndex + 1));
            if (m_ReplayButton != null) m_ReplayButton.onClick.AddListener(SceneFlow.ReloadCurrent);
            if (m_MenuButton != null) m_MenuButton.onClick.AddListener(SceneFlow.LoadMenu);
        }

        void Start()
        {
            if (m_ScenarioManager == null)
                m_ScenarioManager = ScenarioManager.instance;

            if (m_ScenarioManager != null)
                m_ScenarioManager.onScenarioComplete.AddListener(OnScenarioComplete);

            if (m_Root != null)
                m_Root.SetActive(false);
        }

        void OnDestroy()
        {
            if (m_ScenarioManager != null)
                m_ScenarioManager.onScenarioComplete.RemoveListener(OnScenarioComplete);
        }

        void OnScenarioComplete()
        {
            Invoke(nameof(Show), m_ShowDelaySeconds);
        }

        public void Show()
        {
            if (m_ScenarioManager == null)
                return;

            var result = m_ScenarioManager.result ?? m_ScenarioManager.BuildResult();
            var scenario = m_ScenarioManager.scenario;

            if (m_TitleText != null)
                m_TitleText.text = result.stars >= 3 ? "Excelente! Fase concluída" : "Fase concluída!";

            for (var i = 0; i < m_StarImages.Count; i++)
            {
                if (m_StarImages[i] != null)
                    m_StarImages[i].color = i < result.stars ? m_StarOnColor : m_StarOffColor;
            }

            if (m_SummaryText != null)
            {
                var sb = new StringBuilder();
                sb.Append("Tempo de reação: ").Append(ScenarioResult.FormatTime(result.elapsedSeconds));
                sb.Append("   |   Passos: ").Append(result.completedCount).Append('/').Append(result.totalSteps);
                sb.Append("   |   Pontos: ").Append(result.score);
                if (result.bestTimeSeconds >= 0f && !result.isNewBest)
                    sb.Append("\nMelhor tempo: ").Append(ScenarioResult.FormatTime(result.bestTimeSeconds));
                else if (result.isNewBest)
                    sb.Append("\nNovo recorde pessoal!");
                m_SummaryText.text = sb.ToString();
            }

            if (m_DetailsText != null)
            {
                var sb = new StringBuilder();
                sb.Append("Decisões corretas: ").Append(result.completedCount).Append('\n');
                sb.Append("Fora da ordem sugerida: ").Append(result.outOfOrderCount).Append('\n');
                sb.Append("Decisões inseguras: ").Append(result.mistakeCount);
                for (var i = 0; i < result.mistakes.Count; i++)
                    sb.Append("\n  • ").Append(result.mistakes[i]);
                for (var i = 0; i < result.notes.Count; i++)
                    sb.Append('\n').Append(result.notes[i]);
                m_DetailsText.text = sb.ToString();
            }

            m_Cards = scenario != null ? scenario.recapCards : null;
            ShowCard(0);

            if (m_Root != null)
                m_Root.SetActive(true);
        }

        void ShowCard(int index)
        {
            var count = m_Cards != null ? m_Cards.Count : 0;
            if (count == 0)
            {
                if (m_CardTitleText != null) m_CardTitleText.text = "";
                if (m_CardBodyText != null) m_CardBodyText.text = "";
                if (m_CardCounterText != null) m_CardCounterText.text = "";
                if (m_PrevCardButton != null) m_PrevCardButton.interactable = false;
                if (m_NextCardButton != null) m_NextCardButton.interactable = false;
                return;
            }

            m_CardIndex = ((index % count) + count) % count;
            var card = m_Cards[m_CardIndex];
            if (m_CardTitleText != null) m_CardTitleText.text = card.title;
            if (m_CardBodyText != null) m_CardBodyText.text = card.tipText + (string.IsNullOrEmpty(card.source) ? "" : $"\n<size=70%><i>Fonte: {card.source}</i></size>");
            if (m_CardCounterText != null) m_CardCounterText.text = $"{m_CardIndex + 1} / {count}";
            if (m_PrevCardButton != null) m_PrevCardButton.interactable = count > 1;
            if (m_NextCardButton != null) m_NextCardButton.interactable = count > 1;
        }
    }
}
