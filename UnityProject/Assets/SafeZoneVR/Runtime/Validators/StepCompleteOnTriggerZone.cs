using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Conclui o passo quando o jogador entra na zona (ex.: ponto de encontro).
    /// Além do OnTriggerEnter, verifica periodicamente a posição da câmera/pés dentro do collider,
    /// porque o teleporte não dispara eventos de trigger de forma confiável.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class StepCompleteOnTriggerZone : StepValidatorBase
    {
        [SerializeField]
        float m_CheckInterval = 0.2f;

        [SerializeField]
        [Tooltip("Só aceita a entrada depois que a fase começou (evita concluir durante o briefing).")]
        bool m_RequireScenarioStarted = true;

        Collider m_Zone;
        float m_NextCheck;

        protected override void Awake()
        {
            base.Awake();
            m_Zone = GetComponent<Collider>();
            m_Zone.isTrigger = true;
        }

        void Update()
        {
            if (m_Completed || Time.time < m_NextCheck)
                return;
            m_NextCheck = Time.time + m_CheckInterval;

            if (PlayerLocator.TryGetHeadPosition(out var head) && m_Zone.bounds.Contains(head))
            {
                TryComplete();
                return;
            }

            if (PlayerLocator.TryGetFeetPosition(out var feet) && m_Zone.bounds.Contains(feet + Vector3.up * 0.2f))
                TryComplete();
        }

        void OnTriggerEnter(Collider other)
        {
            if (m_Completed)
                return;
            if (PlayerLocator.IsPlayerCollider(other))
                TryComplete();
        }

        void TryComplete()
        {
            if (m_RequireScenarioStarted)
            {
                var m = manager;
                if (m != null && !m.hasStarted)
                    return;
            }
            Complete();
        }
    }
}
