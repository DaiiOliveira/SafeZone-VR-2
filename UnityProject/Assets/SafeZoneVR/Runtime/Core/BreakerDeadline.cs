using UnityEngine;

namespace SafeZoneVR
{
    public class BreakerDeadline : MonoBehaviour
    {
        [SerializeField] BreakerLever m_Lever;
        [SerializeField] FloodWaterController m_Water;
        [SerializeField] float m_WarnLevel = 0.05f;
        [SerializeField] float m_OutletLevel = 0.12f;
        [SerializeField] string m_MistakeId = "energia_ligada_na_agua";
        [SerializeField, TextArea] string m_Message = "A água chegou às tomadas com o quadro de energia ligado.";
        [SerializeField, TextArea] string m_Advice = "Desligue o quadro geral assim que a água começar a subir, antes que ela alcance tomadas e aparelhos.";
        [SerializeField, TextArea] string m_Warning = "A água está subindo e o quadro de energia ainda está ligado. Desligue-o antes que a água chegue às tomadas.";
        [SerializeField] float m_CheckInterval = 0.5f;

        float m_NextCheck;
        bool m_Warned;
        bool m_Registered;

        public BreakerLever lever { get => m_Lever; set => m_Lever = value; }
        public FloodWaterController water { get => m_Water; set => m_Water = value; }
        public float outletLevel { get => m_OutletLevel; set => m_OutletLevel = value; }
        public bool isPowerOn => m_Lever == null || m_Lever.isOn;
        public bool waterAtOutlets => m_Water != null && m_Water.currentLevel >= m_OutletLevel;

        void Update()
        {
            if (m_Registered || m_Water == null || Time.time < m_NextCheck) return;
            m_NextCheck = Time.time + m_CheckInterval;

            var manager = ScenarioManager.instance;
            if (manager == null || !manager.hasStarted || manager.isComplete) return;
            if (!isPowerOn)
            {
                m_Registered = true;
                return;
            }

            if (!m_Warned && m_Water.currentLevel >= m_WarnLevel)
            {
                m_Warned = true;
                manager.ShowInfo(m_Warning);
            }

            if (m_Water.currentLevel < m_OutletLevel)
                return;

            m_Registered = true;
            manager.RegisterMistake(m_MistakeId, m_Message, 60f, ActionKind.ErroGrave, false, m_Advice);
        }
    }
}
