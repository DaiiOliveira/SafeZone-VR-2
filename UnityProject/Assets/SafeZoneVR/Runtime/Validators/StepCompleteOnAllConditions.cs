using System.Collections.Generic;
using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    public class StepCompleteOnAllConditions : StepValidatorBase
    {
        [SerializeField] List<BreakerLever> m_LeversOff = new List<BreakerLever>();
        [SerializeField] List<XRKnob> m_KnobsTurned = new List<XRKnob>();
        [SerializeField, Range(0.05f, 1f)] float m_RequiredDelta = 0.25f;
        [SerializeField] List<XRSocketInteractor> m_SocketsEmpty = new List<XRSocketInteractor>();
        readonly Dictionary<XRKnob, float> m_KnobStart = new Dictionary<XRKnob, float>();
        int m_LastDone = -1;

        public List<BreakerLever> leversOff => m_LeversOff;
        public List<XRKnob> knobsTurned => m_KnobsTurned;
        public List<XRSocketInteractor> socketsEmpty => m_SocketsEmpty;
        public float requiredDelta { get => m_RequiredDelta; set => m_RequiredDelta = value; }

        void Start()
        {
            foreach (var l in m_LeversOff) if (l != null) l.onStateChanged.AddListener(_ => Evaluate());
            foreach (var k in m_KnobsTurned) if (k != null) { m_KnobStart[k] = k.value; k.onValueChange.AddListener(_ => Evaluate()); }
            foreach (var s in m_SocketsEmpty) if (s != null) s.selectExited.AddListener(_ => Evaluate());
        }

        void Evaluate()
        {
            if (m_Completed) return;
            int done = 0, total = m_LeversOff.Count + m_KnobsTurned.Count + m_SocketsEmpty.Count;
            foreach (var l in m_LeversOff) if (l != null && !l.isOn) done++;
            foreach (var k in m_KnobsTurned) if (k != null && Mathf.Abs(k.value - m_KnobStart[k]) >= m_RequiredDelta) done++;
            foreach (var s in m_SocketsEmpty) if (s != null && !s.hasSelection) done++;
            if (done != m_LastDone) { m_LastDone = done; ReportProgress(done, total); }
            if (done >= total) Complete();
        }
    }
}
