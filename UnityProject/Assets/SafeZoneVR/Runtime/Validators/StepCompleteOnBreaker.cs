using UnityEngine;

namespace SafeZoneVR
{
    public class StepCompleteOnBreaker : StepValidatorBase
    {
        [SerializeField]
        BreakerLever m_Lever;

        [SerializeField]
        [Tooltip("Estado que conclui o passo (falso = desligado).")]
        bool m_CompleteWhenOn = false;

        public BreakerLever lever
        {
            get => m_Lever;
            set => m_Lever = value;
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_Lever == null)
                m_Lever = GetComponent<BreakerLever>();
        }

        void OnEnable()
        {
            if (m_Lever != null)
                m_Lever.onStateChanged.AddListener(OnStateChanged);
        }

        void OnDisable()
        {
            if (m_Lever != null)
                m_Lever.onStateChanged.RemoveListener(OnStateChanged);
        }

        void OnStateChanged(bool isOn)
        {
            if (m_Completed)
                return;
            if (isOn == m_CompleteWhenOn)
                Complete();
        }
    }
}
