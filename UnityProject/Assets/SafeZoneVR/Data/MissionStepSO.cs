using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Um passo (missão) dentro de um cenário. Conteúdo puro: o que o jogador deve fazer e por quê,
    /// segundo as orientações da Defesa Civil. A validação fica nos componentes de cena (Validators).
    /// </summary>
    [CreateAssetMenu(fileName = "MissionStep", menuName = "SafeZone VR/Mission Step", order = 10)]
    public class MissionStepSO : ScriptableObject
    {
        [SerializeField]
        string m_StepId = "step_001";

        [SerializeField]
        int m_OrderIndex = 0;

        [SerializeField]
        string m_Title = "Step Title";

        [SerializeField]
        [TextArea(2, 4)]
        string m_InstructionText = "Step instructions";

        [SerializeField]
        [Tooltip("Onde fica o objetivo (ex.: 'Quarto', 'Garagem'). Exibido na UI de pulso.")]
        string m_LocationHint = "";

        [SerializeField]
        [TextArea(2, 4)]
        [Tooltip("Por que este passo importa, segundo a Defesa Civil. Exibido ao concluir o passo e no relatório.")]
        string m_WhyItMatters = "";

        public string stepId => m_StepId;
        public int orderIndex => m_OrderIndex;
        public string title => m_Title;
        public string instructionText => m_InstructionText;
        public string locationHint => m_LocationHint;
        public string whyItMatters => m_WhyItMatters;

#if UNITY_EDITOR
        public void EditorInitialize(string stepId, int orderIndex, string title, string instructionText,
            string locationHint = "", string whyItMatters = "")
        {
            m_StepId = stepId;
            m_OrderIndex = orderIndex;
            m_Title = title;
            m_InstructionText = instructionText;
            m_LocationHint = locationHint;
            m_WhyItMatters = whyItMatters;
        }
#endif
    }
}
