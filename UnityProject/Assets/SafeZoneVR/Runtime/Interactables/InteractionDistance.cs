using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    public static class InteractionDistance
    {
        public static bool IsNear(XRBaseInteractable interactable, IXRInteractor interactor, float maxDistance)
        {
            if (interactable == null || interactor == null || interactor.transform == null)
                return false;
            if (interactor is XRSocketInteractor)
                return false;
            if (interactor is NearFarInteractor nearFar && nearFar.hasSelection && nearFar.selectionRegion.Value == NearFarInteractor.Region.Far)
                return false;

            var hand = interactor.transform.position;
            var best = float.MaxValue;
            var colliders = interactable.colliders;
            for (var i = 0; i < colliders.Count; i++)
            {
                var c = colliders[i];
                if (c == null || !c.enabled) continue;
                var closest = c is MeshCollider mc && !mc.convex ? c.bounds.ClosestPoint(hand) : c.ClosestPoint(hand);
                best = Mathf.Min(best, (closest - hand).sqrMagnitude);
            }
            if (best == float.MaxValue)
                best = (interactable.transform.position - hand).sqrMagnitude;
            return best <= maxDistance * maxDistance;
        }
    }
}
