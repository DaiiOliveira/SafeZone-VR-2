using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>Identifica um item de objetivo (ex.: "agua", "documentos") para validadores e filtros de socket.</summary>
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
