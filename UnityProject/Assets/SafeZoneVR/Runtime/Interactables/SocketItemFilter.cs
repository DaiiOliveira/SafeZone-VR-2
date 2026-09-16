using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class SocketItemFilter : MonoBehaviour, IXRSelectFilter, IXRHoverFilter
    {
        [SerializeField]
        List<string> m_AllowedItemIds = new List<string>();

        public List<string> allowedItemIds => m_AllowedItemIds;

        public bool canProcess => isActiveAndEnabled;

        void Awake()
        {
            var socket = GetComponent<XRSocketInteractor>();
            socket.selectFilters.Add(this);
            socket.hoverFilters.Add(this);
        }

        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            return IsAllowed(interactable.transform);
        }

        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            return IsAllowed(interactable.transform);
        }

        bool IsAllowed(Transform t)
        {
            if (m_AllowedItemIds.Count == 0 || t == null)
                return true;
            var id = t.GetComponent<ObjectiveItemId>();
            return id != null && m_AllowedItemIds.Contains(id.itemId);
        }
    }
}
