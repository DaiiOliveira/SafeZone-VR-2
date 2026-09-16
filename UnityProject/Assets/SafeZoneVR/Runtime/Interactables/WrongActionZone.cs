using UnityEngine;

namespace SafeZoneVR
{
    [RequireComponent(typeof(Collider))]
    public class WrongActionZone : MonoBehaviour
    {
        [SerializeField]
        string m_MistakeId = "wrong_zone";

        [SerializeField]
        [TextArea(2, 4)]
        string m_Message = "Evite esta área.";

        [SerializeField]
        float m_CooldownSeconds = 15f;

        [SerializeField]
        float m_CheckInterval = 0.25f;

        Collider m_Zone;
        float m_NextCheck;

        public string mistakeId { get => m_MistakeId; set => m_MistakeId = value; }
        public string message { get => m_Message; set => m_Message = value; }

        void Awake()
        {
            m_Zone = GetComponent<Collider>();
            m_Zone.isTrigger = true;
        }

        void Update()
        {
            if (Time.time < m_NextCheck)
                return;
            m_NextCheck = Time.time + m_CheckInterval;

            if (PlayerLocator.TryGetHeadPosition(out var head) && m_Zone.bounds.Contains(head))
                Register();
        }

        void OnTriggerEnter(Collider other)
        {
            if (PlayerLocator.IsPlayerCollider(other))
                Register();
        }

        void Register()
        {
            var m = ScenarioManager.instance;
            if (m != null)
                m.RegisterMistake(m_MistakeId, m_Message, m_CooldownSeconds);
        }
    }
}
