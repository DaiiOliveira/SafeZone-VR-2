using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SafeZoneVR
{
    /// <summary>
    /// Raycaster de tela (mouse) para canvases world-space, ao lado do <see cref="TrackedDeviceGraphicRaycaster"/>.
    /// O TrackedDeviceGraphicRaycaster só responde a raios de controle: sem este componente o "apontar e clicar"
    /// do XR Interaction Simulator não encontra os botões e aperta o grip em vez do gatilho (botão não clica).
    /// Ignora eventos de controle rastreado para não duplicar cliques e se desliga no Quest (sem mouse).
    /// </summary>
    public class ScreenPointGraphicRaycaster : GraphicRaycaster
    {
        protected override void Awake()
        {
            base.Awake();
            if (Application.isMobilePlatform)
                enabled = false;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (eventData is TrackedDeviceEventData)
                return;
            base.Raycast(eventData, resultAppendList);
        }
    }
}
