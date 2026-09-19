using System;
using UnityEngine;

namespace SafeZoneVR
{
    public enum LocomotionMode
    {
        Teleport = 0,
        Continuous = 1
    }

    public enum TurnMode
    {
        Snap = 0,
        Smooth = 1
    }

    public enum DominantHand
    {
        Right = 0,
        Left = 1
    }

    public static class ComfortSettings
    {
        const string k_Locomotion = "safezone.comfort.locomotion";
        const string k_Turn = "safezone.comfort.turn";
        const string k_Vignette = "safezone.comfort.vignette";
        const string k_Seated = "safezone.comfort.seated";
        const string k_DominantHand = "safezone.comfort.dominantHand";

        static bool s_Loaded;
        static LocomotionMode s_Locomotion = LocomotionMode.Teleport;
        static TurnMode s_Turn = TurnMode.Snap;
        static bool s_Vignette = true;
        static bool s_Seated;
        static DominantHand s_DominantHand = DominantHand.Right;

        public static event Action changed;

        public static LocomotionMode locomotion
        {
            get { Load(); return s_Locomotion; }
            set { Load(); if (s_Locomotion == value) return; s_Locomotion = value; Save(); }
        }

        public static TurnMode turn
        {
            get { Load(); return s_Turn; }
            set { Load(); if (s_Turn == value) return; s_Turn = value; Save(); }
        }

        public static bool vignetteEnabled
        {
            get { Load(); return s_Vignette; }
            set { Load(); if (s_Vignette == value) return; s_Vignette = value; Save(); }
        }

        public static bool seatedMode
        {
            get { Load(); return s_Seated; }
            set { Load(); if (s_Seated == value) return; s_Seated = value; Save(); }
        }

        public static DominantHand dominantHand
        {
            get { Load(); return s_DominantHand; }
            set { Load(); if (s_DominantHand == value) return; s_DominantHand = value; Save(); }
        }

        public static bool leftHanded => dominantHand == DominantHand.Left;

        public static void ToggleLocomotion() => locomotion = locomotion == LocomotionMode.Teleport ? LocomotionMode.Continuous : LocomotionMode.Teleport;
        public static void ToggleTurn() => turn = turn == TurnMode.Snap ? TurnMode.Smooth : TurnMode.Snap;
        public static void ToggleVignette() => vignetteEnabled = !vignetteEnabled;
        public static void ToggleSeated() => seatedMode = !seatedMode;
        public static void ToggleDominantHand() => dominantHand = leftHanded ? DominantHand.Right : DominantHand.Left;

        static void Load()
        {
            if (s_Loaded) return;
            s_Loaded = true;
            s_Locomotion = (LocomotionMode)PlayerPrefs.GetInt(k_Locomotion, (int)LocomotionMode.Teleport);
            s_Turn = (TurnMode)PlayerPrefs.GetInt(k_Turn, (int)TurnMode.Snap);
            s_Vignette = PlayerPrefs.GetInt(k_Vignette, 1) == 1;
            s_Seated = PlayerPrefs.GetInt(k_Seated, 0) == 1;
            s_DominantHand = (DominantHand)PlayerPrefs.GetInt(k_DominantHand, (int)DominantHand.Right);
        }

        static void Save()
        {
            PlayerPrefs.SetInt(k_Locomotion, (int)s_Locomotion);
            PlayerPrefs.SetInt(k_Turn, (int)s_Turn);
            PlayerPrefs.SetInt(k_Vignette, s_Vignette ? 1 : 0);
            PlayerPrefs.SetInt(k_Seated, s_Seated ? 1 : 0);
            PlayerPrefs.SetInt(k_DominantHand, (int)s_DominantHand);
            PlayerPrefs.Save();
            changed?.Invoke();
        }
    }
}
