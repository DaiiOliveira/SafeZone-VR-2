using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Luzes internas da casa. Ficam acesas o tempo todo (sem depender de bake de iluminação) e só apagam
    /// quando o quadro de energia é desligado — ligado ao <see cref="BreakerLever"/> pelo builder da fase.
    /// </summary>
    public class HouseLights : MonoBehaviour
    {
        [SerializeField]
        List<Light> m_Lights = new List<Light>();

        [SerializeField]
        [Tooltip("Estado inicial: com energia, as luzes da casa ficam acesas.")]
        bool m_Powered = true;

        public List<Light> lights => m_Lights;
        public bool powered { get => m_Powered; set => SetPowered(value); }

        void Start()
        {
            Apply();
        }

        /// <summary>Liga/desliga todas as luzes de uma vez (alavanca do quadro de energia).</summary>
        public void SetPowered(bool on)
        {
            m_Powered = on;
            Apply();
        }

        void Apply()
        {
            for (var i = 0; i < m_Lights.Count; i++)
                if (m_Lights[i] != null)
                    m_Lights[i].enabled = m_Powered;
        }
    }
}
