using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Generic enemy audio controller:
/// - Plays a looping movement sound while the NavMeshAgent is moving.
/// - Plays a one-shot attack sound when PlayAttackSfx() is called by enemy logic.
/// </summary>
[DisallowMultipleComponent]
public class EnemySoundController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Loop clip to play while moving (e.g., slime squish loop, wizard robe swish loop).")]
    [SerializeField] private AudioClip moveLoopClip;
    [SerializeField, Range(0f, 1f)] private float moveVolume = 0.7f;
    [Tooltip("Speed magnitude above this counts as moving.")]
    [SerializeField] private float moveSpeedThreshold = 0.15f;
    [Tooltip("If enabled, movement loop pitch scales slightly with speed.")]
    [SerializeField] private bool scaleMovePitchWithSpeed = false;
    [SerializeField] private Vector2 movePitchRange = new Vector2(0.95f, 1.10f);

    [Header("Attack")]
    [Tooltip("One-shot clips played when the enemy attacks.")]
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField, Range(0f, 1f)] private float attackVolume = 0.9f;
    [SerializeField] private Vector2 attackPitchRange = new Vector2(0.97f, 1.03f);

    [Header("3D Settings")]
    [Tooltip("If false, audio is forced 2D (useful for debugging or arcade-style sound).")]
    [SerializeField] private bool playIn3D = true;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 35f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

    [Header("Refs (optional)")]
    [Tooltip("If empty, will auto-find on this GameObject.")]
    [SerializeField] private NavMeshAgent navAgent;
    [Tooltip("If empty, will be created for movement.")]
    [SerializeField] private AudioSource moveSource;
    [Tooltip("If empty, will be created for attacks.")]
    [SerializeField] private AudioSource attackSource;

    [Header("Debug (optional)")]
    [SerializeField] private bool debugWarnings = false;
    private float nextWarnTime = 0f;

    private void Awake()
    {
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null) navAgent = GetComponentInParent<NavMeshAgent>();
        if (navAgent == null) navAgent = GetComponentInChildren<NavMeshAgent>();

        if (moveSource == null)
        {
            moveSource = gameObject.AddComponent<AudioSource>();
            moveSource.playOnAwake = false;
        }

        moveSource.loop = true;
        moveSource.clip = moveLoopClip;
        moveSource.volume = moveVolume;
        ConfigureSpatial(moveSource);

        if (attackSource == null)
        {
            attackSource = gameObject.AddComponent<AudioSource>();
            attackSource.playOnAwake = false;
        }

        attackSource.loop = false;
        attackSource.volume = 1f;
        ConfigureSpatial(attackSource);
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            StopMoveLoop();
            return;
        }

        if (debugWarnings && Time.time >= nextWarnTime)
        {
            if (FindFirstObjectByType<AudioListener>() == null)
                Debug.LogWarning("[EnemySoundController] No AudioListener found in scene. Add one to your main camera.", this);
            if (AudioListener.volume <= 0.0001f)
                Debug.LogWarning("[EnemySoundController] AudioListener.volume is 0 (global mute). Check your master volume.", this);
            nextWarnTime = Time.time + 2f;
        }

        if (moveLoopClip == null || navAgent == null)
        {
            StopMoveLoop();
            return;
        }

        float speed = navAgent.velocity.magnitude;
        bool shouldMove = speed >= moveSpeedThreshold;

        if (!shouldMove)
        {
            StopMoveLoop();
            return;
        }

        moveSource.clip = moveLoopClip;
        moveSource.volume = moveVolume;

        if (scaleMovePitchWithSpeed)
        {
            float t = Mathf.InverseLerp(moveSpeedThreshold, Mathf.Max(moveSpeedThreshold + 0.01f, navAgent.speed), speed);
            moveSource.pitch = Mathf.Lerp(
                Mathf.Min(movePitchRange.x, movePitchRange.y),
                Mathf.Max(movePitchRange.x, movePitchRange.y),
                t
            );
        }
        else
        {
            moveSource.pitch = 1f;
        }

        if (!moveSource.isPlaying) moveSource.Play();
    }

    public void PlayAttackSfx()
    {
        if (Time.timeScale == 0f) return;
        if (attackSource == null) return;
        AudioClip clip = PickRandom(attackClips);
        if (clip == null) return;

        attackSource.pitch = PickPitch(attackPitchRange);
        attackSource.PlayOneShot(clip, attackVolume);
    }

    private void StopMoveLoop()
    {
        if (moveSource != null && moveSource.isPlaying) moveSource.Stop();
    }

    private void ConfigureSpatial(AudioSource src)
    {
        src.spatialBlend = playIn3D ? 1f : 0f;
        if (playIn3D)
        {
            src.rolloffMode = rolloffMode;
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
        }
        src.dopplerLevel = 0f;
    }

    [ContextMenu("EnemyAudio/Test: Play Attack SFX")]
    private void TestPlayAttack() => PlayAttackSfx();

    [ContextMenu("EnemyAudio/Test: Log State")]
    private void TestLogState()
    {
        float speed = navAgent != null ? navAgent.velocity.magnitude : -1f;
        Debug.Log(
            $"[EnemySoundController] playIn3D={playIn3D}, AudioListener.volume={AudioListener.volume}, " +
            $"hasAudioListener={(FindFirstObjectByType<AudioListener>() != null)}, " +
            $"navAgent={(navAgent != null ? "OK" : "NULL")}, navSpeed={speed:F3}, " +
            $"moveClip={(moveLoopClip != null ? moveLoopClip.name : "NULL")}, " +
            $"attackClips={(attackClips != null ? attackClips.Length : 0)}",
            this
        );
    }

    private static AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    private static float PickPitch(Vector2 range)
    {
        float min = Mathf.Min(range.x, range.y);
        float max = Mathf.Max(range.x, range.y);
        if (Mathf.Abs(max - min) < 0.0001f) return min;
        return Random.Range(min, max);
    }
}

