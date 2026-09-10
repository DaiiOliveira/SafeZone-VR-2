using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Conclui o passo por chamada externa (UnityEvent de um botão XR, poke, etc.) ou por trigger físico.
    /// </summary>
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
