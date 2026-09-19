using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class EndReportPanelController : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        GameObject m_Root;

        [SerializeField]
        Image m_HeaderBar;

        [SerializeField]
        TextMeshProUGUI m_TitleText;

        [SerializeField]
        List<Image> m_StarImages = new List<Image>();

        [SerializeField]
        Color m_StarOnColor = new Color(1f, 0.82f, 0.2f);

        [SerializeField]
        Color m_StarOffColor = new Color(0.35f, 0.35f, 0.4f);

        [SerializeField]
        Color m_SuccessColor = new Color(0.18f, 0.72f, 0.42f);

        [SerializeField]
        Color m_FailureColor = new Color(0.72f, 0.33f, 0.18f);

        [SerializeField]
        TextMeshProUGUI m_SummaryText;

        [SerializeField]
        TextMeshProUGUI m_HitsText;

        [SerializeField]
        TextMeshProUGUI m_MistakesText;

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
        public Image headerBar { get => m_HeaderBar; set => m_HeaderBar = value; }
        public TextMeshProUGUI titleText { get => m_TitleText; set => m_TitleText = value; }
        public List<Image> starImages => m_StarImages;
        public TextMeshProUGUI summaryText { get => m_SummaryText; set => m_SummaryText = value; }
        public TextMeshProUGUI hitsText { get => m_HitsText; set => m_HitsText = value; }
        public TextMeshProUGUI mistakesText { get => m_MistakesText; set => m_MistakesText = value; }
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
                m_TitleText.text = result.success
                    ? (result.score >= 90 ? "Excelente! Fase concluída" : "Fase concluída!")
                    : "Fase interrompida";

            if (m_HeaderBar != null)
                m_HeaderBar.color = result.success ? m_SuccessColor : m_FailureColor;

            for (var i = 0; i < m_StarImages.Count; i++)
                if (m_StarImages[i] != null)
                    m_StarImages[i].color = i < result.stars ? m_StarOnColor : m_StarOffColor;

            if (m_SummaryText != null)
            {
                var sb = new StringBuilder();
                sb.Append("<size=130%>").Append(result.score).Append(" / 100</size>   ").Append(result.classification);
                sb.Append('\n');
                sb.Append("Tempo: ").Append(ScenarioResult.FormatTime(result.elapsedSeconds));
                if (result.overTimeLimit && scenario != null)
                    sb.Append(" (acima do limite de ").Append(ScenarioResult.FormatTime(scenario.timeLimitSeconds)).Append(": −10)");
                sb.Append("   |   Passos: ").Append(result.completedCount).Append('/').Append(result.totalSteps);
                if (result.bestScore >= 0 && !result.isNewBest)
                    sb.Append("\nMelhor até agora: ").Append(result.bestScore).Append(" pontos");
                else if (result.isNewBest)
                    sb.Append("\nNovo recorde pessoal!");
                m_SummaryText.text = sb.ToString();
            }

            if (m_HitsText != null)
            {
                var sb = new StringBuilder();
                sb.Append("<b>Acertos (").Append(CountKind(result, ActionKind.Acerto)).Append(")</b>");
                foreach (var a in result.actions)
                {
                    if (a.kind != ActionKind.Acerto) continue;
                    sb.Append("\n• ").Append(a.label);
                }
                foreach (var note in result.notes)
                    sb.Append("\n<size=85%>").Append(note).Append("</size>");
                m_HitsText.text = sb.ToString();
            }

            if (m_MistakesText != null)
            {
                var sb = new StringBuilder();
                sb.Append("<b>Erros: ").Append(result.graveMistakes).Append(" graves, ").Append(result.lightMistakes).Append(" leves</b>");
                if (!result.success && !string.IsNullOrEmpty(result.failureReason))
                    sb.Append("\n<color=#E08A57>A fase parou aqui: ").Append(result.failureReason).Append("</color>");
                var any = false;
                foreach (var a in result.actions)
                {
                    if (!a.isMistake) continue;
                    any = true;
                    sb.Append("\n• ").Append(a.kind == ActionKind.ErroGrave ? "<color=#E08A57><b>Grave</b></color> — " : "<b>Leve</b> — ").Append(a.label);
                    if (!string.IsNullOrEmpty(a.advice))
                        sb.Append("\n  <size=85%>O certo: ").Append(a.advice).Append("</size>");
                }
                if (result.outOfOrderCount > 0)
                    sb.Append("\n• <b>Leve</b> — ").Append(result.outOfOrderCount).Append(" passo(s) fora da ordem sugerida");
                if (!any && result.outOfOrderCount == 0)
                    sb.Append("\nNenhuma decisão insegura registrada. Muito bem!");
                if (!string.IsNullOrEmpty(result.recommendation))
                    sb.Append("\n\n<i>").Append(result.recommendation).Append("</i>");
                m_MistakesText.text = sb.ToString();
            }

            m_Cards = scenario != null ? scenario.recapCards : null;
            ShowCard(0);

            if (m_Root != null)
                m_Root.SetActive(true);
        }

        static int CountKind(ScenarioResult result, ActionKind kind)
        {
            var n = 0;
            for (var i = 0; i < result.actions.Count; i++)
                if (result.actions[i].kind == kind) n++;
            return n;
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
