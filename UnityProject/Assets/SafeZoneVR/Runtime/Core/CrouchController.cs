using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SafeZoneVR
{
    public class CrouchController : MonoBehaviour
    {
        [SerializeField] XROrigin m_Origin;
        [SerializeField] float m_CrouchHeight = 0.55f;
        [SerializeField] float m_Speed = 2.2f;

        [SerializeField]
        [Tooltip("Segure para agachar: botão B/Y do controle (Ctrl esquerdo no simulador).")]
        InputActionProperty m_CrouchInput = new InputActionProperty(new InputAction("Crouch", InputActionType.Button));

        Transform m_Offset;
        float m_BaseY;
        float m_Current;
        bool m_WasCrouching;

        public float crouchHeight { get => m_CrouchHeight; set => m_CrouchHeight = value; }
        public float currentDrop => m_Current;
        public bool isCrouching => m_Current > 0.02f;
        public XROrigin origin { get => m_Origin; set => m_Origin = value; }

        void Awake()
        {
            if (m_Origin == null)
                m_Origin = PlayerLocator.origin;
            if (m_Origin != null && m_Origin.CameraFloorOffsetObject != null)
                m_Offset = m_Origin.CameraFloorOffsetObject.transform;

            var action = m_CrouchInput.action;
            if (action != null && m_CrouchInput.reference == null && action.bindings.Count == 0)
            {
                action.AddBinding("<XRController>{LeftHand}/secondaryButton");
                action.AddBinding("<XRController>{RightHand}/secondaryButton");
                action.AddBinding("<Keyboard>/leftCtrl");
            }
        }

        void OnEnable()
        {
            if (m_CrouchInput.reference == null)
                m_CrouchInput.action?.Enable();
        }

        void OnDisable()
        {
            if (m_CrouchInput.reference == null)
                m_CrouchInput.action?.Disable();
            Release();
        }

        void Update()
        {
            if (m_Offset == null)
                return;

            var action = m_CrouchInput.action;
            var pressed = action != null && action.IsPressed();

            if (pressed && !m_WasCrouching)
                m_BaseY = m_Offset.localPosition.y + m_Current;
            m_WasCrouching = pressed;

            var target = pressed ? m_CrouchHeight : 0f;
            if (Mathf.Approximately(m_Current, target))
                return;

            m_Current = Mathf.MoveTowards(m_Current, target, m_Speed * Time.deltaTime);
            var p = m_Offset.localPosition;
            p.y = m_BaseY - m_Current;
            m_Offset.localPosition = p;
        }

        void Release()
        {
            if (m_Offset == null || m_Current <= 0f)
                return;
            var p = m_Offset.localPosition;
            p.y = m_BaseY;
            m_Offset.localPosition = p;
            m_Current = 0f;
            m_WasCrouching = false;
        }
    }
}
