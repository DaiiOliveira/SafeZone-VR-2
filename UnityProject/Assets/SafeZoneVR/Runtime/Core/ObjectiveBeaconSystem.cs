using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    public class ObjectiveBeaconSystem : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        [Tooltip("Objeto (inativo) usado como modelo do marcador.")]
        GameObject m_MarkerTemplate;

        [SerializeField]
        float m_BobAmplitude = 0.05f;

        [SerializeField]
        float m_BobSpeed = 2.2f;

        [SerializeField]
        float m_SpinSpeed = 60f;

        readonly Dictionary<MissionStepSO, List<ObjectiveTarget>> m_TargetsByStep = new Dictionary<MissionStepSO, List<ObjectiveTarget>>();
        readonly List<ObjectiveTarget> m_AllTargets = new List<ObjectiveTarget>();
        readonly List<Transform> m_MarkerPool = new List<Transform>();
        readonly List<ObjectiveTarget> m_ActiveTargets = new List<ObjectiveTarget>();

        static ObjectiveBeaconSystem s_Instance;

        public static ObjectiveBeaconSystem instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = FindAnyObjectByType<ObjectiveBeaconSystem>();
                return s_Instance;
            }
            private set => s_Instance = value;
        }

        public ScenarioManager scenarioManager { get => m_ScenarioManager; set => m_ScenarioManager = value; }
        public GameObject markerTemplate { get => m_MarkerTemplate; set => m_MarkerTemplate = value; }

        public bool TryGetNearestActiveTarget(Vector3 from, out Vector3 position)
        {
            position = Vector3.zero;
            var best = float.MaxValue;
            var found = false;
            for (var i = 0; i < m_ActiveTargets.Count; i++)
            {
                var t = m_ActiveTargets[i];
                if (t == null || t.isSatisfied || !t.gameObject.activeInHierarchy) continue;
                var p = t.GetMarkerPosition();
                var d = (p - from).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    position = p;
                    found = true;
                }
            }
            return found;
        }

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onStepChanged.RemoveListener(OnStepChanged);
                m_ScenarioManager.onScenarioComplete.RemoveListener(OnScenarioComplete);
            }
        }

        void Start()
        {
            if (m_ScenarioManager == null)
                m_ScenarioManager = ScenarioManager.instance;

            m_AllTargets.AddRange(FindObjectsByType<ObjectiveTarget>(FindObjectsInactive.Include));
            for (var i = 0; i < m_AllTargets.Count; i++)
            {
                var t = m_AllTargets[i];
                if (t.step == null) continue;
                if (!m_TargetsByStep.TryGetValue(t.step, out var list))
                {
                    list = new List<ObjectiveTarget>();
                    m_TargetsByStep[t.step] = list;
                }
                list.Add(t);
                t.SetActiveVisuals(false);
            }

            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onStepChanged.AddListener(OnStepChanged);
                m_ScenarioManager.onScenarioComplete.AddListener(OnScenarioComplete);
                if (m_ScenarioManager.currentStep != null)
                    OnStepChanged(m_ScenarioManager.currentStep);
            }
        }

        void LateUpdate()
        {
            if (m_ActiveTargets.Count == 0)
                return;

            var bob = Mathf.Sin(Time.time * m_BobSpeed) * m_BobAmplitude;
            var spin = Quaternion.Euler(0f, Time.time * m_SpinSpeed, 0f);

            var markerIndex = 0;
            for (var i = 0; i < m_ActiveTargets.Count; i++)
            {
                var t = m_ActiveTargets[i];
                if (t == null) continue;

                var marker = GetMarker(markerIndex++);
                var show = !t.isSatisfied && t.gameObject.activeInHierarchy;
                if (marker.gameObject.activeSelf != show)
                    marker.gameObject.SetActive(show);
                if (!show) continue;

                marker.position = t.GetMarkerPosition() + Vector3.up * bob;
                marker.rotation = spin;
            }

            for (var i = markerIndex; i < m_MarkerPool.Count; i++)
            {
                if (m_MarkerPool[i].gameObject.activeSelf)
                    m_MarkerPool[i].gameObject.SetActive(false);
            }
        }

        void OnStepChanged(MissionStepSO step)
        {
            for (var i = 0; i < m_ActiveTargets.Count; i++)
                if (m_ActiveTargets[i] != null) m_ActiveTargets[i].SetActiveVisuals(false);
            m_ActiveTargets.Clear();

            if (step != null && m_TargetsByStep.TryGetValue(step, out var list))
            {
                m_ActiveTargets.AddRange(list);
                for (var i = 0; i < list.Count; i++)
                    list[i].SetActiveVisuals(true);
            }
        }

        void OnScenarioComplete()
        {
            OnStepChanged(null);
            for (var i = 0; i < m_MarkerPool.Count; i++)
                m_MarkerPool[i].gameObject.SetActive(false);
        }

        Transform GetMarker(int index)
        {
            while (m_MarkerPool.Count <= index)
            {
                GameObject go;
                if (m_MarkerTemplate != null)
                {
                    go = Instantiate(m_MarkerTemplate, transform);
                }
                else
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var col = go.GetComponent<Collider>();
                    if (col != null) Destroy(col);
                    go.transform.SetParent(transform, false);
                    go.transform.localScale = Vector3.one * 0.1f;
                }
                go.name = "ObjectiveMarker";
                go.SetActive(false);
                m_MarkerPool.Add(go.transform);
            }
            return m_MarkerPool[index];
        }
    }
}
