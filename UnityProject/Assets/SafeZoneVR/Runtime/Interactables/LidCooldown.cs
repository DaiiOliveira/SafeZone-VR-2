using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    public class LidCooldown : MonoBehaviour
    {
        [SerializeField] XRSocketInteractor m_LidSocket;
        [SerializeField] FireController m_PanFire;
        [SerializeField] float m_CoolSeconds = 45f;
        [SerializeField] string m_MistakeId = "tampa_cedo";
        [SerializeField, TextArea] string m_Message = "Não levante a tampa logo: espere a panela esfriar, senão o fogo pode voltar.";
        [SerializeField, TextArea] string m_Advice = "Deixe a panela tampada até esfriar; só então destampe, com o gás já fechado.";
        [SerializeField] UnityEvent m_OnCovered = new UnityEvent();
        float m_CoveredAt = -1f;

        public UnityEvent onCovered => m_OnCovered;
        public XRSocketInteractor lidSocket { get => m_LidSocket; set => m_LidSocket = value; }
        public FireController panFire { get => m_PanFire; set => m_PanFire = value; }
        public float coolSeconds { get => m_CoolSeconds; set => m_CoolSeconds = value; }

        void Awake()
        {
            m_LidSocket.selectEntered.AddListener(OnLidPlaced);
            m_LidSocket.selectExited.AddListener(OnLidRemoved);
        }

        void OnLidPlaced(SelectEnterEventArgs _)
        {
            m_CoveredAt = Time.time;
            if (m_PanFire != null) m_PanFire.Extinguish();
            m_OnCovered.Invoke();
        }

        void OnLidRemoved(SelectExitEventArgs args)
        {
            if (args.isCanceled || m_CoveredAt < 0f) return;
            if (Time.time - m_CoveredAt < m_CoolSeconds)
            {
                if (m_PanFire != null) m_PanFire.Ignite(FireController.Size.Small);
                var m = ScenarioManager.instance;
                if (m != null) m.RegisterMistake(m_MistakeId, m_Message, 10f, ActionKind.ErroLeve, false, m_Advice);
            }
            m_CoveredAt = -1f;
        }
    }
}
