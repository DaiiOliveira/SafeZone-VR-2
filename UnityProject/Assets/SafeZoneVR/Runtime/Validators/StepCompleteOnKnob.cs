using Unity.VRTemplate;
using UnityEngine;

namespace SafeZoneVR
{
    public class StepCompleteOnKnob : StepValidatorBase
    {
        [SerializeField]
        XRKnob m_Knob;

        [SerializeField]
        [Range(0.05f, 1f)]
        [Tooltip("Quanto o valor normalizado (0..1) precisa mudar em relação ao inicial. 0.25 com faixa de -180..180 = 90 graus.")]
        float m_RequiredDelta = 0.25f;

        float m_StartValue;
        bool m_Initialized;

        public XRKnob knob
        {
            get => m_Knob;
            set => m_Knob = value;
        }

        public float requiredDelta
        {
            get => m_RequiredDelta;
            set => m_RequiredDelta = value;
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_Knob == null)
                m_Knob = GetComponent<XRKnob>();
        }

        void OnEnable()
        {
            if (m_Knob != null)
                m_Knob.onValueChange.AddListener(OnValueChanged);
        }

        void Start()
        {
            if (m_Knob != null)
            {
                m_StartValue = m_Knob.value;
                m_Initialized = true;
            }
        }

        void OnDisable()
        {
            if (m_Knob != null)
                m_Knob.onValueChange.RemoveListener(OnValueChanged);
        }

        void OnValueChanged(float value)
        {
            if (m_Completed || !m_Initialized)
                return;

            if (Mathf.Abs(value - m_StartValue) >= m_RequiredDelta)
                Complete();
        }
    }
}
