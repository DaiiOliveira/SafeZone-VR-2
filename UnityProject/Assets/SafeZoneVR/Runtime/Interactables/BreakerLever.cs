using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    [Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [RequireComponent(typeof(XRSimpleInteractable))]
    public class BreakerLever : MonoBehaviour
    {
        [SerializeField]
        Transform m_Pivot;

        [SerializeField]
        Vector3 m_Axis = Vector3.right;

        [SerializeField]
        float m_OnAngle = 40f;

        [SerializeField]
        float m_OffAngle = -40f;

        [SerializeField]
        float m_DegreesPerSecond = 300f;

        [SerializeField]
        bool m_StartsOn = true;

        [SerializeField]
        [Tooltip("Renderer opcional com luz indicadora (verde = ligado, cinza = desligado).")]
        Renderer m_IndicatorRenderer;

        [SerializeField]
        BoolEvent m_OnStateChanged = new BoolEvent();

        XRSimpleInteractable m_Interactable;
        Quaternion m_InitialLocalRotation;
        float m_TargetAngle;
        bool m_IsOn;
        MaterialPropertyBlock m_Block;
        static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");

        public bool isOn => m_IsOn;
        public BoolEvent onStateChanged => m_OnStateChanged;

        public Transform pivot { get => m_Pivot; set => m_Pivot = value; }
        public Vector3 axis { get => m_Axis; set => m_Axis = value; }
        public float onAngle { get => m_OnAngle; set => m_OnAngle = value; }
        public float offAngle { get => m_OffAngle; set => m_OffAngle = value; }
        public bool startsOn { get => m_StartsOn; set => m_StartsOn = value; }
        public Renderer indicatorRenderer { get => m_IndicatorRenderer; set => m_IndicatorRenderer = value; }

        void Awake()
        {
            if (m_Pivot == null)
                m_Pivot = transform;

            m_InitialLocalRotation = m_Pivot.localRotation;
            m_IsOn = m_StartsOn;
            m_TargetAngle = m_IsOn ? m_OnAngle : m_OffAngle;
            m_Pivot.localRotation = m_InitialLocalRotation * Quaternion.AngleAxis(m_TargetAngle, m_Axis);

            m_Interactable = GetComponent<XRSimpleInteractable>();
            m_Interactable.selectEntered.AddListener(OnSelectEntered);
            m_Block = new MaterialPropertyBlock();
            UpdateIndicator();
        }

        void OnDestroy()
        {
            if (m_Interactable != null)
                m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        void Update()
        {
            var target = m_InitialLocalRotation * Quaternion.AngleAxis(m_TargetAngle, m_Axis);
            if (m_Pivot.localRotation == target)
                return;
            m_Pivot.localRotation = Quaternion.RotateTowards(m_Pivot.localRotation, target, m_DegreesPerSecond * Time.deltaTime);
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            Toggle();
        }

        public void Toggle()
        {
            SetState(!m_IsOn);
        }

        public void SetState(bool on)
        {
            if (m_IsOn == on)
                return;
            m_IsOn = on;
            m_TargetAngle = m_IsOn ? m_OnAngle : m_OffAngle;
            UpdateIndicator();
            m_OnStateChanged.Invoke(m_IsOn);
        }

        void UpdateIndicator()
        {
            if (m_IndicatorRenderer == null)
                return;
            m_IndicatorRenderer.GetPropertyBlock(m_Block);
            m_Block.SetColor(k_BaseColor, m_IsOn ? new Color(0.2f, 0.9f, 0.3f) : new Color(0.35f, 0.35f, 0.35f));
            m_IndicatorRenderer.SetPropertyBlock(m_Block);
        }
    }
}
