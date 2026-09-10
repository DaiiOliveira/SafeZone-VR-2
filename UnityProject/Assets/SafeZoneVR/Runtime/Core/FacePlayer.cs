using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>Faz o objeto olhar para a câmera do jogador (UI de pulso, etiquetas, marcadores).</summary>
    public class FacePlayer : MonoBehaviour
    {
        [SerializeField]
        bool m_YawOnly = false;

        [SerializeField]
        [Tooltip("Se verdadeiro, a frente (+Z) aponta para longe do jogador, como um Canvas.")]
        bool m_FlipForward = true;

        public bool yawOnly { get => m_YawOnly; set => m_YawOnly = value; }
        public bool flipForward { get => m_FlipForward; set => m_FlipForward = value; }

        void LateUpdate()
        {
            var cam = PlayerLocator.playerCamera;
            if (cam == null)
                return;

            var dir = transform.position - cam.transform.position;
            if (!m_FlipForward)
                dir = -dir;
            if (m_YawOnly)
                dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                return;

            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }
}
