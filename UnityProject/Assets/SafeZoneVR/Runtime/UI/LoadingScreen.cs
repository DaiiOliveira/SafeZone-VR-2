using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] GameObject m_Root;
        [SerializeField] TextMeshProUGUI m_TitleText;
        [SerializeField] TextMeshProUGUI m_ProgressText;
        [SerializeField] RectTransform m_ProgressBar;
        [SerializeField] float m_FadeSeconds = 0.5f;
        [SerializeField] float m_MinimumSeconds = 0.6f;

        static LoadingScreen s_Instance;
        bool m_Loading;
        float m_BarWidth = -1f;

        public static LoadingScreen instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = FindAnyObjectByType<LoadingScreen>(FindObjectsInactive.Include);
                return s_Instance;
            }
        }

        public GameObject root { get => m_Root; set => m_Root = value; }
        public TextMeshProUGUI titleText { get => m_TitleText; set => m_TitleText = value; }
        public TextMeshProUGUI progressText { get => m_ProgressText; set => m_ProgressText = value; }
        public RectTransform progressBar { get => m_ProgressBar; set => m_ProgressBar = value; }
        public bool isLoading => m_Loading;

        void Awake()
        {
            s_Instance = this;
            if (m_Root == null)
                m_Root = gameObject;
            m_Root.SetActive(false);
        }

        void OnDestroy()
        {
            if (s_Instance == this)
                s_Instance = null;
        }

        public void Load(string sceneName, string label)
        {
            if (m_Loading)
                return;
            m_Loading = true;
            StartCoroutine(LoadRoutine(sceneName, label));
        }

        IEnumerator LoadRoutine(string sceneName, string label)
        {
            var fader = ScreenFader.instance;
            if (fader != null)
            {
                fader.FadeTo(1f);
                yield return new WaitForSeconds(m_FadeSeconds + 0.05f);
            }

            if (m_TitleText != null)
                m_TitleText.text = string.IsNullOrEmpty(label) ? "Carregando…" : $"Carregando {label}…";
            SetProgress(0f);
            m_Root.SetActive(true);

            var started = Time.unscaledTime;
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                SetProgress(operation.progress / 0.9f);
                yield return null;
            }

            SetProgress(1f);
            while (Time.unscaledTime - started < m_MinimumSeconds)
                yield return null;

            operation.allowSceneActivation = true;
        }

        void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);
            if (m_ProgressText != null)
                m_ProgressText.text = Mathf.RoundToInt(value * 100f) + "%";
            if (m_ProgressBar == null)
                return;
            if (m_BarWidth < 0f)
                m_BarWidth = m_ProgressBar.offsetMax.x - m_ProgressBar.offsetMin.x;
            m_ProgressBar.offsetMax = new Vector2(m_ProgressBar.offsetMin.x + m_BarWidth * value, m_ProgressBar.offsetMax.y);
        }
    }
}
