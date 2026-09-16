using UnityEngine;

namespace SafeZoneVR
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasCameraBinder : MonoBehaviour
    {
        void Start()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas.worldCamera == null)
                canvas.worldCamera = PlayerLocator.playerCamera;
        }
    }
}
