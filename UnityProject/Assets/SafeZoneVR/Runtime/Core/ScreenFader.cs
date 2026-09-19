using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SafeZoneVR
{
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField]
        Material m_FadeMaterial;

        [SerializeField]
        float m_FadeDuration = 0.5f;

        [SerializeField]
        bool m_FadeInOnStart = true;

        MeshRenderer m_Renderer;
        Material m_Instance;
        float m_Alpha;
        Coroutine m_Routine;

        public static ScreenFader instance { get; private set; }

        public Material fadeMaterial
        {
            get => m_FadeMaterial;
            set => m_FadeMaterial = value;
        }

        void Awake()
        {
            instance = this;

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "FadeQuad";
            var col = quad.GetComponent<Collider>();
            if (col != null) Destroy(col);
            quad.transform.SetParent(transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale = new Vector3(4f, 4f, 1f);
            quad.layer = gameObject.layer;

            m_Renderer = quad.GetComponent<MeshRenderer>();
            m_Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_Renderer.receiveShadows = false;
            if (m_FadeMaterial != null)
            {
                m_Instance = new Material(m_FadeMaterial);
                m_Renderer.sharedMaterial = m_Instance;
            }

            SetAlpha(m_FadeInOnStart ? 1f : 0f);
        }

        void Start()
        {
            if (m_FadeInOnStart)
                FadeTo(0f);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
            if (m_Instance != null)
                Destroy(m_Instance);
        }

        public bool isBusy => m_Routine != null;

        public void SetOverlay(float alpha)
        {
            if (m_Routine != null)
                return;
            SetAlpha(alpha);
        }

        public void FadeTo(float alpha)
        {
            if (m_Routine != null)
                StopCoroutine(m_Routine);
            m_Routine = StartCoroutine(FadeRoutine(alpha, null));
        }

        public void FadeOutAndLoad(string sceneName)
        {
            if (m_Routine != null)
                StopCoroutine(m_Routine);
            m_Routine = StartCoroutine(FadeRoutine(1f, sceneName));
        }

        IEnumerator FadeRoutine(float target, string sceneToLoad)
        {
            var from = m_Alpha;
            var t = 0f;
            var duration = Mathf.Max(0.01f, m_FadeDuration);
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(from, target, t / duration));
                yield return null;
            }
            SetAlpha(target);
            m_Routine = null;

            if (!string.IsNullOrEmpty(sceneToLoad))
                SceneManager.LoadScene(sceneToLoad);
        }

        void SetAlpha(float a)
        {
            m_Alpha = Mathf.Clamp01(a);
            if (m_Renderer == null)
                return;
            m_Renderer.enabled = m_Alpha > 0.001f;
            if (m_Instance != null)
            {
                var c = m_Instance.color;
                c.a = m_Alpha;
                m_Instance.color = c;
            }
        }
    }
}
