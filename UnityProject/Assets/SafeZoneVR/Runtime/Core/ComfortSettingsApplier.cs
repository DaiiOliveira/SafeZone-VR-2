using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace SafeZoneVR
{
    public class ComfortSettingsApplier : MonoBehaviour
    {
        [SerializeField]
        XROrigin m_Origin;

        [SerializeField]
        List<ControllerInputActionManager> m_ControllerManagers = new List<ControllerInputActionManager>();

        [SerializeField]
        TunnelingVignetteController m_Vignette;

        [SerializeField]
        [Tooltip("Altura da câmera usada no modo sentado (Device tracking).")]
        float m_SeatedCameraHeight = 1.5f;

        float m_DefaultCameraYOffset;
        XROrigin.TrackingOriginMode m_DefaultTrackingMode;

        public XROrigin origin { get => m_Origin; set => m_Origin = value; }
        public List<ControllerInputActionManager> controllerManagers => m_ControllerManagers;
        public TunnelingVignetteController vignette { get => m_Vignette; set => m_Vignette = value; }

        void Awake()
        {
            if (m_Origin == null)
                m_Origin = PlayerLocator.origin;
            if (m_ControllerManagers.Count == 0 && m_Origin != null)
                m_ControllerManagers.AddRange(m_Origin.GetComponentsInChildren<ControllerInputActionManager>(true));
            if (m_Vignette == null && m_Origin != null)
                m_Vignette = m_Origin.GetComponentInChildren<TunnelingVignetteController>(true);

            if (m_Origin != null)
            {
                m_DefaultCameraYOffset = m_Origin.CameraYOffset;
                m_DefaultTrackingMode = m_Origin.RequestedTrackingOriginMode;
            }
        }

        void OnEnable()
        {
            ComfortSettings.changed += Apply;
        }

        void Start()
        {
            Apply();
        }

        void OnDisable()
        {
            ComfortSettings.changed -= Apply;
        }

        public void Apply()
        {
            var continuous = ComfortSettings.locomotion == LocomotionMode.Continuous;
            var smoothTurn = ComfortSettings.turn == TurnMode.Smooth;

            var moveHandName = ComfortSettings.leftHanded ? "Right" : "Left";
            for (var i = 0; i < m_ControllerManagers.Count; i++)
            {
                var m = m_ControllerManagers[i];
                if (m == null) continue;
                var isMoveHand = m.name.StartsWith(moveHandName);
                m.smoothMotionEnabled = continuous && isMoveHand;
                m.smoothTurnEnabled = smoothTurn;
            }

            if (m_Vignette != null)
            {
                var providers = m_Vignette.locomotionVignetteProviders;
                for (var i = 0; i < providers.Count; i++)
                    providers[i].enabled = ComfortSettings.vignetteEnabled;
                m_Vignette.gameObject.SetActive(ComfortSettings.vignetteEnabled);
            }

            if (m_Origin != null)
            {
                if (ComfortSettings.seatedMode)
                {
                    m_Origin.CameraYOffset = m_SeatedCameraHeight;
                    m_Origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
                }
                else
                {
                    m_Origin.CameraYOffset = m_DefaultCameraYOffset;
                    m_Origin.RequestedTrackingOriginMode = m_DefaultTrackingMode;
                }
            }
        }

        public static void Recenter()
        {
            var subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            for (var i = 0; i < subsystems.Count; i++)
                subsystems[i].TryRecenter();
        }
    }
}
