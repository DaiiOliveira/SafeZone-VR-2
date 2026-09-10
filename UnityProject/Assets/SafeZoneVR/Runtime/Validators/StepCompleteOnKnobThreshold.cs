using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Conclui o passo quando um Transform gira mais que um ângulo em relação à rotação inicial.
    /// Útil para objetos girados por física (HingeJoint) ou animação. Para XRKnob, prefira <see cref="StepCompleteOnKnob"/>.
    /// </summary>
    public class StepCompleteOnKnobThreshold : StepValidatorBase
    {
        [SerializeField]
        Transform m_TargetTransform;

        [SerializeField]
        float m_ThresholdAngle = 90f;

        [SerializeField]
        [Tooltip("Intervalo (s) entre verificações; evita custo por frame.")]
        float m_CheckInterval = 0.1f;

        Quaternion m_StartRotation;
        float m_NextCheck;

        public Transform targetTransform
        {
            get => m_TargetTransform;
            set => m_TargetTransform = value;
        }

        public float thresholdAngle
        {
            get => m_ThresholdAngle;
            set => m_ThresholdAngle = value;
        }

        void Start()
        {
            if (m_TargetTransform == null)
            {
                Debug.LogWarning($"StepCompleteOnKnobThreshold em '{name}' sem Target Transform; validador desativado.", this);
                enabled = false;
                return;
            }

            m_StartRotation = m_TargetTransform.localRotation;
        }

        void Update()
        {
            if (m_Completed || Time.time < m_NextCheck)
                return;
            m_NextCheck = Time.time + m_CheckInterval;

            var angle = Quaternion.Angle(m_StartRotation, m_TargetTransform.localRotation);
            if (angle >= m_ThresholdAngle)
                Complete();
        }
    }
}
