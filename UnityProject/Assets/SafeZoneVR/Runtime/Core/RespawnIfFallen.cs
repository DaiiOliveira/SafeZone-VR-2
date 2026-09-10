using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>Devolve itens que caíram fora do mundo à posição inicial (não deixa a missão travar).</summary>
    public class RespawnIfFallen : MonoBehaviour
    {
        [SerializeField]
        float m_MinY = -2f;

        Vector3 m_StartPosition;
        Quaternion m_StartRotation;
        Rigidbody m_Rigidbody;
        float m_NextCheck;

        void Start()
        {
            m_StartPosition = transform.position;
            m_StartRotation = transform.rotation;
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (Time.time < m_NextCheck)
                return;
            m_NextCheck = Time.time + 0.5f;

            if (transform.position.y > m_MinY)
                return;

            if (m_Rigidbody != null)
            {
                m_Rigidbody.linearVelocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }
            transform.SetPositionAndRotation(m_StartPosition, m_StartRotation);
        }
    }
}
