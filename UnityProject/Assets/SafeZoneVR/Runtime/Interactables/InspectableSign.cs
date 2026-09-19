using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class InspectableSign : MonoBehaviour
    {
        static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] TextMeshPro m_Label;
        [SerializeField, TextArea] string m_Explanation;
        [SerializeField] Renderer m_Highlight;
        [SerializeField] float m_LabelSeconds = 6f;

        public bool inspected { get; private set; }
        public TextMeshPro label { get => m_Label; set => m_Label = value; }
        public string explanation { get => m_Explanation; set => m_Explanation = value; }
        public Renderer highlight { get => m_Highlight; set => m_Highlight = value; }

        void Awake() => GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => Inspect());

        public void Inspect()
        {
            inspected = true;
            if (m_Label != null)
            {
                m_Label.text = m_Explanation;
                m_Label.gameObject.SetActive(true);
                CancelInvoke();
                Invoke(nameof(HideLabel), m_LabelSeconds);
            }
            if (m_Highlight != null)
            {
                var b = new MaterialPropertyBlock();
                b.SetColor(k_BaseColor, new Color(1f, 0.75f, 0.3f));
                m_Highlight.SetPropertyBlock(b);
            }
        }

        void HideLabel() { if (m_Label != null) m_Label.gameObject.SetActive(false); }
    }
}
