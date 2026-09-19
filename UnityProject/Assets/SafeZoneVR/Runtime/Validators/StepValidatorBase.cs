using UnityEngine;

namespace SafeZoneVR
{
    public abstract class StepValidatorBase : MonoBehaviour
    {
        [SerializeField]
        protected ScenarioManager m_ScenarioManager;

        [SerializeField]
        protected MissionStepSO m_Step;

        protected bool m_Completed;

        public ScenarioManager scenarioManager
        {
            get => m_ScenarioManager;
            set => m_ScenarioManager = value;
        }

        public MissionStepSO step
        {
            get => m_Step;
            set => m_Step = value;
        }

        public bool isCompleted => m_Completed;

        protected ScenarioManager manager
        {
            get
            {
                if (m_ScenarioManager == null)
                    m_ScenarioManager = ScenarioManager.instance;
                return m_ScenarioManager;
            }
        }

        protected virtual void Awake()
        {
            if (m_Step == null)
                Debug.LogWarning($"{GetType().Name} em '{name}' sem MissionStep atribuído.", this);
        }

        protected void Complete()
        {
            if (m_Completed)
                return;

            var m = manager;
            if (m == null || m_Step == null)
            {
                Debug.LogWarning($"{GetType().Name} em '{name}': sem ScenarioManager ou passo; conclusão ignorada.", this);
                return;
            }

            m_Completed = true;
            m.CompleteStep(m_Step);
        }

        protected void ReportProgress(int done, int total)
        {
            var m = manager;
            if (m != null && m_Step != null)
                m.ReportStepProgress(m_Step, done, total);
        }
    }
}
