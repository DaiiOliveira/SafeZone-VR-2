using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    /// <summary>
    /// Na jarra de água: se o fogo estiver aceso, dá o "puff" (o erro é registrado pelo WrongActionInteractable).
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class FlareOnSelect : MonoBehaviour
    {
        [SerializeField] FireController m_Fire;
        public FireController fire { get => m_Fire; set => m_Fire = value; }
        void Awake() => GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => { if (m_Fire != null) m_Fire.Flare(); });
    }
}
