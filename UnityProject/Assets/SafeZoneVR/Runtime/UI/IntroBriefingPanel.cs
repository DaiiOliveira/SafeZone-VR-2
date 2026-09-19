using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class IntroBriefingPanel : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        GameObject m_Root;

        [SerializeField]
        TextMeshProUGUI m_TitleText;

        [SerializeField]
        TextMeshProUGUI m_BodyText;

        [SerializeField]
        Button m_StartButton;

        [SerializeField]
        [TextArea(3, 8)]
        string m_IntroText = "";

        public ScenarioManager scenarioManager { get => m_ScenarioManager; set => m_ScenarioManager = value; }
        public GameObject root { get => m_Root; set => m_Root = value; }
        public TextMeshProUGUI titleText { get => m_TitleText; set => m_TitleText = value; }
        public TextMeshProUGUI bodyText { get => m_BodyText; set => m_BodyText = value; }
        public Button startButton { get => m_StartButton; set => m_StartButton = value; }
        public string introText { get => m_IntroText; set => m_IntroText = value; }

        void Awake()
        {
            if (m_Root == null)
                m_Root = gameObject;
            if (m_StartButton != null)
                m_StartButton.onClick.AddListener(Begin);
        }

        void Start()
        {
            if (m_ScenarioManager == null)
                m_ScenarioManager = ScenarioManager.instance;

            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.autoStart = false;
                var scenario = m_ScenarioManager.scenario;
                if (m_TitleText != null && scenario != null)
                    m_TitleText.text = scenario.displayName;

                if (m_BodyText != null)
                {
                    var sb = new System.Text.StringBuilder(m_IntroText);
                    if (scenario != null)
                    {
                        sb.Append("\n\n<b>Suas missões:</b>");
                        for (var i = 0; i < scenario.steps.Count; i++)
                            sb.Append($"\n{i + 1}. {scenario.steps[i].title}");
                    }
                    m_BodyText.text = sb.ToString();
                }

                m_ScenarioManager.onScenarioStarted.AddListener(HideRoot);
            }
        }

        void OnDestroy()
        {
            if (m_ScenarioManager != null)
                m_ScenarioManager.onScenarioStarted.RemoveListener(HideRoot);
        }

        public void Begin()
        {
            if (m_ScenarioManager != null)
                m_ScenarioManager.StartScenario();
            HideRoot();
        }

        void HideRoot()
        {
            if (m_Root != null)
                m_Root.SetActive(false);
        }
    }
}
