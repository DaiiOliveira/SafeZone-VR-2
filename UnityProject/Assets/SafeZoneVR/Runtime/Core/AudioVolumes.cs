using System;
using UnityEngine;

namespace SafeZoneVR
{
    public enum AudioCategory
    {
        Effects = 0,
        Narration = 1,
        Ambient = 2
    }

    public static class AudioVolumes
    {
        const string k_Prefix = "safezone.audio.";
        const float k_Step = 0.1f;

        static bool s_Loaded;
        static readonly float[] s_Values = { 1f, 1f, 1f };

        public static event Action changed;

        public static float Get(AudioCategory category)
        {
            Load();
            return s_Values[(int)category];
        }

        public static void Set(AudioCategory category, float value)
        {
            Load();
            value = Mathf.Round(Mathf.Clamp01(value) * 10f) / 10f;
            if (Mathf.Approximately(s_Values[(int)category], value))
                return;
            s_Values[(int)category] = value;
            PlayerPrefs.SetFloat(k_Prefix + category, value);
            PlayerPrefs.Save();
            changed?.Invoke();
        }

        public static void Increase(AudioCategory category) => Set(category, Get(category) + k_Step);
        public static void Decrease(AudioCategory category) => Set(category, Get(category) - k_Step);

        public static string Label(AudioCategory category)
        {
            switch (category)
            {
                case AudioCategory.Narration: return "Narração";
                case AudioCategory.Ambient: return "Ambiente";
                default: return "Efeitos";
            }
        }

        static void Load()
        {
            if (s_Loaded) return;
            s_Loaded = true;
            for (var i = 0; i < s_Values.Length; i++)
                s_Values[i] = Mathf.Clamp01(PlayerPrefs.GetFloat(k_Prefix + (AudioCategory)i, 1f));
        }
    }
}
