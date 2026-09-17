using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class SafetyNoticePanel : MonoBehaviour
    {
        public const string k_AcceptedKey = "safezone.safety.accepted";
        public const int k_NoticeVersion = 1;

        [SerializeField] GameObject m_Root;
        [SerializeField] Button m_AcceptButton;
        [SerializeField] List<Button> m_BlockedButtons = new List<Button>();
        [SerializeField] UnityEvent m_OnAccepted = new UnityEvent();

        readonly List<bool> m_PreviousState = new List<bool>();

        public GameObject root { get => m_Root; set => m_Root = value; }
        public Button acceptButton { get => m_AcceptButton; set => m_AcceptButton = value; }
        public List<Button> blockedButtons => m_BlockedButtons;
        public UnityEvent onAccepted => m_OnAccepted;
        public bool isShowing => m_Root != null && m_Root.activeSelf;

        public static bool hasAccepted => PlayerPrefs.GetInt(k_AcceptedKey, 0) >= k_NoticeVersion;

        void Awake()
        {
            if (m_Root == null) m_Root = gameObject;
            if (m_AcceptButton != null) m_AcceptButton.onClick.AddListener(Accept);
        }

        void Start()
        {
            if (hasAccepted)
                m_Root.SetActive(false);
            else
                Show();
        }

        public void Show()
        {
            m_PreviousState.Clear();
            foreach (var b in m_BlockedButtons)
            {
                m_PreviousState.Add(b != null && b.interactable);
                if (b != null) b.interactable = false;
            }
            m_Root.SetActive(true);
        }

        public void Accept()
        {
            PlayerPrefs.SetInt(k_AcceptedKey, k_NoticeVersion);
            PlayerPrefs.Save();
            for (var i = 0; i < m_BlockedButtons.Count; i++)
                if (m_BlockedButtons[i] != null)
                    m_BlockedButtons[i].interactable = i < m_PreviousState.Count ? m_PreviousState[i] : true;
            m_Root.SetActive(false);
            m_OnAccepted.Invoke();
        }
    }
}
