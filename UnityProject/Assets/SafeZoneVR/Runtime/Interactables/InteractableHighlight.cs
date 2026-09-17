using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    [DisallowMultipleComponent]
    public class InteractableHighlight : MonoBehaviour
    {
        static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int k_Color = Shader.PropertyToID("_Color");

        [SerializeField] Color m_HighlightColor = new Color(1f, 0.86f, 0.35f);
        [SerializeField, Range(0f, 1f)] float m_NearStrength = 0.45f;
        [SerializeField, Range(0f, 1f)] float m_FarStrength = 0.22f;
        [SerializeField] float m_NearDistance = 0.45f;

        struct Slot
        {
            public Renderer renderer;
            public int materialIndex;
            public int property;
            public Color original;
            public bool hadBlock;
        }

        XRBaseInteractable m_Interactable;
        readonly List<Slot> m_Slots = new List<Slot>();
        readonly List<IXRHoverInteractor> m_Hovering = new List<IXRHoverInteractor>();
        MaterialPropertyBlock m_Block;
        float m_Applied = -1f;

        public Color highlightColor { get => m_HighlightColor; set => m_HighlightColor = value; }

        void Awake()
        {
            m_Interactable = GetComponent<XRBaseInteractable>();
            m_Block = new MaterialPropertyBlock();
            CollectRenderers();
        }

        void OnEnable()
        {
            if (m_Interactable == null) return;
            m_Interactable.hoverEntered.AddListener(OnHoverEntered);
            m_Interactable.hoverExited.AddListener(OnHoverExited);
        }

        void OnDisable()
        {
            if (m_Interactable != null)
            {
                m_Interactable.hoverEntered.RemoveListener(OnHoverEntered);
                m_Interactable.hoverExited.RemoveListener(OnHoverExited);
            }
            m_Hovering.Clear();
            Apply(0f);
        }

        void CollectRenderers()
        {
            var found = new List<Renderer>();
            found.AddRange(GetComponentsInChildren<Renderer>(true));
            if (m_Interactable != null)
            {
                foreach (var c in m_Interactable.colliders)
                    if (c != null && !c.transform.IsChildOf(transform))
                        found.AddRange(c.GetComponentsInChildren<Renderer>(true));
            }

            foreach (var r in found)
            {
                if (r == null || r is ParticleSystemRenderer || r is LineRenderer || r is TrailRenderer) continue;
                if (r.GetComponent<TMP_Text>() != null) continue;
                var owner = r.GetComponentInParent<XRBaseInteractable>(true);
                if (owner != null && owner != m_Interactable) continue;

                var materials = r.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var m = materials[i];
                    if (m == null) continue;
                    var property = m.HasProperty(k_BaseColor) ? k_BaseColor : m.HasProperty(k_Color) ? k_Color : 0;
                    if (property == 0) continue;
                    m_Slots.Add(new Slot { renderer = r, materialIndex = i, property = property, original = m.GetColor(property), hadBlock = r.HasPropertyBlock() });
                }
            }
        }

        static bool Counts(IXRHoverInteractor interactor)
        {
            return !(interactor is XRSocketInteractor) && !(interactor is XRGazeInteractor);
        }

        void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (Counts(args.interactorObject) && !m_Hovering.Contains(args.interactorObject))
                m_Hovering.Add(args.interactorObject);
        }

        void OnHoverExited(HoverExitEventArgs args)
        {
            m_Hovering.Remove(args.interactorObject);
            if (m_Hovering.Count == 0)
                Apply(0f);
        }

        void LateUpdate()
        {
            if (m_Hovering.Count == 0)
                return;
            var strength = m_Interactable != null && m_Interactable.isSelected ? m_FarStrength * 0.5f : m_FarStrength;
            for (var i = 0; i < m_Hovering.Count; i++)
            {
                var h = m_Hovering[i];
                if (h == null || h.transform == null) continue;
                if (InteractionDistance.IsNear(m_Interactable, h, m_NearDistance))
                {
                    strength = m_NearStrength;
                    break;
                }
            }
            Apply(strength);
        }

        void Apply(float strength)
        {
            if (Mathf.Approximately(strength, m_Applied))
                return;
            m_Applied = strength;
            foreach (var slot in m_Slots)
            {
                if (slot.renderer == null) continue;
                if (strength <= 0f && !slot.hadBlock)
                {
                    slot.renderer.SetPropertyBlock(null, slot.materialIndex);
                    continue;
                }
                slot.renderer.GetPropertyBlock(m_Block, slot.materialIndex);
                if (strength <= 0f)
                {
                    m_Block.SetColor(slot.property, slot.original);
                }
                else
                {
                    var c = Color.Lerp(slot.original, m_HighlightColor, strength);
                    c.a = slot.original.a;
                    m_Block.SetColor(slot.property, c);
                }
                slot.renderer.SetPropertyBlock(m_Block, slot.materialIndex);
            }
        }
    }
}
