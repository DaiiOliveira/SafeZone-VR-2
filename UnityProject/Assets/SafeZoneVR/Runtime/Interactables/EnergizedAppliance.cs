using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class EnergizedAppliance : MonoBehaviour
    {
        [SerializeField] BreakerLever m_Lever;
        [SerializeField] FloodWaterController m_Water;
        [SerializeField] float m_WetLevel = 0.03f;
        [SerializeField] string m_MistakeId = "choque_na_agua";
        [SerializeField, TextArea] string m_WetMessage = "Você tocou um aparelho ligado na tomada com os pés na água: choque elétrico.";
        [SerializeField, TextArea] string m_DryMessage = "Mexer em aparelho ligado com a água subindo é risco de choque.";
        [SerializeField, TextArea] string m_Advice = "Desligue o quadro geral antes de tocar em qualquer aparelho, e nunca mexa em eletricidade com os pés na água.";
        [SerializeField, TextArea] string m_SafeMessage = "Com a energia desligada, dá para mexer no aparelho sem risco de choque.";

        XRSimpleInteractable m_Interactable;

        public BreakerLever lever { get => m_Lever; set => m_Lever = value; }
        public FloodWaterController water { get => m_Water; set => m_Water = value; }
        public bool isEnergized => m_Lever == null || m_Lever.isOn;

        void Awake()
        {
            m_Interactable = GetComponent<XRSimpleInteractable>();
            m_Interactable.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDestroy()
        {
            if (m_Interactable != null)
                m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            var manager = ScenarioManager.instance;
            if (manager == null) return;

            if (!isEnergized)
            {
                manager.ShowInfo(m_SafeMessage);
                return;
            }

            var wet = m_Water != null && m_Water.currentLevel >= m_WetLevel;
            manager.RegisterMistake(m_MistakeId, wet ? m_WetMessage : m_DryMessage, 5f, ActionKind.ErroGrave, wet, m_Advice);
        }
    }
}
