using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    public class StepCompleteOnOpenings : StepValidatorBase
    {
        [SerializeField] List<ToggleOpening> m_Openings = new List<ToggleOpening>();
        [SerializeField] bool m_TargetOpen = true;
        [Tooltip("Opcional: só conclui com a cabeça do jogador dentro deste collider (ex.: quintal da frente).")]
        [SerializeField] Collider m_RequirePlayerInZone;

        public List<ToggleOpening> openings => m_Openings;
        public bool targetOpen { get => m_TargetOpen; set => m_TargetOpen = value; }
        public Collider requirePlayerInZone { get => m_RequirePlayerInZone; set => m_RequirePlayerInZone = value; }

        void OnEnable()  { foreach (var o in m_Openings) if (o != null) o.onStateChanged.AddListener(OnChanged); }
        void OnDisable() { foreach (var o in m_Openings) if (o != null) o.onStateChanged.RemoveListener(OnChanged); }

        void OnChanged(bool _)
        {
            if (m_Completed) return;
            if (m_RequirePlayerInZone != null &&
                !(PlayerLocator.TryGetHeadPosition(out var head) && m_RequirePlayerInZone.bounds.Contains(head)))
                return;
            var ok = 0;
            foreach (var o in m_Openings) if (o != null && o.isOpen == m_TargetOpen) ok++;
            if (m_Openings.Count > 1) ReportProgress(ok, m_Openings.Count);
            if (ok >= m_Openings.Count) Complete();
        }
    }
}
