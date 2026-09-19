using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class WrongActionInteractable : MonoBehaviour
    {
        [SerializeField]
        string m_MistakeId = "wrong_action";

        [SerializeField]
        [TextArea(2, 4)]
        string m_Message = "Essa não é uma ação segura.";

        [SerializeField]
        float m_CooldownSeconds = 10f;

        [SerializeField]
        ActionKind m_Kind = ActionKind.ErroLeve;

        [SerializeField]
        bool m_Fatal;

        [SerializeField]
        [TextArea(2, 4)]
        string m_Advice = "";

        XRSimpleInteractable m_Interactable;

        public string mistakeId { get => m_MistakeId; set => m_MistakeId = value; }
        public string message { get => m_Message; set => m_Message = value; }
        public ActionKind kind { get => m_Kind; set => m_Kind = value; }
        public bool fatal { get => m_Fatal; set => m_Fatal = value; }
        public string advice { get => m_Advice; set => m_Advice = value; }

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
            var m = ScenarioManager.instance;
            if (m != null)
                m.RegisterMistake(m_MistakeId, m_Message, m_CooldownSeconds, m_Kind, m_Fatal, m_Advice);
        }
    }
}
