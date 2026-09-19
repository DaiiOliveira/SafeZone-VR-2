using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class ToggleOpening : MonoBehaviour
    {
        [SerializeField] Transform m_Hinge;
        [SerializeField] Vector3 m_Axis = Vector3.up;
        [SerializeField] float m_OpenAngle = 80f;
        [SerializeField] float m_DegreesPerSecond = 240f;
        [SerializeField] bool m_StartsOpen;
        [SerializeField] bool m_DragWhenNear = true;
        [SerializeField] float m_NearDistance = 0.4f;
        [SerializeField] float m_OpenThreshold = 20f;
        [SerializeField] float m_ClosedThreshold = 8f;
        [SerializeField] BoolEvent m_OnStateChanged = new BoolEvent();

        XRSimpleInteractable m_Interactable;
        Quaternion m_Closed;
        float m_Target, m_Angle;
        bool m_IsOpen;
        IXRSelectInteractor m_Dragger;
        Vector3 m_DragStartDir;
        float m_DragStartAngle;

        public bool isOpen => m_IsOpen;
        public bool isDragging => m_Dragger != null;
        public float currentAngle => m_Angle;
        public BoolEvent onStateChanged => m_OnStateChanged;
        public Transform hinge { get => m_Hinge; set => m_Hinge = value; }
        public Vector3 axis { get => m_Axis; set => m_Axis = value; }
        public bool startsOpen { get => m_StartsOpen; set => m_StartsOpen = value; }
        public float openAngle { get => m_OpenAngle; set => m_OpenAngle = value; }
        public bool dragWhenNear { get => m_DragWhenNear; set => m_DragWhenNear = value; }

        void Awake()
        {
            if (m_Hinge == null) m_Hinge = transform;
            m_Closed = m_Hinge.localRotation;
            m_IsOpen = m_StartsOpen;
            m_Target = m_IsOpen ? m_OpenAngle : 0f;
            m_Angle = m_Target;
            ApplyAngle();
            m_Interactable = GetComponent<XRSimpleInteractable>();
            m_Interactable.selectEntered.AddListener(OnSelectEntered);
            m_Interactable.selectExited.AddListener(OnSelectExited);
        }

        void OnDestroy()
        {
            if (m_Interactable == null) return;
            m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
            m_Interactable.selectExited.RemoveListener(OnSelectExited);
        }

        Vector3 WorldAxis()
        {
            var parentRot = m_Hinge.parent != null ? m_Hinge.parent.rotation : Quaternion.identity;
            return (parentRot * m_Closed * m_Axis).normalized;
        }

        Vector3 FlatDirection(Vector3 point)
        {
            return Vector3.ProjectOnPlane(point - m_Hinge.position, WorldAxis());
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            var interactor = args.interactorObject;
            if (m_DragWhenNear && m_Dragger == null && IsNear(interactor))
            {
                var dir = FlatDirection(interactor.transform.position);
                if (dir.sqrMagnitude > 0.0025f)
                {
                    m_Dragger = interactor;
                    m_DragStartDir = dir;
                    m_DragStartAngle = m_Angle;
                    return;
                }
            }
            SetOpen(!m_IsOpen);
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
            if (args.interactorObject != m_Dragger) return;
            m_Dragger = null;
            if (Mathf.Abs(m_Angle) <= m_ClosedThreshold)
                m_Target = 0f;
            else
                m_Target = m_Angle;
            UpdateStateFromAngle();
        }

        bool IsNear(IXRSelectInteractor interactor)
        {
            return InteractionDistance.IsNear(m_Interactable, interactor, m_NearDistance);
        }

        void Update()
        {
            if (m_Dragger != null)
            {
                if (m_Dragger.transform == null) { m_Dragger = null; return; }
                var dir = FlatDirection(m_Dragger.transform.position);
                if (dir.sqrMagnitude > 0.0025f)
                {
                    var delta = Vector3.SignedAngle(m_DragStartDir, dir, WorldAxis());
                    m_Angle = Mathf.Clamp(m_DragStartAngle + delta, Mathf.Min(0f, m_OpenAngle), Mathf.Max(0f, m_OpenAngle));
                    m_Target = m_Angle;
                    ApplyAngle();
                    UpdateStateFromAngle();
                }
                return;
            }

            if (!Mathf.Approximately(m_Angle, m_Target))
            {
                m_Angle = Mathf.MoveTowards(m_Angle, m_Target, m_DegreesPerSecond * Time.deltaTime);
                ApplyAngle();
            }
        }

        void ApplyAngle()
        {
            m_Hinge.localRotation = m_Closed * Quaternion.AngleAxis(m_Angle, m_Axis);
        }

        void UpdateStateFromAngle()
        {
            var abs = Mathf.Abs(m_Angle);
            if (!m_IsOpen && abs >= m_OpenThreshold) SetState(true);
            else if (m_IsOpen && abs <= m_ClosedThreshold) SetState(false);
        }

        void SetState(bool open)
        {
            if (m_IsOpen == open) return;
            m_IsOpen = open;
            m_OnStateChanged.Invoke(open);
        }

        public void Toggle() => SetOpen(!m_IsOpen);

        public void SetOpen(bool open)
        {
            m_Target = open ? m_OpenAngle : 0f;
            SetState(open);
        }
    }
}
