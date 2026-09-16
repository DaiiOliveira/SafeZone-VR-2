using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    /// <summary>
    /// Vários sockets compartilham uma área de captura do tamanho da mochila e apenas o primeiro espaço
    /// vazio fica ativo: o item é guardado ao ser solto perto da mochila e só um "fantasma" aparece por vez.
    /// </summary>
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

        // Primeira atualização só depois do Awake de todos os sockets da cena.
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

        /// <summary>Espaços ocupados continuam ativos (seguram o item); dos vazios, só o primeiro aceita itens.</summary>
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
