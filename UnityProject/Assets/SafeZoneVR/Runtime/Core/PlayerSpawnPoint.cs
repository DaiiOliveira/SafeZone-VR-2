using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Ponto de início seguro da cena. Ao carregar, posiciona a CABEÇA do jogador (não só o XR Origin) sobre este
    /// ponto, olhando para a frente dele: o deslocamento da cabeça dentro do rig (quarto real no Quest, ou WASD do
    /// XR Interaction Simulator, que sobrevive à troca de cena) não deixa o jogador fora do chão. Se ainda assim ele
    /// cair para fora do mundo, volta para cá.
    /// </summary>
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

            // O rastreamento posiciona a câmera nos primeiros quadros.
            yield return null;
            yield return null;
            Respawn();

            // No headset a pose da cabeça pode chegar um pouco depois: realinha uma vez quando chegar.
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

        /// <summary>Coloca os pés do jogador na altura deste ponto, com a cabeça sobre ele, olhando para a frente do ponto.</summary>
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

            // O CharacterController do rig guarda a própria posição: sincroniza para a física enxergar o novo lugar.
            Physics.SyncTransforms();
        }
    }
}
