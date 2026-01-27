using System;
using UnityEngine;

public enum SpellSfxId
{
    Lightning = 0,
    Fireball = 1,
    Freeze = 2,
}

[DisallowMultipleComponent]
public class SpellAudioController : MonoBehaviour
{
    [Serializable]
    public class SpellSfxSet
    {
        public SpellSfxId spell = SpellSfxId.Lightning;

        [Header("Cast (when spell is fired)")]
        public AudioClip[] castClips;
        [Range(0f, 1f)] public float castVolume = 0.9f;

        [Header("Hit (when spell hits an enemy)")]
        public AudioClip[] hitClips;
        [Range(0f, 1f)] public float hitVolume = 0.95f;

        [Header("Variation")]
        public Vector2 pitchRange = new Vector2(0.97f, 1.03f);
    }

    [Header("Per-spell clips")]
    [SerializeField] private SpellSfxSet[] sfxSets;

    [Header("Playback")]
    [Tooltip("Casts are always played 2D. Hits can be played as 3D at the impact point.")]
    [SerializeField] private bool hitsAre3D = true;
    [SerializeField] private float hitMinDistance = 2f;
    [SerializeField] private float hitMaxDistance = 60f;
    [SerializeField] private AudioRolloffMode hitRolloffMode = AudioRolloffMode.Linear;

    [Header("Debug (optional)")]
    [Tooltip("Logs warnings when PlayCast/PlayHit is called but no clips are configured, or when global audio is muted.")]
    [SerializeField] private bool debugWarnings = false;
    private float nextDebugWarnTime = 0f;

    [Header("Audio Sources (optional)")]
    [Tooltip("2D source used for casting SFX. If empty, one will be created.")]
    [SerializeField] private AudioSource castSource;

    private void Awake()
    {
        if (castSource == null)
        {
            castSource = gameObject.AddComponent<AudioSource>();
            castSource.playOnAwake = false;
        }

        castSource.spatialBlend = 0f; // 2D
        castSource.dopplerLevel = 0f;
    }

    [ContextMenu("SpellAudio/Test: Play Cast (Lightning)")]
    private void TestPlayCastLightning() => PlayCast(SpellSfxId.Lightning);

    [ContextMenu("SpellAudio/Test: Play Cast (Fireball)")]
    private void TestPlayCastFireball() => PlayCast(SpellSfxId.Fireball);

    [ContextMenu("SpellAudio/Test: Play Cast (Freeze)")]
    private void TestPlayCastFreeze() => PlayCast(SpellSfxId.Freeze);

    [ContextMenu("SpellAudio/Test: Log Audio State")]
    private void TestLogAudioState()
    {
        Debug.Log(
            $"[SpellAudioController] AudioListener.volume={AudioListener.volume}, timeScale={Time.timeScale}, " +
            $"hasAudioListener={(FindFirstObjectByType<AudioListener>() != null)}, " +
            $"castSource={(castSource != null ? "OK" : "NULL")}",
            this
        );
    }

    public void PlayCast(SpellSfxId spell)
    {
        if (Time.timeScale == 0f) return;

        SpellSfxSet set = FindSet(spell);
        if (set == null)
        {
            Warn($"No SFX set configured for {spell}. Add it to 'Sfx Sets' on SpellAudioController.");
            return;
        }

        AudioClip clip = PickRandom(set.castClips);
        if (clip == null)
        {
            Warn($"{spell} cast requested, but 'Cast Clips' is empty.");
            return;
        }

        float pitch = PickPitch(set.pitchRange);
        castSource.pitch = pitch;
        castSource.PlayOneShot(clip, set.castVolume);
    }

    public void PlayHit(SpellSfxId spell, Vector3 worldPoint)
    {
        if (Time.timeScale == 0f) return;

        SpellSfxSet set = FindSet(spell);
        if (set == null)
        {
            Warn($"No SFX set configured for {spell}. Add it to 'Sfx Sets' on SpellAudioController.");
            return;
        }

        AudioClip clip = PickRandom(set.hitClips);
        // Hit clips are intentionally optional: if empty, just don't play hit SFX.
        if (clip == null) return;

        float pitch = PickPitch(set.pitchRange);

        if (hitsAre3D)
            PlayOneShot3D(clip, worldPoint, set.hitVolume, pitch);
        else
            castSource.PlayOneShot(clip, set.hitVolume);
    }

    private SpellSfxSet FindSet(SpellSfxId spell)
    {
        if (sfxSets == null) return null;
        for (int i = 0; i < sfxSets.Length; i++)
        {
            if (sfxSets[i] != null && sfxSets[i].spell == spell)
                return sfxSets[i];
        }
        return null;
    }

    private static AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    private static float PickPitch(Vector2 pitchRange)
    {
        float min = Mathf.Min(pitchRange.x, pitchRange.y);
        float max = Mathf.Max(pitchRange.x, pitchRange.y);
        if (Mathf.Abs(max - min) < 0.0001f) return min;
        return UnityEngine.Random.Range(min, max);
    }

    private void PlayOneShot3D(AudioClip clip, Vector3 pos, float volume, float pitch)
    {
        // One-shot emitter object (self-destroys when finished).
        GameObject go = new GameObject($"SpellHitSFX_{clip.name}");
        go.transform.position = pos;

        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 1f; // 3D
        src.rolloffMode = hitRolloffMode;
        src.minDistance = hitMinDistance;
        src.maxDistance = hitMaxDistance;
        src.dopplerLevel = 0f;
        src.pitch = pitch;
        src.volume = Mathf.Clamp01(volume);
        src.loop = false;
        src.clip = clip;
        src.Play();

        OneShotAutoDestroy auto = go.AddComponent<OneShotAutoDestroy>();
        auto.Init(src, debugWarnings);
    }

    private void Warn(string msg)
    {
        if (!debugWarnings) return;
        if (Time.time < nextDebugWarnTime) return;

        if (FindFirstObjectByType<AudioListener>() == null)
            Debug.LogWarning("[SpellAudioController] No AudioListener found in scene. Add one to your main camera.");
        if (AudioListener.volume <= 0.0001f)
            Debug.LogWarning("[SpellAudioController] AudioListener.volume is 0 (global mute). Check your SettingsManager / volume slider.");

        Debug.LogWarning($"[SpellAudioController] {msg}", this);
        nextDebugWarnTime = Time.time + 1.5f;
    }

    private sealed class OneShotAutoDestroy : MonoBehaviour
    {
        private AudioSource src;
        private bool debug;
        private float startTime;

        public void Init(AudioSource source, bool debugWarningsEnabled)
        {
            src = source;
            debug = debugWarningsEnabled;
            startTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (src == null)
            {
                Destroy(gameObject);
                return;
            }

            // If clip isn't playing anymore, we're done.
            if (!src.isPlaying)
            {
                Destroy(gameObject);
                return;
            }

            // Safety: if something goes wrong (e.g., streaming), don't leak.
            float expected = (src.clip != null) ? (src.clip.length / Mathf.Max(0.01f, src.pitch)) : 2f;
            if (Time.unscaledTime - startTime > expected + 1.0f)
            {
                if (debug) Debug.LogWarning("[SpellAudioController] Hit emitter exceeded expected lifetime; destroying.", this);
                Destroy(gameObject);
            }
        }
    }
}

