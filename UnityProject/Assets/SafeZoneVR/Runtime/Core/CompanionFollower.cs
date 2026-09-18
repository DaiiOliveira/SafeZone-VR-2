using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    public class CompanionFollower : MonoBehaviour
    {
        [SerializeField] XRBaseInteractable m_TalkInteractable;
        [SerializeField] List<Transform> m_Waypoints = new List<Transform>();
        [SerializeField] float m_Speed = 0.8f;
        [SerializeField] float m_MaxLeadDistance = 3f;
        [SerializeField] TextMeshPro m_SpeechBubble;
        [SerializeField] UnityEvent m_OnStartedFollowing = new UnityEvent();
        [SerializeField] float m_SpeechSeconds = 6f;
        int m_Index; bool m_Following;

        public float speechSeconds { get => m_SpeechSeconds; set => m_SpeechSeconds = value; }
        public bool isFollowing => m_Following;
        public bool arrived => m_Following && m_Index >= m_Waypoints.Count;
        public List<Transform> waypoints => m_Waypoints;
        public UnityEvent onStartedFollowing => m_OnStartedFollowing;
        public XRBaseInteractable talkInteractable { get => m_TalkInteractable; set => m_TalkInteractable = value; }
        public float speed { get => m_Speed; set => m_Speed = value; }
        public float maxLeadDistance { get => m_MaxLeadDistance; set => m_MaxLeadDistance = value; }
        public TextMeshPro speechBubble { get => m_SpeechBubble; set => m_SpeechBubble = value; }

        void OnEnable()  { if (m_TalkInteractable != null) m_TalkInteractable.selectEntered.AddListener(OnTalk); }
        void OnDisable() { if (m_TalkInteractable != null) m_TalkInteractable.selectEntered.RemoveListener(OnTalk); }
        void OnTalk(SelectEnterEventArgs _) => StartFollowing();

        IEnumerator HideSpeech()
        {
            yield return new WaitForSeconds(m_SpeechSeconds);
            if (m_SpeechBubble != null) m_SpeechBubble.gameObject.SetActive(false);
        }

        public void StartFollowing()
        {
            if (m_Following) return;
            m_Following = true;
            if (m_SpeechBubble != null)
            {
                m_SpeechBubble.gameObject.SetActive(true);
                if (m_SpeechSeconds > 0f && isActiveAndEnabled) StartCoroutine(HideSpeech());
            }
            m_OnStartedFollowing.Invoke();
        }

        public void Restart(List<Transform> newRoute, bool follow = true)
        {
            m_Waypoints = newRoute;
            m_Index = 0;
            m_Following = false;
            if (follow) StartFollowing();
        }

        void Update()
        {
            if (!m_Following || m_Index >= m_Waypoints.Count) return;
            var pos = transform.position;
            var target = m_Waypoints[m_Index].position;

            if (PlayerLocator.TryGetFeetPosition(out var player))
            {
                var toPlayer = player - pos;
                if (toPlayer.sqrMagnitude > m_MaxLeadDistance * m_MaxLeadDistance && Vector3.Dot(toPlayer, target - pos) < 0f) return;
            }

            var next = Vector3.MoveTowards(pos, new Vector3(target.x, pos.y, target.z), m_Speed * Time.deltaTime);
            if (Physics.Raycast(next + Vector3.up, Vector3.down, out var hit, 2f, ~0, QueryTriggerInteraction.Ignore))
                next.y = hit.point.y;
            var dir = target - pos; dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
            transform.position = next;

            if (new Vector2(next.x - target.x, next.z - target.z).sqrMagnitude < 0.04f) m_Index++;
        }
    }
}
