using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    // O XR Interaction Simulator (só no PC) sobrevive à troca de cena via DontDestroyOnLoad,
    // mas só procura os controles do rig no OnEnable. Ao sair do HUB ele fica mirando os
    // controles destruídos e o point-and-click do mouse para de funcionar na fase.
    public static class SimulatorSceneRebinder
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var simulator = XRInteractionSimulator.instance;
            if (simulator == null)
                return;

            var camera = Camera.main;
            simulator.cameraTransform = camera != null ? camera.transform : null;

            Transform leftController = null, rightController = null, leftHand = null, rightHand = null;
            var modality = Object.FindAnyObjectByType<XRInputModalityManager>();
            if (modality != null)
            {
                leftController = FindPoseDriver(modality.leftController);
                rightController = FindPoseDriver(modality.rightController);
                leftHand = FindFarCastOrigin(modality.leftHand);
                rightHand = FindFarCastOrigin(modality.rightHand);
            }

            var fallback = simulator.cameraTransform;
            simulator.leftControllerTransform = leftController != null ? leftController : fallback;
            simulator.rightControllerTransform = rightController != null ? rightController : fallback;
            simulator.leftHandAimTransform = leftHand != null ? leftHand : fallback;
            simulator.rightHandAimTransform = rightHand != null ? rightHand : fallback;
        }

        static Transform FindPoseDriver(GameObject root)
        {
            if (root == null)
                return null;
            var driver = root.GetComponentInChildren<TrackedPoseDriver>(true);
            return driver != null ? driver.transform : null;
        }

        static Transform FindFarCastOrigin(GameObject root)
        {
            if (root == null)
                return null;
            foreach (var interactor in root.GetComponentsInChildren<NearFarInteractor>(true))
            {
                var caster = interactor.farInteractionCaster;
                if (caster != null && caster.castOrigin != null)
                    return caster.castOrigin;
            }
            return null;
        }
    }
}
