using UnityEngine;

namespace SafeZoneVR
{
    public static class ProceduralAudio
    {
        const int k_SampleRate = 22050;

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

            var fade = Mathf.RoundToInt(0.1f * k_SampleRate);
            for (var i = 0; i < fade; i++)
            {
                var k = i / (float)fade;
                data[i] *= k;
                data[total - 1 - i] *= k;
            }
            return Build(name, data);
        }

        public static AudioClip CreateHailLoop(string name, float seconds = 3f, float hitsPerSecond = 90f, float volume = 0.35f)
        {
            var n = Mathf.CeilToInt(seconds * k_SampleRate);
            var data = new float[n];
            var rng = new System.Random(5);
            var hits = Mathf.RoundToInt(seconds * hitsPerSecond);
            for (var h = 0; h < hits; h++)
            {
                var start = rng.Next(n);
                var len = 220 + rng.Next(440);
                var amp = (float)(0.3 + rng.NextDouble() * 0.7) * volume;
                var freq = 1800f + (float)rng.NextDouble() * 2500f;
                for (var i = 0; i < len; i++)
                {
                    var idx = (start + i) % n;
                    data[idx] += Mathf.Sin(2f * Mathf.PI * freq * i / k_SampleRate) * Mathf.Exp(-i / (len * 0.25f)) * amp;
                }
            }
            Clamp(data);
            return Build(name, data);
        }

        public static AudioClip CreateCrackleLoop(string name, float seconds = 3f, float volume = 0.3f)
        {
            var n = Mathf.CeilToInt(seconds * k_SampleRate);
            var data = new float[n];
            var rng = new System.Random(3);
            var lp = 0f;
            for (var i = 0; i < n; i++)
            {
                lp += 0.05f * ((float)(rng.NextDouble() * 2.0 - 1.0) - lp);
                data[i] = lp * 1.5f * volume;
            }
            var pops = Mathf.RoundToInt(seconds * 14f);
            for (var h = 0; h < pops; h++)
            {
                var start = rng.Next(n);
                var len = 60 + rng.Next(160);
                var amp = (float)(0.2 + rng.NextDouble() * 0.5) * volume;
                for (var i = 0; i < len; i++)
                    data[(start + i) % n] += (float)(rng.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-i / (len * 0.3f)) * amp;
            }
            Clamp(data);
            return Build(name, data);
        }

        public static AudioClip CreateSmokeAlarmLoop(string name, float volume = 0.18f)
        {
            var n = k_SampleRate * 2;
            var data = new float[n];
            for (var b = 0; b < 3; b++)
            {
                var start = (int)(b * 0.3f * k_SampleRate);
                var len = (int)(0.18f * k_SampleRate);
                for (var i = 0; i < len; i++)
                {
                    var env = Mathf.Min(1f, i / 200f) * Mathf.Min(1f, (len - i) / 200f);
                    data[start + i] = Mathf.Sin(2f * Mathf.PI * 1800f * i / k_SampleRate) * env * volume;
                }
            }
            return Build(name, data);
        }

        public static AudioClip CreateDistantSirenLoop(string name, float volume = 0.25f)
        {
            var n = (int)(2.4f * k_SampleRate);
            var data = new float[n];
            float phase = 0f, lp = 0f;
            for (var i = 0; i < n; i++)
            {
                var t = (float)i / k_SampleRate;
                var freq = Mathf.Lerp(650f, 900f, 0.5f + 0.5f * Mathf.Sign(Mathf.Sin(t * Mathf.PI / 0.6f)));
                phase += 2f * Mathf.PI * freq / k_SampleRate;
                lp += 0.12f * (Mathf.Sin(phase) - lp);
                data[i] = lp * volume;
            }
            return Build(name, data);
        }

        static void Clamp(float[] data)
        {
            for (var i = 0; i < data.Length; i++)
                data[i] = Mathf.Clamp(data[i], -1f, 1f);
        }

        static AudioClip Build(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, k_SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
