using UnityEngine;

namespace SafeZoneVR
{
    public class StepCompleteOnButtonPress : StepValidatorBase
    {
        [SerializeField]
        bool m_CompleteOnTriggerEnter = false;

        void OnTriggerEnter(Collider other)
        {
            if (m_CompleteOnTriggerEnter)
                OnButtonPressed();
        }

        public void OnButtonPressed()
        {
            Complete();
        }
    }
}
