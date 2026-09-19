using UnityEngine;

namespace SafeZoneVR
{
    public class ObjectiveItemId : MonoBehaviour
    {
        [SerializeField]
        string m_ItemId = "item_001";

        [SerializeField]
        string m_DisplayName = "";

        public string itemId
        {
            get => m_ItemId;
            set => m_ItemId = value;
        }

        public string displayName
        {
            get => string.IsNullOrEmpty(m_DisplayName) ? m_ItemId : m_DisplayName;
            set => m_DisplayName = value;
        }
    }
}
