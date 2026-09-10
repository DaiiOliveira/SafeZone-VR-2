using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>Define a câmera de evento de um Canvas world-space como a câmera do jogador.</summary>
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
