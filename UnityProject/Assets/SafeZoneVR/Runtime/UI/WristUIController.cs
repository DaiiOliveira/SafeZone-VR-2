using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class WristUIController : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        FloodWaterController m_Water;

        [SerializeField]
        OptionsPanelController m_OptionsPanel;

        [Header("Textos")]
        [SerializeField]
        TextMeshProUGUI m_ProgressText;

        [SerializeField]
        TextMeshProUGUI m_TitleText;

        [SerializeField]
        TextMeshProUGUI m_InstructionText;

        [SerializeField]
        TextMeshProUGUI m_LocationText;

        [SerializeField]
        TextMeshProUGUI m_TimerText;

        [SerializeField]
        TextMeshProUGUI m_WaterText;

        [SerializeField]
        TextMeshProUGUI m_IntegrityText;

        [SerializeField]
        TextMeshProUGUI m_WarningText;

        [SerializeField]
        TextMeshProUGUI m_SubProgressText;

        [Header("Outros")]
        [SerializeField]
        Transform m_CompassArrow;

        [SerializeField]
        Button m_OptionsButton;

        [SerializeField]
        [Tooltip("Atalho para abrir/fechar as opções: botão Menu do controle (tecla M no XR Interaction Simulator).")]
        InputActionProperty m_ToggleOptionsInput = new InputActionProperty(
            new InputAction("Toggle Options", InputActionType.Button, "<XRController>/{MenuButton}"));

        [SerializeField]
        [Tooltip("Parte visual do relógio (Canvas). Some quando a fase termina para não cobrir o relatório final.")]
        GameObject m_PanelRoot;

        [SerializeField]
        float m_WarningSeconds = 6f;

        float m_WarningUntil;
        float m_NextTimerUpdate;
        int m_LastWaterCm = int.MinValue;

        public ScenarioManager scenarioManager { get => m_ScenarioManager; set => m_ScenarioManager = value; }
        public FloodWaterController water { get => m_Water; set => m_Water = value; }
        public OptionsPanelController optionsPanel { get => m_OptionsPanel; set => m_OptionsPanel = value; }
        public TextMeshProUGUI progressText { get => m_ProgressText; set => m_ProgressText = value; }
        public TextMeshProUGUI titleText { get => m_TitleText; set => m_TitleText = value; }
        public TextMeshProUGUI instructionText { get => m_InstructionText; set => m_InstructionText = value; }
        public TextMeshProUGUI locationText { get => m_LocationText; set => m_LocationText = value; }
        public TextMeshProUGUI timerText { get => m_TimerText; set => m_TimerText = value; }
        public TextMeshProUGUI waterText { get => m_WaterText; set => m_WaterText = value; }
        public TextMeshProUGUI integrityText { get => m_IntegrityText; set => m_IntegrityText = value; }
        public TextMeshProUGUI warningText { get => m_WarningText; set => m_WarningText = value; }
        public TextMeshProUGUI subProgressText { get => m_SubProgressText; set => m_SubProgressText = value; }
        public Transform compassArrow { get => m_CompassArrow; set => m_CompassArrow = value; }
        public Button optionsButton { get => m_OptionsButton; set => m_OptionsButton = value; }
        public GameObject panelRoot { get => m_PanelRoot; set => m_PanelRoot = value; }

        void OnEnable()
        {
            var action = m_ToggleOptionsInput.action;
            if (action == null)
                return;
            ComfortSettings.changed += MountOnNonDominantHand;
            action.performed += OnToggleOptionsInput;
            if (m_ToggleOptionsInput.reference == null)
                action.Enable();
        }

        void OnDisable()
        {
            var action = m_ToggleOptionsInput.action;
            if (action == null)
                return;
            ComfortSettings.changed -= MountOnNonDominantHand;
            action.performed -= OnToggleOptionsInput;
            if (m_ToggleOptionsInput.reference == null)
                action.Disable();
        }

        void OnToggleOptionsInput(InputAction.CallbackContext context)
        {
            ToggleOptions();
        }

        void MountOnNonDominantHand()
        {
            var hand = PlayerLocator.GetNonDominantController();
            if (hand == null || transform.parent == hand)
                return;
            transform.SetParent(hand, false);
        }

        void Start()
        {
            MountOnNonDominantHand();
            if (m_PanelRoot == null)
            {
                var canvas = GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                    m_PanelRoot = canvas.gameObject;
            }

            if (m_ScenarioManager == null)
                m_ScenarioManager = ScenarioManager.instance;
            if (m_Water == null)
                m_Water = FindAnyObjectByType<FloodWaterController>();

            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onScenarioStarted.AddListener(ShowPanel);
                SetPanelVisible(m_ScenarioManager.hasStarted);
                m_ScenarioManager.onStepChanged.AddListener(OnStepChanged);
                m_ScenarioManager.onStepCompleted.AddListener(OnStepCompleted);
                m_ScenarioManager.onStepProgress.AddListener(OnStepProgress);
                m_ScenarioManager.onMistake.AddListener(OnMistake);
                m_ScenarioManager.onInfo.AddListener(OnInfo);
                m_ScenarioManager.onScenarioComplete.AddListener(OnScenarioComplete);
                OnStepChanged(m_ScenarioManager.currentStep);
            }

            if (m_OptionsButton != null)
                m_OptionsButton.onClick.AddListener(ToggleOptions);

            SetText(m_WarningText, "");
            SetText(m_SubProgressText, "");
            SetText(m_WaterText, "");
        }

        void OnDestroy()
        {
            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onScenarioStarted.RemoveListener(ShowPanel);
                m_ScenarioManager.onStepChanged.RemoveListener(OnStepChanged);
                m_ScenarioManager.onStepCompleted.RemoveListener(OnStepCompleted);
                m_ScenarioManager.onStepProgress.RemoveListener(OnStepProgress);
                m_ScenarioManager.onMistake.RemoveListener(OnMistake);
                m_ScenarioManager.onInfo.RemoveListener(OnInfo);
                m_ScenarioManager.onScenarioComplete.RemoveListener(OnScenarioComplete);
            }
        }

        void ShowPanel()
        {
            SetPanelVisible(true);
        }

        void SetPanelVisible(bool visible)
        {
            if (m_PanelRoot != null)
                m_PanelRoot.SetActive(visible);
            if (m_CompassArrow != null)
                m_CompassArrow.gameObject.SetActive(visible);
        }

        void Update()
        {
            if (m_PanelRoot != null && !m_PanelRoot.activeSelf)
                return;

            if (Time.time >= m_NextTimerUpdate)
            {
                m_NextTimerUpdate = Time.time + 0.25f;
                UpdateTimer();
                UpdateWater();
                UpdateIntegrity();
            }

            if (m_WarningText != null && m_WarningUntil > 0f && Time.time > m_WarningUntil)
            {
                m_WarningUntil = 0f;
                m_WarningText.text = "";
            }

            UpdateCompass();
        }

        void UpdateTimer()
        {
            if (m_TimerText == null || m_ScenarioManager == null)
                return;
            m_TimerText.text = m_ScenarioManager.hasStarted
                ? ScenarioResult.FormatTime(m_ScenarioManager.elapsedSeconds)
                : "--:--";
        }

        void UpdateWater()
        {
            if (m_WaterText == null || m_Water == null)
                return;
            var cm = Mathf.RoundToInt(m_Water.currentLevel * 100f);
            if (cm == m_LastWaterCm)
                return;
            m_LastWaterCm = cm;
            m_WaterText.text = cm > 0 ? $"Água: {cm} cm" : "Água: subindo";
        }

        void UpdateIntegrity()
        {
            if (m_IntegrityText == null)
                return;
            var integrity = PlayerIntegrity.instance;
            if (integrity == null || !integrity.isHurt)
            {
                if (m_IntegrityText.text.Length > 0)
                    m_IntegrityText.text = "";
                return;
            }
            var percent = Mathf.CeilToInt(integrity.normalized * 100f);
            m_IntegrityText.color = percent <= 40 ? new Color(1f, 0.45f, 0.4f) : new Color(1f, 0.8f, 0.4f);
            m_IntegrityText.text = $"Você: {percent}%";
        }

        void UpdateCompass()
        {
            if (m_CompassArrow == null)
                return;

            var beacons = ObjectiveBeaconSystem.instance;
            if (beacons == null || !beacons.TryGetNearestActiveTarget(transform.position, out var target))
            {
                if (m_CompassArrow.gameObject.activeSelf)
                    m_CompassArrow.gameObject.SetActive(false);
                return;
            }

            if (!m_CompassArrow.gameObject.activeSelf)
                m_CompassArrow.gameObject.SetActive(true);

            var dir = target - m_CompassArrow.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f)
                return;
            m_CompassArrow.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        void OnStepChanged(MissionStepSO step)
        {
            if (m_ScenarioManager == null)
                return;

            if (step == null)
            {
                SetText(m_TitleText, "Fase concluída!");
                SetText(m_InstructionText, "Veja seu relatório.");
                SetText(m_LocationText, "");
                return;
            }

            var index = m_ScenarioManager.currentStepIndex + 1;
            SetText(m_ProgressText, $"Passo {index} de {m_ScenarioManager.totalSteps}");
            SetText(m_TitleText, step.title);
            SetText(m_InstructionText, step.instructionText);
            SetText(m_LocationText, string.IsNullOrEmpty(step.locationHint) ? "" : $"Onde: {step.locationHint}");
            SetText(m_SubProgressText, "");
        }

        void OnStepCompleted(MissionStepSO step)
        {
            if (step != null && !string.IsNullOrEmpty(step.whyItMatters))
                ShowWarning("Boa! " + step.whyItMatters, new Color(0.55f, 0.95f, 0.6f));
        }

        void OnStepProgress(MissionStepSO step, int done, int total)
        {
            if (m_ScenarioManager != null && step != m_ScenarioManager.currentStep)
                return;
            SetText(m_SubProgressText, total > 1 ? $"{done} de {total}" : "");
        }

        void OnMistake(string message)
        {
            ShowWarning("Atenção: " + message, new Color(1f, 0.8f, 0.4f));
        }

        void OnInfo(string message)
        {
            ShowWarning(message, new Color(0.6f, 0.85f, 1f));
        }

        void OnScenarioComplete()
        {
            SetText(m_ProgressText, "Concluído");
            OnStepChanged(null);

            if (m_OptionsPanel != null)
                m_OptionsPanel.Hide();
            if (m_CompassArrow != null)
                m_CompassArrow.gameObject.SetActive(false);
            if (m_PanelRoot != null)
                m_PanelRoot.SetActive(false);
        }

        void ShowWarning(string message, Color color)
        {
            if (m_WarningText == null)
                return;
            m_WarningText.color = color;
            m_WarningText.text = message;
            m_WarningUntil = Time.time + m_WarningSeconds;
        }

        void ToggleOptions()
        {
            if (m_OptionsPanel != null)
                m_OptionsPanel.Toggle();
        }

        static void SetText(TextMeshProUGUI t, string value)
        {
            if (t != null)
                t.text = value;
        }
    }
}
