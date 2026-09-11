using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    /// <summary>Opção de decisão diegética (ex.: placa "Aqui?" numa vaga). Errada = erro com explicação; certa = conclui.</summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class DecisionOption : MonoBehaviour
    {
        [SerializeField] bool m_IsCorrect;
        [SerializeField] string m_MistakeId;                 // só para opções erradas
        [SerializeField, TextArea] string m_Feedback;        // erro: mensagem do erro · certa: fala no relógio

        public bool isCorrect { get => m_IsCorrect; set => m_IsCorrect = value; }
        public string mistakeId { get => m_MistakeId; set => m_MistakeId = value; }
        public string feedback { get => m_Feedback; set => m_Feedback = value; }
        public event Action<DecisionOption> chosen;

        void Awake() => GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => Choose());

        public void Choose() => chosen?.Invoke(this);
    }
}
