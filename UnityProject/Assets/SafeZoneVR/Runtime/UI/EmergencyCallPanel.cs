using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class EmergencyCallPanel : MonoBehaviour
    {
        [Serializable] public class Line { public string number; public string label; [TextArea] public string reply; public Button button; }

        [SerializeField] GameObject m_Root;
        [SerializeField] Button m_OpenButton;
        [SerializeField] Button m_CloseButton;
        [SerializeField] TextMeshProUGUI m_ReplyText;
        [SerializeField] List<Line> m_Lines = new List<Line>();
        [SerializeField] List<string> m_AcceptedNumbers = new List<string> { "199", "193" };
        [SerializeField] UnityEvent m_OnAcceptedCall = new UnityEvent();

        public List<Line> lines => m_Lines;
        public List<string> acceptedNumbers => m_AcceptedNumbers;
        public UnityEvent onAcceptedCall => m_OnAcceptedCall;
        public GameObject root { get => m_Root; set => m_Root = value; }
        public Button openButton { get => m_OpenButton; set => m_OpenButton = value; }
        public Button closeButton { get => m_CloseButton; set => m_CloseButton = value; }
        public TextMeshProUGUI replyText { get => m_ReplyText; set => m_ReplyText = value; }

        void Awake()
        {
            if (m_OpenButton != null) m_OpenButton.onClick.AddListener(Toggle);
            if (m_CloseButton != null) m_CloseButton.onClick.AddListener(() => m_Root.SetActive(false));
            foreach (var line in m_Lines)
            {
                var captured = line;
                if (line.button != null) line.button.onClick.AddListener(() => Call(captured.number));
            }
        }

        void Start() { if (m_Root != null) m_Root.SetActive(false); }

        public void Toggle() { if (m_Root != null) m_Root.SetActive(!m_Root.activeSelf); }

        public void Call(string number)
        {
            var line = m_Lines.Find(l => l.number == number);
            if (line != null) StartCoroutine(CallRoutine(line));
        }

        IEnumerator CallRoutine(Line line)
        {
            if (m_ReplyText != null) m_ReplyText.text = $"Chamando {line.label} ({line.number})…";
            yield return new WaitForSeconds(1.2f);
            if (m_ReplyText != null) m_ReplyText.text = line.reply;
            if (m_AcceptedNumbers.Contains(line.number)) m_OnAcceptedCall.Invoke();
        }
    }
}
