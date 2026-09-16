using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    public class StepCompleteOnSocket : StepValidatorBase
    {
        [SerializeField]
        List<XRSocketInteractor> m_Sockets = new List<XRSocketInteractor>();

        [SerializeField]
        [Tooltip("IDs de item exigidos. Se vazio, basta preencher todos os sockets com qualquer item identificado.")]
        List<string> m_RequiredItemIds = new List<string>();

        readonly Dictionary<XRSocketInteractor, string> m_Placed = new Dictionary<XRSocketInteractor, string>();

        public List<XRSocketInteractor> sockets => m_Sockets;
        public List<string> requiredItemIds => m_RequiredItemIds;

        public int requiredCount => m_RequiredItemIds.Count > 0 ? m_RequiredItemIds.Count : m_Sockets.Count;

        public int placedCount
        {
            get
            {
                if (m_RequiredItemIds.Count == 0)
                    return m_Placed.Count;

                var count = 0;
                for (var i = 0; i < m_RequiredItemIds.Count; i++)
                {
                    if (m_Placed.ContainsValue(m_RequiredItemIds[i]))
                        count++;
                }
                return count;
            }
        }

        void OnEnable()
        {
            for (var i = 0; i < m_Sockets.Count; i++)
            {
                var s = m_Sockets[i];
                if (s == null) continue;
                s.selectEntered.AddListener(OnSocketSelectEntered);
                s.selectExited.AddListener(OnSocketSelectExited);
            }
        }

        void OnDisable()
        {
            for (var i = 0; i < m_Sockets.Count; i++)
            {
                var s = m_Sockets[i];
                if (s == null) continue;
                s.selectEntered.RemoveListener(OnSocketSelectEntered);
                s.selectExited.RemoveListener(OnSocketSelectExited);
            }
        }

        void OnSocketSelectEntered(SelectEnterEventArgs args)
        {
            if (m_Completed)
                return;

            var socket = args.interactorObject as XRSocketInteractor;
            var itemTransform = args.interactableObject.transform;
            var item = itemTransform != null ? itemTransform.GetComponent<ObjectiveItemId>() : null;
            if (socket == null || item == null)
                return;

            if (m_RequiredItemIds.Count > 0 && !m_RequiredItemIds.Contains(item.itemId))
                return;

            m_Placed[socket] = item.itemId;

            var target = itemTransform.GetComponent<ObjectiveTarget>();
            if (target != null)
                target.SetSatisfied(true);

            var placed = placedCount;
            var required = requiredCount;
            ReportProgress(placed, required);

            if (placed >= required)
                Complete();
        }

        void OnSocketSelectExited(SelectExitEventArgs args)
        {
            if (m_Completed)
                return;

            var socket = args.interactorObject as XRSocketInteractor;
            if (socket == null || !m_Placed.Remove(socket))
                return;

            if (args.isCanceled || !isActiveAndEnabled)
                return;

            var itemTransform = args.interactableObject.transform;
            var target = itemTransform != null ? itemTransform.GetComponent<ObjectiveTarget>() : null;
            if (target != null)
                target.SetSatisfied(false);

            ReportProgress(placedCount, requiredCount);
        }
    }
}
