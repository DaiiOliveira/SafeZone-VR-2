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
        ActionKind m_Kind = ActionKind.ErroLeve;

        [SerializeField]
        bool m_Fatal;

        [SerializeField]
        [TextArea(2, 4)]
        string m_Advice = "";

        [SerializeField]
        float m_CheckInterval = 0.25f;

        Collider m_Zone;
        float m_NextCheck;
        bool m_Armed;

        public string mistakeId { get => m_MistakeId; set => m_MistakeId = value; }
        public string message { get => m_Message; set => m_Message = value; }
        public ActionKind kind { get => m_Kind; set => m_Kind = value; }
        public bool fatal { get => m_Fatal; set => m_Fatal = value; }
        public string advice { get => m_Advice; set => m_Advice = value; }

        void Awake()
        {
            m_Zone = GetComponent<Collider>();
            m_Zone.isTrigger = true;
        }

        void OnEnable()
        {
            m_Armed = false;
            m_NextCheck = 0f;
        }

        void Update()
        {
            if (Time.time < m_NextCheck)
                return;
            m_NextCheck = Time.time + m_CheckInterval;

            if (!PlayerLocator.TryGetHeadPosition(out var head))
                return;

            if (!m_Zone.bounds.Contains(head))
                m_Armed = true;
            else if (m_Armed)
                Register();
        }

        void OnTriggerEnter(Collider other)
        {
            if (m_Armed && PlayerLocator.IsPlayerCollider(other))
                Register();
        }

        void Register()
        {
            var m = ScenarioManager.instance;
            if (m != null)
                m.RegisterMistake(m_MistakeId, m_Message, m_CooldownSeconds, m_Kind, m_Fatal, m_Advice);
        }
    }
}
