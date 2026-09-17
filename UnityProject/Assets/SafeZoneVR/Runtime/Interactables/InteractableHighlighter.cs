using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace SafeZoneVR
{
    public class InteractableHighlighter : MonoBehaviour
    {
        void Start()
        {
            foreach (var interactable in FindObjectsByType<XRBaseInteractable>(FindObjectsInactive.Include))
            {
                if (interactable is BaseTeleportationInteractable) continue;
                if (interactable.GetComponent<InteractableHighlight>() != null) continue;
                interactable.gameObject.AddComponent<InteractableHighlight>();
            }
        }
    }
}
