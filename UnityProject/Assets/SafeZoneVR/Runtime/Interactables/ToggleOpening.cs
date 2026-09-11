using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    /// <summary>
    /// Janela ou porta que abre/fecha com um toque (select), com animação curta. O collider do XRSimpleInteractable
    /// fica na moldura/maçaneta; o collider sólido da folha fica na própria folha.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class ToggleOpening : MonoBehaviour
    {
        [SerializeField] Transform m_Hinge;                 // pivô da folha
        [SerializeField] Vector3 m_Axis = Vector3.up;
        [SerializeField] float m_OpenAngle = 80f;
        [SerializeField] float m_DegreesPerSecond = 240f;
        [SerializeField] bool m_StartsOpen;
        [SerializeField] BoolEvent m_OnStateChanged = new BoolEvent();   // true = aberta
        Quaternion m_Closed; float m_Target; bool m_IsOpen;

        public bool isOpen => m_IsOpen;
        public BoolEvent onStateChanged => m_OnStateChanged;
        public Transform hinge { get => m_Hinge; set => m_Hinge = value; }
        public Vector3 axis { get => m_Axis; set => m_Axis = value; }
        public bool startsOpen { get => m_StartsOpen; set => m_StartsOpen = value; }
        public float openAngle { get => m_OpenAngle; set => m_OpenAngle = value; }

        void Awake()
        {
            if (m_Hinge == null) m_Hinge = transform;
            m_Closed = m_Hinge.localRotation;
            m_IsOpen = m_StartsOpen;
            m_Target = m_IsOpen ? m_OpenAngle : 0f;
            m_Hinge.localRotation = m_Closed * Quaternion.AngleAxis(m_Target, m_Axis);
            GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => SetOpen(!m_IsOpen));
        }

        void Update()
        {
            var target = m_Closed * Quaternion.AngleAxis(m_Target, m_Axis);
            if (m_Hinge.localRotation != target)
                m_Hinge.localRotation = Quaternion.RotateTowards(m_Hinge.localRotation, target, m_DegreesPerSecond * Time.deltaTime);
        }

        public void Toggle() => SetOpen(!m_IsOpen);

        public void SetOpen(bool open)
        {
            if (m_IsOpen == open) return;
            m_IsOpen = open;
            m_Target = open ? m_OpenAngle : 0f;
            m_OnStateChanged.Invoke(open);
        }
    }
}
