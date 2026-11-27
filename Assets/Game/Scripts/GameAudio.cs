using UnityEngine;

namespace JetpackJoe
{
    // synthesised chiptune so the game has sound without shipping audio files
    public class GameAudio : MonoBehaviour
    {
        const int Rate = 44100;
        const string MuteKey = "JetpackJoe.Muted";

        AudioSource musicSource;
        AudioSource sfxSource;
        AudioClip jumpClip;
        AudioClip doubleClip;
        AudioClip heartClip;
        AudioClip hitClip;
        AudioClip deathClip;
        AudioClip startClip;
        AudioClip landClip;

        public bool Muted { get; private set; }

        enum Wave { Sine, Triangle, Square, Noise }

        public void Build()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.26f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.7f;

            jumpClip = BuildJump();
            doubleClip = BuildDoubleJump();
            heartClip = BuildHeart();
            hitClip = BuildHit();
            deathClip = BuildDeath();
            startClip = BuildStart();
            landClip = BuildLand();

            musicSource.clip = BuildMusic();
            musicSource.Play();

            Muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            AudioListener.volume = Muted ? 0f : 1f;
        }

        public void PlayJump()
        {
            sfxSource.PlayOneShot(jumpClip, 0.5f);
        }

        public void PlayDoubleJump()
        {
            sfxSource.PlayOneShot(doubleClip, 0.55f);
        }

        public void PlayHeart()
        {
            sfxSource.PlayOneShot(heartClip, 0.7f);
        }

        public void PlayHit()
        {
            sfxSource.PlayOneShot(hitClip, 0.8f);
        }

        public void PlayDeath()
        {
            sfxSource.PlayOneShot(deathClip, 0.85f);
        }

        public void PlayStart()
        {
            sfxSource.PlayOneShot(startClip, 0.65f);
        }

        public void PlayLand()
        {
            sfxSource.PlayOneShot(landClip, 0.4f);
        }

        public void SetPaused(bool paused)
        {
            if (musicSource != null) musicSource.volume = paused ? 0.1f : 0.26f;
        }

        public bool ToggleMute()
        {
            Muted = !Muted;
            AudioListener.volume = Muted ? 0f : 1f;
            PlayerPrefs.SetInt(MuteKey, Muted ? 1 : 0);
            PlayerPrefs.Save();
            return Muted;
        }

        static AudioClip Clip(string name, float[] samples)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, Rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static float Osc(Wave wave, float phase)
        {
            switch (wave)
            {
                case Wave.Sine: return Mathf.Sin(phase);
                case Wave.Triangle: return 2f / Mathf.PI * Mathf.Asin(Mathf.Sin(phase));
                case Wave.Square: return (float)System.Math.Tanh(Mathf.Sin(phase) * 3f) * 0.7f;
                default: return Random.Range(-1f, 1f);
            }
        }

        static void AddTone(float[] buffer, float start, float duration, float fromHz, float toHz, float volume,
            Wave wave, float attack = 0.008f, float release = 0.06f, float decay = 0f)
        {
            int startSample = Mathf.Max(0, Mathf.RoundToInt(start * Rate));
            int length = Mathf.RoundToInt(duration * Rate);
            float phase = 0f;
            for (int i = 0; i < length; i++)
            {
                int index = startSample + i;
                if (index >= buffer.Length) break;
                float t = i / (float)Rate;
                float u = t / duration;
                float frequency = Mathf.Lerp(fromHz, toHz, u);
                phase += 2f * Mathf.PI * frequency / Rate;
                float envelope = Mathf.Min(1f, t / Mathf.Max(0.0001f, attack)) *
                                 Mathf.Min(1f, Mathf.Max(0f, duration - t) / Mathf.Max(0.0001f, release));
                if (decay > 0f) envelope *= Mathf.Exp(-decay * t);
                buffer[index] += Osc(wave, phase) * volume * envelope;
            }
        }

        static void AddNoise(float[] buffer, float start, float duration, float volume, float decay)
        {
            int startSample = Mathf.Max(0, Mathf.RoundToInt(start * Rate));
            int length = Mathf.RoundToInt(duration * Rate);
            for (int i = 0; i < length; i++)
            {
                int index = startSample + i;
                if (index >= buffer.Length) break;
                float t = i / (float)Rate;
                float envelope = Mathf.Min(1f, t / 0.004f) * Mathf.Exp(-decay * t);
                buffer[index] += Random.Range(-1f, 1f) * volume * envelope;
            }
        }

        static AudioClip BuildJump()
        {
            var buffer = new float[Mathf.RoundToInt(0.22f * Rate)];
            AddTone(buffer, 0f, 0.16f, 330f, 720f, 0.28f, Wave.Triangle, 0.004f, 0.03f, 2f);
            AddTone(buffer, 0.01f, 0.08f, 660f, 980f, 0.08f, Wave.Square, 0.004f, 0.02f, 6f);
            return Clip("jump", buffer);
        }

        static AudioClip BuildDoubleJump()
        {
            var buffer = new float[Mathf.RoundToInt(0.28f * Rate)];
            AddTone(buffer, 0f, 0.2f, 520f, 1040f, 0.26f, Wave.Triangle, 0.004f, 0.04f, 2f);
            AddTone(buffer, 0.06f, 0.16f, 1040f, 1560f, 0.12f, Wave.Sine, 0.004f, 0.03f, 3f);
            return Clip("double_jump", buffer);
        }

        static AudioClip BuildHeart()
        {
            var buffer = new float[Mathf.RoundToInt(0.38f * Rate)];
            AddTone(buffer, 0f, 0.14f, 880f, 880f, 0.24f, Wave.Sine, 0.006f, 0.05f, 0f);
            AddTone(buffer, 0.1f, 0.22f, 1318.5f, 1318.5f, 0.22f, Wave.Sine, 0.006f, 0.08f, 2f);
            AddTone(buffer, 0.2f, 0.16f, 1760f, 1760f, 0.1f, Wave.Sine, 0.004f, 0.05f, 3f);
            return Clip("heart", buffer);
        }

        static AudioClip BuildHit()
        {
            var buffer = new float[Mathf.RoundToInt(0.3f * Rate)];
            AddNoise(buffer, 0f, 0.2f, 0.35f, 18f);
            AddTone(buffer, 0f, 0.22f, 160f, 60f, 0.32f, Wave.Square, 0.002f, 0.05f, 8f);
            return Clip("hit", buffer);
        }

        static AudioClip BuildDeath()
        {
            var buffer = new float[Mathf.RoundToInt(0.9f * Rate)];
            AddTone(buffer, 0f, 0.7f, 440f, 90f, 0.24f, Wave.Triangle, 0.005f, 0.15f, 1.2f);
            AddTone(buffer, 0.05f, 0.6f, 330f, 70f, 0.12f, Wave.Square, 0.005f, 0.15f, 1.5f);
            AddNoise(buffer, 0f, 0.4f, 0.12f, 6f);
            return Clip("death", buffer);
        }

        static AudioClip BuildStart()
        {
            var buffer = new float[Mathf.RoundToInt(0.6f * Rate)];
            AddTone(buffer, 0f, 0.16f, 329.63f, 329.63f, 0.2f, Wave.Triangle, 0.004f, 0.04f, 2f);
            AddTone(buffer, 0.1f, 0.16f, 440f, 440f, 0.2f, Wave.Triangle, 0.004f, 0.04f, 2f);
            AddTone(buffer, 0.2f, 0.34f, 659.25f, 659.25f, 0.22f, Wave.Triangle, 0.004f, 0.1f, 1.5f);
            return Clip("start", buffer);
        }

        static AudioClip BuildLand()
        {
            var buffer = new float[Mathf.RoundToInt(0.14f * Rate)];
            AddTone(buffer, 0f, 0.1f, 110f, 55f, 0.22f, Wave.Sine, 0.002f, 0.03f, 12f);
            AddNoise(buffer, 0f, 0.06f, 0.06f, 25f);
            return Clip("land", buffer);
        }

        static AudioClip BuildMusic()
        {
            const float bpm = 120f;
            const float beat = 60f / bpm;
            var buffer = new float[Mathf.RoundToInt(beat * 16f * Rate)];
            float[][] chords =
            {
                new[] { 220f, 261.63f, 329.63f, 440f },
                new[] { 174.61f, 220f, 261.63f, 349.23f },
                new[] { 261.63f, 329.63f, 392f, 523.25f },
                new[] { 196f, 246.94f, 293.66f, 392f }
            };
            int[] pattern = { 0, 1, 2, 3, 2, 1, 2, 3 };

            for (int bar = 0; bar < 4; bar++)
            {
                float barStart = bar * beat * 4f;
                var chord = chords[bar];
                for (int step = 0; step < 8; step++)
                {
                    float note = chord[pattern[step]];
                    AddTone(buffer, barStart + step * beat * 0.5f, beat * 0.48f, note, note, 0.035f, Wave.Triangle,
                        0.01f, 0.12f, 2.2f);
                }

                AddTone(buffer, barStart, beat * 3.8f, chord[0] * 0.5f, chord[0] * 0.5f, 0.055f, Wave.Sine,
                    0.02f, 0.4f, 0.2f);

                for (int kick = 0; kick < 2; kick++)
                    AddTone(buffer, barStart + kick * beat * 2f, 0.12f, 120f, 45f, 0.09f, Wave.Sine, 0.002f,
                        0.02f, 22f);

                for (int hat = 0; hat < 16; hat++)
                    AddNoise(buffer, barStart + hat * beat * 0.25f, 0.025f, 0.012f, 70f);
            }

            for (int i = 0; i < buffer.Length; i++) buffer[i] = Mathf.Clamp(buffer[i], -0.9f, 0.9f);
            return Clip("music_loop", buffer);
        }
    }
}
