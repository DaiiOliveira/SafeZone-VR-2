using UnityEngine;

namespace SafeZoneVR
{
    public class SmokeLayer : MonoBehaviour
    {
        [SerializeField] BoxCollider m_Zone;
        [SerializeField] Transform m_LayerVisual;
        [SerializeField] float m_StartBottom = 2.0f, m_EndBottom = 1.15f, m_SeatedBottom = 1.6f, m_DescendSeconds = 40f;
        [SerializeField] Color m_FogColor = new Color(0.36f, 0.36f, 0.38f);
        [SerializeField] float m_FogDensity = 0.35f;
        [SerializeField] string m_MistakeId = "em_pe_na_fumaca";
        [SerializeField, TextArea] string m_Message = "Abaixe-se: a fumaça tóxica fica no alto e o ar perto do chão é mais limpo.";
        [SerializeField, TextArea] string m_SeatedTip = "Em pé, você deveria se abaixar: a fumaça fica no alto.";
        [SerializeField] float m_CheckInterval = 0.2f;
        [SerializeField] float m_DamagePerSecond = 7f;
        [SerializeField, TextArea] string m_Advice = "Ande agachado: a fumaça tóxica sobe e o ar limpo fica perto do chão.";
        float m_StartTime, m_NextCheck; bool m_Active, m_FogOn;
        bool m_SavedFog; Color m_SavedColor; float m_SavedDensity; FogMode m_SavedMode;

        public BoxCollider zone { get => m_Zone; set => m_Zone = value; }
        public Transform layerVisual { get => m_LayerVisual; set => m_LayerVisual = value; }
        public bool isActive => m_Active;
        public bool fogOn => m_FogOn;
        public float currentBottom { get; private set; }

        public void Begin()
        {
            if (m_Active) return;
            m_Active = true;
            m_StartTime = Time.time;
            if (m_LayerVisual != null) m_LayerVisual.gameObject.SetActive(true);
            m_SavedFog = RenderSettings.fog; m_SavedColor = RenderSettings.fogColor;
            m_SavedDensity = RenderSettings.fogDensity; m_SavedMode = RenderSettings.fogMode;
            if (ComfortSettings.seatedMode && ScenarioManager.instance != null) ScenarioManager.instance.ShowInfo(m_SeatedTip);
        }

        void Update()
        {
            if (!m_Active || Time.time < m_NextCheck) return;
            m_NextCheck = Time.time + m_CheckInterval;

            var seated = ComfortSettings.seatedMode;
            var k = Mathf.Clamp01((Time.time - m_StartTime) / m_DescendSeconds);
            var bottom = Mathf.Lerp(m_StartBottom, seated ? m_SeatedBottom : m_EndBottom, k);
            var floorY = m_Zone != null ? m_Zone.bounds.min.y : 0f;
            currentBottom = floorY + bottom;
            if (m_LayerVisual != null)
            {
                var p = m_LayerVisual.position;
                p.y = currentBottom;
                m_LayerVisual.position = p;
            }

            var inSmoke = m_Zone != null && PlayerLocator.TryGetHeadPosition(out var head) && m_Zone.bounds.Contains(head) && head.y > currentBottom;
            SetFog(inSmoke);
            if (!inSmoke || seated || k < 0.5f)
                return;
            if (ScenarioManager.instance != null)
                ScenarioManager.instance.RegisterMistake(m_MistakeId, m_Message, 10f, ActionKind.ErroGrave, false, m_Advice);
            if (PlayerIntegrity.instance != null)
                PlayerIntegrity.instance.Damage(m_DamagePerSecond * m_CheckInterval, "A fumaça tomou o cômodo enquanto você estava em pé.");
        }

        void SetFog(bool on)
        {
            if (on == m_FogOn) return;
            m_FogOn = on;
            if (on) { RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogColor = m_FogColor; RenderSettings.fogDensity = m_FogDensity; }
            else    { RenderSettings.fog = m_SavedFog; RenderSettings.fogMode = m_SavedMode; RenderSettings.fogColor = m_SavedColor; RenderSettings.fogDensity = m_SavedDensity; }
        }

        void OnDisable() { if (m_FogOn) SetFog(false); }
    }
}
