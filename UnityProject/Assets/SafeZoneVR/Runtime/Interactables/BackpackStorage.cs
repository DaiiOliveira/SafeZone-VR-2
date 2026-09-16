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

        bool m_Started;

        public List<XRSocketInteractor> slots => m_Slots;

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
            Refresh();
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
