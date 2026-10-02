using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class EmergencyCallPanel : MonoBehaviour
    {
        [Serializable] public class Line { public string number; public string label; [TextArea] public string reply; }

        [SerializeField] GameObject m_Root;
        [SerializeField] Button m_OpenButton;
        [SerializeField] TextMeshProUGUI m_NumberText;
        [SerializeField] TextMeshProUGUI m_ReplyText;
        [SerializeField] TextMeshProUGUI m_ClockText;
        [SerializeField] string m_IdleText = "Digite o número e toque no botão verde.";
        [SerializeField] int m_MaxDigits = 12;
        [SerializeField] List<Line> m_Lines = new List<Line>();
        [SerializeField] List<string> m_AcceptedNumbers = new List<string> { "199", "193" };
        [SerializeField] UnityEvent m_OnAcceptedCall = new UnityEvent();

        string m_Number = string.Empty;
        bool m_Calling;
        bool m_ShowingReply;
        float m_NextClock;

        public List<Line> lines => m_Lines;
        public List<string> acceptedNumbers => m_AcceptedNumbers;
        public UnityEvent onAcceptedCall => m_OnAcceptedCall;
        public GameObject root { get => m_Root; set => m_Root = value; }
        public Button openButton { get => m_OpenButton; set => m_OpenButton = value; }
        public TextMeshProUGUI numberText { get => m_NumberText; set => m_NumberText = value; }
        public TextMeshProUGUI replyText { get => m_ReplyText; set => m_ReplyText = value; }
        public TextMeshProUGUI clockText { get => m_ClockText; set => m_ClockText = value; }
        public string idleText { get => m_IdleText; set => m_IdleText = value; }

        void Awake()
        {
            if (m_OpenButton != null) m_OpenButton.onClick.AddListener(Toggle);
        }

        void Start()
        {
            if (m_Root != null) m_Root.SetActive(false);
        }

        void Update()
        {
            if (m_ClockText == null || Time.unscaledTime < m_NextClock) return;
            m_NextClock = Time.unscaledTime + 1f;
            m_ClockText.text = DateTime.Now.ToString("HH:mm");
        }

        public void Toggle()
        {
            if (m_Root != null && m_Root.activeSelf) Close();
            else Open();
        }

        public void Open()
        {
            m_Number = string.Empty;
            if (m_Root != null) m_Root.SetActive(true);
            m_NextClock = 0f;
            ShowTip();
            Refresh();
        }

        public void Close()
        {
            StopAllCoroutines();
            m_Calling = false;
            if (m_Root != null) m_Root.SetActive(false);
        }

        public void Digit(string digit)
        {
            if (m_Calling || m_Number.Length >= m_MaxDigits) return;
            if (m_ShowingReply) ShowTip();
            m_Number += digit;
            Refresh();
        }

        public void Erase()
        {
            if (m_Calling || m_Number.Length == 0) return;
            m_Number = m_Number.Substring(0, m_Number.Length - 1);
            Refresh();
        }

        public void Dial()
        {
            if (m_Calling || m_Number.Length == 0) return;
            StartCoroutine(CallRoutine(m_Number));
        }

        void ShowTip()
        {
            m_ShowingReply = false;
            if (m_ReplyText == null) return;
            var tip = new StringBuilder("<b>Números de emergência</b>\n");
            foreach (var line in m_Lines)
                tip.Append(m_AcceptedNumbers.Contains(line.number)
                    ? $"<color=#5BD17A><b>{line.number}  {line.label}</b></color>\n"
                    : $"<color=#9AA6B8>{line.number}  {line.label}</color>\n");
            tip.Append($"<size=85%>{m_IdleText}</size>");
            m_ReplyText.text = tip.ToString();
        }

        void Refresh()
        {
            if (m_NumberText != null)
                m_NumberText.text = m_Number.Length > 0 ? m_Number : "<color=#7A8699>_ _ _</color>";
        }

        IEnumerator CallRoutine(string number)
        {
            m_Calling = true;
            if (m_ReplyText != null) m_ReplyText.text = "Chamando…";
            yield return new WaitForSeconds(1.2f);
            var line = m_Lines.Find(l => l.number == number);
            if (m_ReplyText != null)
                m_ReplyText.text = line != null ? line.reply : $"O número {number} não atende. Confira o número de emergência.";
            m_ShowingReply = true;
            m_Number = string.Empty;
            Refresh();
            m_Calling = false;
            if (m_AcceptedNumbers.Contains(number)) m_OnAcceptedCall.Invoke();
        }
    }
}
