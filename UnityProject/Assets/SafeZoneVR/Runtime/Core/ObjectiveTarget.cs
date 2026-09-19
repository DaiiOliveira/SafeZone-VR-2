using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    public class ObjectiveTarget : MonoBehaviour
    {
        [SerializeField]
        MissionStepSO m_Step;

        [SerializeField]
        [Tooltip("Ponto onde o marcador flutua. Se vazio, usa o topo dos renderers + offset.")]
        Transform m_MarkerAnchor;

        [SerializeField]
        float m_MarkerHeightOffset = 0.25f;

        [SerializeField]
        [Tooltip("Objetos ativados somente enquanto este passo for o atual (setas de rota, placas, luzes).")]
        List<GameObject> m_ShowWhileActive = new List<GameObject>();

        public MissionStepSO step
        {
            get => m_Step;
            set => m_Step = value;
        }

        public Transform markerAnchor
        {
            get => m_MarkerAnchor;
            set => m_MarkerAnchor = value;
        }

        public float markerHeightOffset
        {
            get => m_MarkerHeightOffset;
            set => m_MarkerHeightOffset = value;
        }

        public List<GameObject> showWhileActive => m_ShowWhileActive;

        public bool isSatisfied { get; private set; }

        public void SetSatisfied(bool satisfied)
        {
            isSatisfied = satisfied;
        }

        public Vector3 GetMarkerPosition()
        {
            if (m_MarkerAnchor != null)
                return m_MarkerAnchor.position;

            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return transform.position + Vector3.up * m_MarkerHeightOffset;

            var b = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);

            return new Vector3(b.center.x, b.max.y + m_MarkerHeightOffset, b.center.z);
        }

        public void SetActiveVisuals(bool active)
        {
            for (var i = 0; i < m_ShowWhileActive.Count; i++)
            {
                if (m_ShowWhileActive[i] != null && m_ShowWhileActive[i].activeSelf != active)
                    m_ShowWhileActive[i].SetActive(active);
            }
        }
    }
}
