using System.Collections.Generic;
using UnityEngine;

public enum Sfx
{
    Footstep,
    MeleeSwing,
    MeleeHit,
    ArrowShoot,
    ArrowHit,
    MagicCast,
    MagicHit,
    PlayerHurt,
    Heal,
    PlayerDeath,
    ActionDenied,
    EnemyRoar,
    EnemyWarning,
    EnemySlam,
    EnemyHurt,
    EnemyDeath,
    Victory
}

// Reproductor de sons del joc. No cal posar-lo a cap escena: es crea sol el primer cop.
//   GameAudio.Play(Sfx.MeleeHit);
public static class GameAudio
{
    private const int PoolSize = 10;
    private const int SampleRate = 44100;

    private static GameSoundLibrary library;
    private static bool libraryLoaded;
    private static AudioSource[] pool;
    private static int nextSource;
    private static readonly Dictionary<Sfx, AudioClip> generated = new Dictionary<Sfx, AudioClip>();
    private static readonly Dictionary<Sfx, float> lastPlayTime = new Dictionary<Sfx, float>();

    public static void Play(Sfx sound, float volume = 1f, float pitchVariation = 0.06f, float pitch = 1f)
    {
        // Evita que el mateix so soni dues vegades al mateix instant
        float now = Time.unscaledTime;
        if (lastPlayTime.TryGetValue(sound, out float last) && now - last < 0.03f) return;
        lastPlayTime[sound] = now;

        AudioClip clip = GetClip(sound);
        if (clip == null) return;

        AudioSource source = GetSource();
        if (source == null) return;

        float master = library != null ? library.masterSfxVolume : 1f;
        source.pitch = pitch * Random.Range(1f - pitchVariation, 1f + pitchVariation);
        source.volume = Mathf.Clamp01(volume * master);
        source.clip = clip;
        source.Play();
    }

    private static GameSoundLibrary Library
    {
        get
        {
            if (!libraryLoaded)
            {
                libraryLoaded = true;
                library = Resources.Load<GameSoundLibrary>("GameSoundLibrary");
            }
            return library;
        }
    }

    private static AudioSource GetSource()
    {
        if (pool == null || pool[0] == null)
        {
            GameObject go = new GameObject("GameAudio");
            Object.DontDestroyOnLoad(go);
            pool = new AudioSource[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                AudioSource s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.loop = false;
                s.spatialBlend = 0f;
                s.ignoreListenerPause = false;
                if (Library != null && Library.sfxGroup != null) s.outputAudioMixerGroup = Library.sfxGroup;
                pool[i] = s;
            }
        }

        // Busca una font lliure; si totes sonen, fa servir la seguent en ordre
        for (int i = 0; i < PoolSize; i++)
        {
            int idx = (nextSource + i) % PoolSize;
            if (!pool[idx].isPlaying)
            {
                nextSource = (idx + 1) % PoolSize;
                return pool[idx];
            }
        }
        AudioSource fallback = pool[nextSource];
        nextSource = (nextSource + 1) % PoolSize;
        return fallback;
    }

    private static AudioClip GetClip(Sfx sound)
    {
        GameSoundLibrary lib = Library;
        if (lib != null)
        {
            AudioClip c = FromLibrary(lib, sound);
            if (c != null) return c;
        }

        if (!generated.TryGetValue(sound, out AudioClip clip) || clip == null)
        {
            clip = Generate(sound);
            generated[sound] = clip;
        }
        return clip;
    }

    private static AudioClip FromLibrary(GameSoundLibrary lib, Sfx sound)
    {
        switch (sound)
        {
            case Sfx.Footstep: return lib.footstep;
            case Sfx.MeleeSwing: return lib.meleeSwing;
            case Sfx.MeleeHit: return lib.meleeHit;
            case Sfx.ArrowShoot: return lib.arrowShoot;
            case Sfx.ArrowHit: return lib.arrowHit;
            case Sfx.MagicCast: return lib.magicCast;
            case Sfx.MagicHit: return lib.magicHit;
            case Sfx.PlayerHurt: return lib.playerHurt;
            case Sfx.Heal: return lib.heal;
            case Sfx.PlayerDeath: return lib.playerDeath;
            case Sfx.ActionDenied: return lib.actionDenied;
            case Sfx.EnemyRoar: return lib.enemyRoar;
            case Sfx.EnemyWarning: return lib.enemyWarning;
            case Sfx.EnemySlam: return lib.enemySlam;
            case Sfx.EnemyHurt: return lib.enemyHurt;
            case Sfx.EnemyDeath: return lib.enemyDeath;
            case Sfx.Victory: return lib.victory;
        }
        return null;
    }

    // ---------------- Sons generats per codi (per als que no tenen fitxer) ----------------

    private static AudioClip Generate(Sfx sound)
    {
        switch (sound)
        {
            case Sfx.Footstep: return Make("Footstep", 0.12f, (t, d, r) =>
            {
                float env = Mathf.Exp(-t * 45f);
                float thump = Mathf.Sin(2f * Mathf.PI * (95f - 40f * t / d) * t);
                return (thump * 0.7f + r.Noise() * 0.35f) * env;
            }, lowPass: 0.18f);

            case Sfx.MeleeSwing: return Make("Swing", 0.24f, (t, d, r) =>
            {
                float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / d)) * Mathf.Exp(-t * 4f);
                return r.Noise() * env * 0.8f;
            }, lowPass: 0.25f);

            case Sfx.MagicCast: return Make("MagicCast", 0.4f, (t, d, r) =>
            {
                float f = Mathf.Lerp(500f, 1400f, t / d);
                float env = Mathf.Exp(-t * 6f) * Mathf.Clamp01(t * 60f);
                float trem = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 22f * t);
                return (Mathf.Sin(2f * Mathf.PI * f * t) + 0.4f * Mathf.Sin(2f * Mathf.PI * f * 1.5f * t)) * env * trem * 0.45f;
            });

            case Sfx.Heal: return Make("Heal", 0.5f, (t, d, r) =>
            {
                float f = t < 0.16f ? 660f : (t < 0.32f ? 880f : 1320f);
                float local = t < 0.16f ? t : (t < 0.32f ? t - 0.16f : t - 0.32f);
                float env = Mathf.Exp(-local * 9f) * Mathf.Clamp01(local * 200f);
                return Mathf.Sin(2f * Mathf.PI * f * t) * env * 0.4f;
            });

            case Sfx.ActionDenied: return Make("Denied", 0.22f, (t, d, r) =>
            {
                float f = t < 0.1f ? 220f : 165f;
                float sq = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * t));
                float env = (t % 0.11f) < 0.09f ? 1f : 0f;
                return sq * env * 0.18f;
            }, lowPass: 0.3f);

            case Sfx.EnemyWarning: return Make("Warning", 0.35f, (t, d, r) =>
            {
                float env = (t % 0.17f) < 0.11f ? Mathf.Exp(-(t % 0.17f) * 12f) : 0f;
                return Mathf.Sin(2f * Mathf.PI * 520f * t) * env * 0.35f;
            });

            case Sfx.PlayerDeath: return Make("Defeat", 1.2f, (t, d, r) =>
            {
                float f = Mathf.Lerp(330f, 90f, t / d);
                float env = Mathf.Exp(-t * 1.8f);
                return (Mathf.Sin(2f * Mathf.PI * f * t) * 0.6f + Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * 0.5f * t)) * 0.15f) * env * 0.5f;
            }, lowPass: 0.2f);

            case Sfx.Victory: return Make("Victory", 1.0f, (t, d, r) =>
            {
                float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
                int i = Mathf.Min((int)(t / 0.14f), notes.Length - 1);
                float local = t - i * 0.14f;
                float env = Mathf.Exp(-local * (i == notes.Length - 1 ? 3f : 10f)) * Mathf.Clamp01(local * 200f);
                return (Mathf.Sin(2f * Mathf.PI * notes[i] * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * notes[i] * t)) * env * 0.35f;
            });

            case Sfx.EnemySlam: return Make("Slam", 0.5f, (t, d, r) =>
            {
                float env = Mathf.Exp(-t * 9f);
                return (Mathf.Sin(2f * Mathf.PI * (70f - 30f * t) * t) * 0.8f + r.Noise() * 0.5f) * env;
            }, lowPass: 0.12f);

            case Sfx.EnemyRoar: return Make("Roar", 0.8f, (t, d, r) =>
            {
                float env = Mathf.Sin(Mathf.PI * t / d);
                float f = 85f + 25f * Mathf.Sin(2f * Mathf.PI * 3f * t);
                return (Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * t)) * 0.4f + r.Noise() * 0.4f) * env * 0.6f;
            }, lowPass: 0.1f);

            case Sfx.EnemyDeath: return Make("EnemyDeath", 1.2f, (t, d, r) =>
            {
                float env = Mathf.Exp(-t * 2f);
                float f = Mathf.Lerp(120f, 40f, t / d);
                return (Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * t)) * 0.4f + r.Noise() * 0.5f) * env * 0.6f;
            }, lowPass: 0.08f);

            case Sfx.PlayerHurt:
            case Sfx.EnemyHurt:
            case Sfx.MeleeHit:
            case Sfx.ArrowHit:
            case Sfx.MagicHit:
            case Sfx.ArrowShoot:
            default:
                return Make("Hit", 0.18f, (t, d, r) =>
                {
                    float env = Mathf.Exp(-t * 28f);
                    return (r.Noise() * 0.6f + Mathf.Sin(2f * Mathf.PI * 150f * t) * 0.6f) * env;
                }, lowPass: 0.35f);
        }
    }

    private class Rng
    {
        private uint state = 0x12345u;
        public float Noise()
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            return (state / (float)uint.MaxValue) * 2f - 1f;
        }
    }

    private delegate float SampleFn(float t, float duration, Rng rng);

    private static AudioClip Make(string name, float duration, SampleFn fn, float lowPass = 1f)
    {
        int count = Mathf.CeilToInt(duration * SampleRate);
        float[] data = new float[count];
        Rng rng = new Rng();
        float prev = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)SampleRate;
            float v = fn(t, duration, rng);
            prev += lowPass * (v - prev);   // filtre passa-baixos simple
            data[i] = Mathf.Clamp(prev, -1f, 1f);
        }

        // Fade final per evitar clics
        int fade = Mathf.Min(count, 400);
        for (int i = 0; i < fade; i++) data[count - 1 - i] *= i / (float)fade;

        AudioClip clip = AudioClip.Create("SFX_" + name, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
