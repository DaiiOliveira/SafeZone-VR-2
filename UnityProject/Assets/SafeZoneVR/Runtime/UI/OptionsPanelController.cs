using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SafeZoneVR
{
    public class OptionsPanelController : MonoBehaviour
    {
        [Serializable]
        public class VolumeRow
        {
            public AudioCategory category;
            public Button minus;
            public Button plus;
            public TextMeshProUGUI label;
        }

        [SerializeField]
        GameObject m_Root;

        [SerializeField]
        Button m_LocomotionButton;

        [SerializeField]
        TextMeshProUGUI m_LocomotionLabel;

        [SerializeField]
        Button m_TurnButton;

        [SerializeField]
        TextMeshProUGUI m_TurnLabel;

        [SerializeField]
        Button m_VignetteButton;

        [SerializeField]
        TextMeshProUGUI m_VignetteLabel;

        [SerializeField]
        Button m_SeatedButton;

        [SerializeField]
        TextMeshProUGUI m_SeatedLabel;

        [SerializeField]
        Button m_DominantHandButton;

        [SerializeField]
        TextMeshProUGUI m_DominantHandLabel;

        [SerializeField]
        List<VolumeRow> m_VolumeRows = new List<VolumeRow>();

        [SerializeField]
        Button m_RecenterButton;

        [SerializeField]
        Button m_RestartButton;

        [SerializeField]
        Button m_MenuButton;

        [SerializeField]
        Button m_CloseButton;

        [SerializeField]
        bool m_StartHidden = true;

        public GameObject root { get => m_Root; set => m_Root = value; }
        public Button locomotionButton { get => m_LocomotionButton; set => m_LocomotionButton = value; }
        public TextMeshProUGUI locomotionLabel { get => m_LocomotionLabel; set => m_LocomotionLabel = value; }
        public Button turnButton { get => m_TurnButton; set => m_TurnButton = value; }
        public TextMeshProUGUI turnLabel { get => m_TurnLabel; set => m_TurnLabel = value; }
        public Button vignetteButton { get => m_VignetteButton; set => m_VignetteButton = value; }
        public TextMeshProUGUI vignetteLabel { get => m_VignetteLabel; set => m_VignetteLabel = value; }
        public Button seatedButton { get => m_SeatedButton; set => m_SeatedButton = value; }
        public TextMeshProUGUI seatedLabel { get => m_SeatedLabel; set => m_SeatedLabel = value; }
        public Button dominantHandButton { get => m_DominantHandButton; set => m_DominantHandButton = value; }
        public TextMeshProUGUI dominantHandLabel { get => m_DominantHandLabel; set => m_DominantHandLabel = value; }
        public List<VolumeRow> volumeRows => m_VolumeRows;
        public Button recenterButton { get => m_RecenterButton; set => m_RecenterButton = value; }
        public Button restartButton { get => m_RestartButton; set => m_RestartButton = value; }
        public Button menuButton { get => m_MenuButton; set => m_MenuButton = value; }
        public Button closeButton { get => m_CloseButton; set => m_CloseButton = value; }
        public bool startHidden { get => m_StartHidden; set => m_StartHidden = value; }

        public bool isVisible => m_Root != null && m_Root.activeSelf;

        void Awake()
        {
            if (m_Root == null)
                m_Root = gameObject;

            Bind(m_LocomotionButton, () => { ComfortSettings.ToggleLocomotion(); RefreshLabels(); });
            Bind(m_TurnButton, () => { ComfortSettings.ToggleTurn(); RefreshLabels(); });
            Bind(m_VignetteButton, () => { ComfortSettings.ToggleVignette(); RefreshLabels(); });
            Bind(m_SeatedButton, () => { ComfortSettings.ToggleSeated(); RefreshLabels(); });
            Bind(m_DominantHandButton, () => { ComfortSettings.ToggleDominantHand(); RefreshLabels(); });
            foreach (var row in m_VolumeRows)
            {
                if (row == null) continue;
                var category = row.category;
                Bind(row.minus, () => { AudioVolumes.Decrease(category); RefreshLabels(); });
                Bind(row.plus, () => { AudioVolumes.Increase(category); RefreshLabels(); });
            }
            Bind(m_RecenterButton, ComfortSettingsApplier.Recenter);
            Bind(m_RestartButton, SceneFlow.ReloadCurrent);
            Bind(m_MenuButton, SceneFlow.LoadMenu);
            Bind(m_CloseButton, Hide);
        }

        void Start()
        {
            RefreshLabels();
            if (m_StartHidden)
                Hide();
        }

        public void Show()
        {
            RefreshLabels();
            if (m_Root != null)
                m_Root.SetActive(true);
        }

        public void Hide()
        {
            if (m_Root != null)
                m_Root.SetActive(false);
        }

        public void Toggle()
        {
            if (isVisible) Hide(); else Show();
        }

        public void RefreshLabels()
        {
            SetText(m_LocomotionLabel, ComfortSettings.locomotion == LocomotionMode.Teleport
                ? "Locomoção: Teleporte"
                : "Locomoção: Contínua");
            SetText(m_TurnLabel, ComfortSettings.turn == TurnMode.Snap
                ? "Giro: Em passos"
                : "Giro: Suave");
            SetText(m_VignetteLabel, ComfortSettings.vignetteEnabled
                ? "Vinheta de conforto: Ligada"
                : "Vinheta de conforto: Desligada");
            SetText(m_SeatedLabel, ComfortSettings.seatedMode
                ? "Posição: Sentado"
                : "Posição: Em pé");
            SetText(m_DominantHandLabel, ComfortSettings.leftHanded
                ? "Mão dominante: Esquerda"
                : "Mão dominante: Direita");
            foreach (var row in m_VolumeRows)
            {
                if (row == null) continue;
                SetText(row.label, $"{AudioVolumes.Label(row.category)}: {Mathf.RoundToInt(AudioVolumes.Get(row.category) * 100f)}%");
                if (row.minus != null) row.minus.interactable = AudioVolumes.Get(row.category) > 0.001f;
                if (row.plus != null) row.plus.interactable = AudioVolumes.Get(row.category) < 0.999f;
            }
        }

        static void Bind(Button b, UnityEngine.Events.UnityAction action)
        {
            if (b != null)
                b.onClick.AddListener(action);
        }

        static void SetText(TextMeshProUGUI t, string value)
        {
            if (t != null)
                t.text = value;
        }
    }
}
