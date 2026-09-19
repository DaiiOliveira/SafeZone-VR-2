using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    public class IdleHintSystem : MonoBehaviour
    {
        [SerializeField] ScenarioManager m_ScenarioManager;
        [SerializeField] FeedbackPlayer m_Feedback;
        [SerializeField] GameObject m_PanelRoot;
        [SerializeField] TextMeshProUGUI m_TitleText;
        [SerializeField] TextMeshProUGUI m_BodyText;
        [SerializeField] float m_ContextualSeconds = 30f;
        [SerializeField] float m_ExplicitSeconds = 60f;
        [SerializeField] float m_RepeatSeconds = 60f;
        [SerializeField] float m_DisplaySeconds = 9f;
        [SerializeField] float m_CheckInterval = 0.5f;

        readonly List<XRBaseInteractable> m_Watched = new List<XRBaseInteractable>();
        float m_LastActivity;
        float m_NextCheck;
        float m_HideAt;
        int m_Level;
        float m_LastHintAt;

        public ScenarioManager scenarioManager { get => m_ScenarioManager; set => m_ScenarioManager = value; }
        public FeedbackPlayer feedback { get => m_Feedback; set => m_Feedback = value; }
        public GameObject panelRoot { get => m_PanelRoot; set => m_PanelRoot = value; }
        public TextMeshProUGUI titleText { get => m_TitleText; set => m_TitleText = value; }
        public TextMeshProUGUI bodyText { get => m_BodyText; set => m_BodyText = value; }
        public float contextualSeconds { get => m_ContextualSeconds; set => m_ContextualSeconds = value; }
        public float explicitSeconds { get => m_ExplicitSeconds; set => m_ExplicitSeconds = value; }
        public int hintLevel => m_Level;

        void Start()
        {
            if (m_ScenarioManager == null)
                m_ScenarioManager = ScenarioManager.instance;
            if (m_PanelRoot != null)
                m_PanelRoot.SetActive(false);

            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onScenarioStarted.AddListener(MarkActivity);
                m_ScenarioManager.onStepChanged.AddListener(OnStepChanged);
                m_ScenarioManager.onStepProgress.AddListener(OnStepProgress);
                m_ScenarioManager.onMistake.AddListener(OnMessage);
                m_ScenarioManager.onScenarioComplete.AddListener(HidePanel);
            }

            foreach (var interactable in FindObjectsByType<XRBaseInteractable>(FindObjectsInactive.Include))
            {
                if (interactable is UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.BaseTeleportationInteractable) continue;
                interactable.selectEntered.AddListener(OnSelectEntered);
                m_Watched.Add(interactable);
            }
            MarkActivity();
        }

        void OnDestroy()
        {
            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onScenarioStarted.RemoveListener(MarkActivity);
                m_ScenarioManager.onStepChanged.RemoveListener(OnStepChanged);
                m_ScenarioManager.onStepProgress.RemoveListener(OnStepProgress);
                m_ScenarioManager.onMistake.RemoveListener(OnMessage);
                m_ScenarioManager.onScenarioComplete.RemoveListener(HidePanel);
            }
            foreach (var interactable in m_Watched)
                if (interactable != null)
                    interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        void OnStepChanged(MissionStepSO _) { MarkActivity(); HidePanel(); }
        void OnStepProgress(MissionStepSO a, int b, int c) => MarkActivity();
        void OnMessage(string _) => MarkActivity();
        void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor) return;
            MarkActivity();
        }

        public void MarkActivity()
        {
            m_LastActivity = Time.time;
            m_Level = 0;
        }

        void Update()
        {
            if (m_PanelRoot != null && m_PanelRoot.activeSelf && Time.time >= m_HideAt)
                HidePanel();

            if (Time.time < m_NextCheck) return;
            m_NextCheck = Time.time + m_CheckInterval;

            var m = m_ScenarioManager;
            if (m == null || !m.hasStarted || m.isComplete || m.currentStep == null)
            {
                m_LastActivity = Time.time;
                return;
            }

            var idle = Time.time - m_LastActivity;
            if (m_Level == 0 && idle >= m_ContextualSeconds)
            {
                m_Level = 1;
                ShowContextual(m.currentStep);
            }
            else if (m_Level == 1 && idle >= m_ExplicitSeconds)
            {
                m_Level = 2;
                ShowExplicit(m.currentStep);
            }
            else if (m_Level == 2 && Time.time - m_LastHintAt >= m_RepeatSeconds)
            {
                ShowExplicit(m.currentStep);
            }
        }

        void ShowContextual(MissionStepSO step)
        {
            var where = string.IsNullOrEmpty(step.locationHint) ? "" : $"\nProcure em: <b>{step.locationHint}</b>.";
            Show("Precisa de uma dica?", $"Sua missão agora é: <b>{step.title}</b>.{where}");
        }

        void ShowExplicit(MissionStepSO step)
        {
            Show("O que fazer agora", step.instructionText);
        }

        void Show(string title, string body)
        {
            m_LastHintAt = Time.time;
            if (m_TitleText != null) m_TitleText.text = title;
            if (m_BodyText != null) m_BodyText.text = body;
            if (m_PanelRoot != null)
            {
                m_PanelRoot.SetActive(true);
                m_HideAt = Time.time + m_DisplaySeconds;
            }
            else if (m_ScenarioManager != null)
            {
                m_ScenarioManager.ShowInfo(body);
            }
            if (m_Feedback != null)
                m_Feedback.PlayHint();
        }

        void HidePanel()
        {
            if (m_PanelRoot != null && m_PanelRoot.activeSelf)
                m_PanelRoot.SetActive(false);
        }
    }
}
