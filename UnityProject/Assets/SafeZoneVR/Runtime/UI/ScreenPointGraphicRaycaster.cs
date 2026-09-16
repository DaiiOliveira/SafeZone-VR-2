using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SafeZoneVR
{
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
