using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    public class BackpackStorage : MonoBehaviour
    {
        [SerializeField]
        List<XRSocketInteractor> m_Slots = new List<XRSocketInteractor>();

        [SerializeField]
        [Tooltip("Esconde o item enquanto ele está guardado, como se tivesse entrado na mochila.")]
        bool m_HideStoredItems = true;

        bool m_Started;

        public List<XRSocketInteractor> slots => m_Slots;
        public bool hideStoredItems { get => m_HideStoredItems; set => m_HideStoredItems = value; }

        void OnEnable()
        {
            for (var i = 0; i < m_Slots.Count; i++)
            {
                var s = m_Slots[i];
                if (s == null) continue;
                s.selectEntered.AddListener(OnSlotChanged);
                s.selectExited.AddListener(OnSlotChanged);
            }
            if (m_Started)
                Refresh();
        }

        void Start()
        {
            m_Started = true;
            Refresh();
        }

        void OnDisable()
        {
            for (var i = 0; i < m_Slots.Count; i++)
            {
                var s = m_Slots[i];
                if (s == null) continue;
                s.selectEntered.RemoveListener(OnSlotChanged);
                s.selectExited.RemoveListener(OnSlotChanged);
            }
        }

        void OnSlotChanged(BaseInteractionEventArgs args)
        {
            if (m_HideStoredItems)
            {
                if (args is SelectEnterEventArgs entered)
                    SetItemVisible(entered.interactableObject as Component, false);
                else if (args is SelectExitEventArgs exited)
                    SetItemVisible(exited.interactableObject as Component, true);
            }
            Refresh();
        }

        static void SetItemVisible(Component item, bool visible)
        {
            if (item == null)
                return;
            foreach (var r in item.GetComponentsInChildren<Renderer>(true))
                r.enabled = visible;
        }

        public void Refresh()
        {
            var emptyActivated = false;
            for (var i = 0; i < m_Slots.Count; i++)
            {
                var s = m_Slots[i];
                if (s == null) continue;
                if (s.hasSelection)
                {
                    s.socketActive = true;
                    continue;
                }
                s.socketActive = !emptyActivated;
                emptyActivated = true;
            }
        }
    }
}
