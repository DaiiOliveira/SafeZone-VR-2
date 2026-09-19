using System.Collections;
using UnityEngine;

namespace SafeZoneVR
{
    public class SimpleMover : MonoBehaviour
    {
        [SerializeField] Transform m_Target;
        [SerializeField] float m_Seconds = 6f;
        [SerializeField] bool m_MoveOnEnable;

        public Transform target { get => m_Target; set => m_Target = value; }
        public float seconds { get => m_Seconds; set => m_Seconds = value; }
        public bool moveOnEnable { get => m_MoveOnEnable; set => m_MoveOnEnable = value; }

        void OnEnable()
        {
            if (m_MoveOnEnable) MoveToTarget();
        }

        public void MoveToTarget() => StartCoroutine(Move());

        IEnumerator Move()
        {
            if (m_Target == null) yield break;
            Vector3 fromPos = transform.position; Quaternion fromRot = transform.rotation;
            for (var t = 0f; t < 1f; t += Time.deltaTime / m_Seconds)
            {
                var k = Mathf.SmoothStep(0f, 1f, t);
                transform.SetPositionAndRotation(Vector3.Lerp(fromPos, m_Target.position, k), Quaternion.Slerp(fromRot, m_Target.rotation, k));
                yield return null;
            }
            transform.SetPositionAndRotation(m_Target.position, m_Target.rotation);
        }
    }
}
