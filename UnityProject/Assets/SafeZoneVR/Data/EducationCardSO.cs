using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>Card educativo exibido no relatório final (texto extraído das cartilhas da Defesa Civil).</summary>
    [CreateAssetMenu(fileName = "EducationCard", menuName = "SafeZone VR/Education Card", order = 20)]
    public class EducationCardSO : ScriptableObject
    {
        [SerializeField]
        string m_Title = "Card Title";

        [SerializeField]
        [TextArea(2, 6)]
        string m_TipText = "Card tip text";

        [SerializeField]
        string m_Source = "Defesa Civil / MDR";

        public string title => m_Title;
        public string tipText => m_TipText;
        public string source => m_Source;

#if UNITY_EDITOR
        public void EditorInitialize(string title, string tipText, string source)
        {
            m_Title = title;
            m_TipText = tipText;
            m_Source = source;
        }
#endif
    }
}
