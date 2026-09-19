using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SafeZoneVR
{
    public class ScenarioPhaseSwitcher : MonoBehaviour
    {
        [Serializable]
        public class Phase
        {
            public string name;
            public List<MissionStepSO> triggerSteps = new List<MissionStepSO>();
            public float fadeSeconds;
            [TextArea] public string caption;
            [TextArea] public string info;
            public List<GameObject> activate = new List<GameObject>();
            public List<GameObject> deactivate = new List<GameObject>();
            public UnityEvent onEnter = new UnityEvent();
            [NonSerialized] public bool entered;
        }

        [SerializeField] List<Phase> m_Phases = new List<Phase>();
        [SerializeField] TextMeshProUGUI m_Caption;
        ScenarioManager m_Manager;

        public List<Phase> phases => m_Phases;
        public TextMeshProUGUI caption { get => m_Caption; set => m_Caption = value; }

        void Start()
        {
            m_Manager = ScenarioManager.instance;
            if (m_Manager != null) m_Manager.onStepCompleted.AddListener(OnStepCompleted);
            if (m_Caption != null) m_Caption.transform.parent.gameObject.SetActive(false);
        }

        void OnDestroy() { if (m_Manager != null) m_Manager.onStepCompleted.RemoveListener(OnStepCompleted); }

        void OnStepCompleted(MissionStepSO _)
        {
            foreach (var p in m_Phases)
            {
                if (p.entered || p.triggerSteps.Count == 0) continue;
                var all = true;
                foreach (var s in p.triggerSteps) all &= m_Manager.IsStepCompleted(s);
                if (all) StartCoroutine(Enter(p));
            }
        }

        IEnumerator Enter(Phase p)
        {
            p.entered = true;
            var fader = ScreenFader.instance;
            var useFade = p.fadeSeconds > 0f && fader != null;
            if (useFade)
            {
                fader.FadeTo(1f);
                yield return new WaitForSeconds(0.55f);
                if (m_Caption != null && !string.IsNullOrEmpty(p.caption)) { m_Caption.text = p.caption; SetCaptionVisible(true); }
            }
            foreach (var go in p.deactivate) if (go != null) go.SetActive(false);
            foreach (var go in p.activate) if (go != null) go.SetActive(true);
            p.onEnter.Invoke();
            if (!string.IsNullOrEmpty(p.info) && m_Manager != null) m_Manager.ShowInfo(p.info);
            if (useFade)
            {
                yield return new WaitForSeconds(p.fadeSeconds);
                SetCaptionVisible(false);
                fader.FadeTo(0f);
            }
        }

        void SetCaptionVisible(bool visible)
        {
            if (m_Caption == null) return;

            m_Caption.transform.parent.gameObject.SetActive(visible);
        }
    }
}
