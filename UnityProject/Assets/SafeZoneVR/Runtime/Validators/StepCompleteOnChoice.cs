using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SafeZoneVR
{
    public class StepCompleteOnChoice : StepValidatorBase
    {
        [SerializeField] List<DecisionOption> m_Options = new List<DecisionOption>();
        [SerializeField] UnityEvent m_OnCorrect = new UnityEvent();

        public List<DecisionOption> options => m_Options;
        public UnityEvent onCorrect => m_OnCorrect;

        void OnEnable()  { foreach (var o in m_Options) if (o != null) o.chosen += OnChosen; }
        void OnDisable() { foreach (var o in m_Options) if (o != null) o.chosen -= OnChosen; }

        void OnChosen(DecisionOption option)
        {
            if (m_Completed) return;
            var m = manager;
            if (m == null) return;
            if (!option.isCorrect)
            {
                m.RegisterMistake(option.mistakeId, option.feedback, 10f, ActionKind.ErroLeve, false, option.advice);
                return;
            }
            if (!string.IsNullOrEmpty(option.feedback)) m.ShowInfo(option.feedback);
            foreach (var o in m_Options)
                if (o != null && o.TryGetComponent<ObjectiveTarget>(out var t)) t.SetSatisfied(true);
            m_OnCorrect.Invoke();
            Complete();
        }
    }
}
