using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Liga ou desliga objetos quando um passo é concluído. Não pode estar dentro de um objeto que ele mesmo desliga:
    /// coloque todos sob SafeZone_Gameplay/Phases.
    /// </summary>
    public class ActivateOnStepCompleted : MonoBehaviour
    {
        [SerializeField] MissionStepSO m_Step;
        [SerializeField] List<GameObject> m_Activate = new List<GameObject>();
        [SerializeField] List<GameObject> m_Deactivate = new List<GameObject>();
        [SerializeField] bool m_ApplyInitialState = true;   // começa com "Activate" desligado e "Deactivate" ligado

        public MissionStepSO step { get => m_Step; set => m_Step = value; }
        public List<GameObject> activate => m_Activate;
        public List<GameObject> deactivate => m_Deactivate;
        public bool applyInitialState { get => m_ApplyInitialState; set => m_ApplyInitialState = value; }

        void Start()
        {
            if (m_ApplyInitialState) Apply(false);
            var m = ScenarioManager.instance;
            if (m != null) m.onStepCompleted.AddListener(OnStepCompleted);
        }

        void OnDestroy() { var m = ScenarioManager.instance; if (m != null) m.onStepCompleted.RemoveListener(OnStepCompleted); }

        void OnStepCompleted(MissionStepSO s) { if (s == m_Step) Apply(true); }

        void Apply(bool completed)
        {
            foreach (var go in m_Activate) if (go != null) go.SetActive(completed);
            foreach (var go in m_Deactivate) if (go != null) go.SetActive(!completed);
        }
    }
}
