using Unity.XR.CoreUtils;
using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Acesso barato à posição do jogador (câmera do XR Origin) sem Find() em Update.
    /// </summary>
    public static class PlayerLocator
    {
        static XROrigin s_Origin;
        static Camera s_Camera;

        public static XROrigin origin
        {
            get
            {
                if (s_Origin == null)
                    s_Origin = Object.FindAnyObjectByType<XROrigin>();
                return s_Origin;
            }
        }

        public static Camera playerCamera
        {
            get
            {
                if (s_Camera == null)
                {
                    var o = origin;
                    s_Camera = o != null && o.Camera != null ? o.Camera : Camera.main;
                }
                return s_Camera;
            }
        }

        public static bool TryGetHeadPosition(out Vector3 position)
        {
            var cam = playerCamera;
            if (cam == null)
            {
                position = Vector3.zero;
                return false;
            }
            position = cam.transform.position;
            return true;
        }

        /// <summary>Posição dos pés: XZ da cabeça, Y do XR Origin.</summary>
        public static bool TryGetFeetPosition(out Vector3 position)
        {
            if (!TryGetHeadPosition(out position))
                return false;
            var o = origin;
            if (o != null)
                position.y = o.transform.position.y;
            return true;
        }

        public static bool IsPlayerCollider(Collider other)
        {
            if (other == null)
                return false;
            if (other.CompareTag("Player"))
                return true;
            if (other.GetComponent<CharacterController>() != null)
                return true;
            return other.GetComponentInParent<XROrigin>() != null;
        }

        public static void Invalidate()
        {
            s_Origin = null;
            s_Camera = null;
        }
    }
}
