using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Gera clipes de áudio simples em tempo de execução (sem depender de assets):
    /// sinos de acerto, aviso suave e chuva ambiente. Leve para Quest (mono, 22 kHz).
    /// </summary>
    public static class ProceduralAudio
    {
        const int k_SampleRate = 22050;

        /// <summary>Sequência de notas com decaimento (ex.: acerto = 3 notas ascendentes).</summary>
        public static AudioClip CreateChime(string name, float[] frequencies, float noteDuration = 0.16f, float volume = 0.5f)
        {
            var noteSamples = Mathf.RoundToInt(noteDuration * k_SampleRate);
            var tail = Mathf.RoundToInt(0.35f * k_SampleRate);
            var total = noteSamples * frequencies.Length + tail;
            var data = new float[total];

            for (var n = 0; n < frequencies.Length; n++)
            {
                var start = n * noteSamples;
                var f = frequencies[n];
                for (var i = 0; i < noteSamples + tail && start + i < total; i++)
                {
                    var t = i / (float)k_SampleRate;
                    var env = Mathf.Exp(-t * 6f);
                    var s = Mathf.Sin(2f * Mathf.PI * f * t) * 0.7f + Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.2f;
                    data[start + i] += s * env * volume;
                }
            }

            return Build(name, data);
        }

        /// <summary>Aviso curto e grave, sem agressividade.</summary>
        public static AudioClip CreateSoftWarning(string name, float volume = 0.35f)
        {
            var duration = 0.45f;
            var total = Mathf.RoundToInt(duration * k_SampleRate);
            var data = new float[total];
            for (var i = 0; i < total; i++)
            {
                var t = i / (float)k_SampleRate;
                var env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration));
                var f = 220f + Mathf.Sin(t * 12f) * 8f;
                data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * env * volume;
            }
            return Build(name, data);
        }

        /// <summary>Loop de chuva: ruído filtrado com leve variação.</summary>
        public static AudioClip CreateRainLoop(string name, float seconds = 4f, float volume = 0.25f)
        {
            var total = Mathf.RoundToInt(seconds * k_SampleRate);
            var data = new float[total];
            var rng = new System.Random(1234);
            var low = 0f;
            for (var i = 0; i < total; i++)
            {
                var white = (float)(rng.NextDouble() * 2.0 - 1.0);
                low = Mathf.Lerp(low, white, 0.25f);
                var t = i / (float)k_SampleRate;
                var swell = 0.85f + 0.15f * Mathf.Sin(t * 0.7f);
                data[i] = low * swell * volume;
            }
            // Fade nas pontas para o loop não estalar.
            var fade = Mathf.RoundToInt(0.1f * k_SampleRate);
            for (var i = 0; i < fade; i++)
            {
                var k = i / (float)fade;
                data[i] *= k;
                data[total - 1 - i] *= k;
            }
            return Build(name, data);
        }

        static AudioClip Build(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, k_SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
