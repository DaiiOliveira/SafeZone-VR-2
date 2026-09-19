using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace SafeZoneVR
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Distância abaixo do ponto (m) que conta como \"caiu para fora do mundo\".")]
        float m_FallLimit = 3f;

        [SerializeField]
        float m_CheckInterval = 0.5f;

        XROrigin m_Origin;
        float m_NextCheck;

        public float fallLimit { get => m_FallLimit; set => m_FallLimit = value; }

        IEnumerator Start()
        {
            m_Origin = PlayerLocator.origin;
            if (m_Origin == null)
                yield break;

            yield return null;
            yield return null;
            Respawn();

            var cam = m_Origin.Camera.transform;
            var waited = 0f;
            while (waited < 1f && cam.localPosition.sqrMagnitude <= 0.0001f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            if (waited > 0f && cam.localPosition.sqrMagnitude > 0.0001f)
                Respawn();
        }

        void Update()
        {
            if (m_Origin == null || Time.time < m_NextCheck)
                return;
            m_NextCheck = Time.time + m_CheckInterval;

            if (m_Origin.transform.position.y < transform.position.y - m_FallLimit)
                Respawn();
        }

        public void Respawn()
        {
            if (m_Origin == null)
                m_Origin = PlayerLocator.origin;
            if (m_Origin == null)
                return;

            var originTransform = m_Origin.transform;
            var cam = m_Origin.Camera.transform;

            var camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            var targetForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (camForward.sqrMagnitude > 0.0001f && targetForward.sqrMagnitude > 0.0001f)
                m_Origin.RotateAroundCameraUsingOriginUp(Vector3.SignedAngle(camForward, targetForward, Vector3.up));

            var camPos = cam.position;
            var target = transform.position;
            originTransform.position += new Vector3(target.x - camPos.x, target.y - originTransform.position.y, target.z - camPos.z);

            Physics.SyncTransforms();
        }
    }
}
