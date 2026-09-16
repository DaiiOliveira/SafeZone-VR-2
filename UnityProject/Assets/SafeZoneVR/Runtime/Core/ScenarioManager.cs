using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SafeZoneVR
{
    [Serializable]
    public class MissionStepEvent : UnityEvent<MissionStepSO> { }

    [Serializable]
    public class MissionStepProgressEvent : UnityEvent<MissionStepSO, int, int> { }

    [Serializable]
    public class StringEvent : UnityEvent<string> { }

    /// <summary>
    /// Estado da fase (RF02): mantém o passo atual, registra conclusões, erros e tempo, e dispara
    /// eventos consumidos pela UI de pulso, pelos guias visuais e pelo relatório final.
    /// Passos fora de ordem são aceitos, mas marcados como tal.
    /// </summary>
    public class ScenarioManager : MonoBehaviour
    {
        [SerializeField]
        ScenarioSO m_Scenario;

        [SerializeField]
        [Tooltip("Se verdadeiro, o cronômetro começa ao carregar a cena. Se falso, aguarda StartScenario() (ex.: painel de briefing).")]
        bool m_AutoStart = true;

        [Header("Eventos")]
        [SerializeField]
        UnityEvent m_OnScenarioStarted = new UnityEvent();

        [SerializeField]
        MissionStepEvent m_OnStepChanged = new MissionStepEvent();

        [SerializeField]
        MissionStepEvent m_OnStepCompleted = new MissionStepEvent();

        [SerializeField]
        MissionStepProgressEvent m_OnStepProgress = new MissionStepProgressEvent();

        [SerializeField]
        StringEvent m_OnMistake = new StringEvent();

        [SerializeField]
        StringEvent m_OnInfo = new StringEvent();

        [SerializeField]
        UnityEvent m_OnScenarioComplete = new UnityEvent();

        readonly HashSet<string> m_CompletedStepIds = new HashSet<string>();
        readonly List<MissionStepSO> m_CompletionOrder = new List<MissionStepSO>();
        readonly List<string> m_Mistakes = new List<string>();
        readonly List<string> m_Notes = new List<string>();
        readonly Dictionary<string, float> m_LastMistakeTime = new Dictionary<string, float>();

        float m_StartTime;
        float m_EndTime;
        ScenarioResult m_Result;

        static ScenarioManager s_Instance;

        /// <summary>Instância da cena atual (busca uma vez se ainda não registrada, ex.: após recarga de domínio).</summary>
        public static ScenarioManager instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = FindAnyObjectByType<ScenarioManager>();
                return s_Instance;
            }
            private set => s_Instance = value;
        }

        public ScenarioSO scenario
        {
            get => m_Scenario;
            set => m_Scenario = value;
        }

        public bool autoStart
        {
            get => m_AutoStart;
            set => m_AutoStart = value;
        }

        public UnityEvent onScenarioStarted => m_OnScenarioStarted;
        public MissionStepEvent onStepChanged => m_OnStepChanged;
        public MissionStepEvent onStepCompleted => m_OnStepCompleted;
        public MissionStepProgressEvent onStepProgress => m_OnStepProgress;
        public StringEvent onMistake => m_OnMistake;
        public StringEvent onInfo => m_OnInfo;
        public UnityEvent onScenarioComplete => m_OnScenarioComplete;

        public MissionStepSO currentStep { get; private set; }
        public bool hasStarted { get; private set; }
        public bool isComplete { get; private set; }
        public int completedCount => m_CompletedStepIds.Count;
        public int totalSteps => m_Scenario != null ? m_Scenario.steps.Count : 0;
        public int outOfOrderCount { get; private set; }
        public int repeatCount { get; private set; }
        public int mistakeCount => m_Mistakes.Count;
        public IReadOnlyList<MissionStepSO> completionOrder => m_CompletionOrder;
        public IReadOnlyList<string> mistakes => m_Mistakes;
        public ScenarioResult result => m_Result;

        public float elapsedSeconds
        {
            get
            {
                if (!hasStarted) return 0f;
                return (isComplete ? m_EndTime : Time.time) - m_StartTime;
            }
        }

        public int currentStepIndex
        {
            get
            {
                if (currentStep == null || m_Scenario == null) return -1;
                return IndexOfStep(m_Scenario.steps, currentStep);
            }
        }

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        void Start()
        {
            if (m_Scenario == null || m_Scenario.steps.Count == 0)
            {
                Debug.LogError("ScenarioManager: nenhum cenário (ou cenário sem passos) atribuído.", this);
                return;
            }

            // O passo atual já é conhecido antes do início, para a UI mostrar o que vem pela frente.
            currentStep = m_Scenario.steps[0];
            m_OnStepChanged.Invoke(currentStep);

            if (m_AutoStart)
                StartScenario();
        }

        /// <summary>Inicia o cronômetro. Chamado pelo painel de briefing ou automaticamente.</summary>
        public void StartScenario()
        {
            if (hasStarted || m_Scenario == null)
                return;

            hasStarted = true;
            m_StartTime = Time.time;
            m_OnScenarioStarted.Invoke();
        }

        public bool IsStepCompleted(MissionStepSO step)
        {
            return step != null && m_CompletedStepIds.Contains(step.stepId);
        }

        public bool IsCurrentStep(MissionStepSO step)
        {
            return step != null && currentStep == step;
        }

        /// <summary>Progresso parcial de um passo (ex.: 2 de 3 itens do kit) para feedback imediato.</summary>
        public void ReportStepProgress(MissionStepSO step, int done, int total)
        {
            if (step == null || isComplete)
                return;
            m_OnStepProgress.Invoke(step, done, total);
        }

        public void CompleteStep(MissionStepSO step)
        {
            if (step == null || m_Scenario == null || isComplete)
                return;

            if (!hasStarted)
                StartScenario();

            if (m_CompletedStepIds.Contains(step.stepId))
            {
                repeatCount++;
                return;
            }

            var steps = m_Scenario.steps;
            var actualIndex = IndexOfStep(steps, step);
            if (actualIndex < 0)
            {
                Debug.LogWarning($"ScenarioManager: passo '{step.name}' não pertence ao cenário '{m_Scenario.name}'.", this);
                return;
            }

            if (!IsInOrder(steps, actualIndex))
                outOfOrderCount++;

            m_CompletedStepIds.Add(step.stepId);
            m_CompletionOrder.Add(step);
            m_OnStepCompleted.Invoke(step);

            var nextStep = FindNextIncompleteStep(steps);
            if (nextStep != null)
            {
                currentStep = nextStep;
                m_OnStepChanged.Invoke(currentStep);
            }
            else
            {
                FinishScenario();
            }
        }

        /// <summary>Em ordem = todos os passos anteriores já feitos, exceto os do mesmo grupo de ordem (≠ 0).</summary>
        bool IsInOrder(IReadOnlyList<MissionStepSO> steps, int index)
        {
            var group = steps[index].orderGroup;
            for (var i = 0; i < index; i++)
            {
                var s = steps[i];
                if (m_CompletedStepIds.Contains(s.stepId)) continue;
                if (group != 0 && s.orderGroup == group) continue;
                return false;
            }
            return true;
        }

        /// <summary>Mensagem informativa (alerta oficial, fala de NPC). Aparece no relógio e não conta como erro.</summary>
        public void ShowInfo(string message)
        {
            if (!string.IsNullOrEmpty(message))
                m_OnInfo.Invoke(message);
        }

        /// <summary>
        /// Registra uma decisão insegura (ex.: tentar sair de carro). Não encerra a fase.
        /// O mesmo id só conta uma vez a cada <paramref name="cooldownSeconds"/>.
        /// </summary>
        public void RegisterMistake(string mistakeId, string message, float cooldownSeconds = 10f)
        {
            if (isComplete)
                return;

            if (!string.IsNullOrEmpty(mistakeId))
            {
                if (m_LastMistakeTime.TryGetValue(mistakeId, out var last) && Time.time - last < cooldownSeconds)
                    return;
                m_LastMistakeTime[mistakeId] = Time.time;
            }

            if (!hasStarted)
                StartScenario();

            m_Mistakes.Add(message);
            m_OnMistake.Invoke(message);
        }

        /// <summary>Observação livre incluída no relatório (ex.: nível da água ao concluir).</summary>
        public void AddResultNote(string note)
        {
            if (string.IsNullOrEmpty(note))
                return;
            m_Notes.Add(note);
            // Ouvintes de onScenarioComplete podem adicionar notas depois que o resultado foi montado.
            if (m_Result != null)
                m_Result.notes.Add(note);
        }

        public ScenarioResult BuildResult()
        {
            var r = new ScenarioResult
            {
                elapsedSeconds = elapsedSeconds,
                completedCount = completedCount,
                totalSteps = totalSteps,
                outOfOrderCount = outOfOrderCount,
                repeatCount = repeatCount,
                mistakeCount = mistakeCount
            };
            r.mistakes.AddRange(m_Mistakes);
            r.notes.AddRange(m_Notes);
            return r;
        }

        void FinishScenario()
        {
            currentStep = null;
            isComplete = true;
            m_EndTime = Time.time;

            m_Result = BuildResult();
            PerformanceEvaluator.Evaluate(m_Scenario, m_Result);

            m_OnScenarioComplete.Invoke();
        }

        static int IndexOfStep(IReadOnlyList<MissionStepSO> steps, MissionStepSO step)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i] == step)
                    return i;
            }
            return -1;
        }

        MissionStepSO FindNextIncompleteStep(IReadOnlyList<MissionStepSO> steps)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                if (!m_CompletedStepIds.Contains(steps[i].stepId))
                    return steps[i];
            }
            return null;
        }
    }
}
