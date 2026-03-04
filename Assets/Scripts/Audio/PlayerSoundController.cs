using UnityEngine;

/// <summary>
/// Player SFX controller:
/// - Plays footsteps while the player is moving on the ground (stops while airborne/jumping).
/// - Plays a sound each time a spell is cast (call PlaySpellCastSfx from your ability logic).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class PlayerSoundController : MonoBehaviour
{
    [Header("Footsteps")]
    [Tooltip("If assigned, this clip will loop while the player is moving on the ground.")]
    [SerializeField] private AudioClip footstepLoopClip;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.8f;
    [Tooltip("Minimum planar speed required to play footsteps.")]
    [SerializeField] private float minPlanarSpeed = 0.1f;
    [Tooltip("If planar speed is above this, treat as 'running' and use runStepInterval (when not using a loop clip).")]
    [SerializeField] private float runSpeedThreshold = 6.5f;
    [Tooltip("Seconds between footstep one-shots while walking (only used if Footstep Loop Clip is empty).")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [Tooltip("Seconds between footstep one-shots while running (only used if Footstep Loop Clip is empty).")]
    [SerializeField] private float runStepInterval = 0.35f;
    [Tooltip("If enabled, one-shot footsteps trigger by distance traveled instead of a time interval (more consistent). Only used if Footstep Loop Clip is empty.")]
    [SerializeField] private bool useDistanceBasedOneShots = true;
    [Tooltip("Meters/units traveled per footstep while walking (only used when distance-based one-shots are enabled and Footstep Loop Clip is empty).")]
    [SerializeField] private float walkStepDistance = 1.4f;
    [Tooltip("Meters/units traveled per footstep while running (only used when distance-based one-shots are enabled and Footstep Loop Clip is empty).")]
    [SerializeField] private float runStepDistance = 1.9f;
    [Tooltip("Prevents footsteps from rapidly stopping/starting when grounding flickers on slopes/steps.")]
    [SerializeField] private float groundedGraceSeconds = 0.08f;
    [Tooltip("If enabled, loop footsteps pitch scales with speed. Disable if it sounds 'wobbly'.")]
    [SerializeField] private bool scaleLoopPitchWithSpeed = false;
    [Tooltip("Optional one-shot footstep clips (used if Footstep Loop Clip is empty).")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Spell Cast")]
    [Tooltip("One-shot clip played whenever a spell is cast.")]
    [SerializeField] private AudioClip spellCastClip;
    [SerializeField, Range(0f, 1f)] private float spellCastVolume = 0.9f;

    [Header("Audio Sources (optional)")]
    [Tooltip("If left empty, an AudioSource will be created for footsteps.")]
    [SerializeField] private AudioSource footstepsSource;
    [Tooltip("If left empty, an AudioSource will be created for spell casts.")]
    [SerializeField] private AudioSource spellSource;

    private CharacterController controller;
    private float nextFootstepTime;
    private Vector3 lastPosition;
    private float stepDistanceAccumulator;
    private float lastGroundedTime;

    [Header("Debug (optional)")]
    [Tooltip("If enabled, prints warnings for common reasons audio may be silent (global volume, missing AudioListener, etc).")]
    [SerializeField] private bool debugWarnings = false;
    private float nextDebugWarningTime = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        lastPosition = transform.position;
        lastGroundedTime = Time.time;

        if (footstepsSource == null)
        {
            footstepsSource = gameObject.AddComponent<AudioSource>();
            footstepsSource.playOnAwake = false;
        }
        footstepsSource.loop = true;
        footstepsSource.volume = footstepVolume;
        footstepsSource.spatialBlend = 0f; // force 2D so distance doesn't matter
        footstepsSource.dopplerLevel = 0f;
        if (footstepLoopClip != null) footstepsSource.clip = footstepLoopClip;

        if (spellSource == null)
        {
            spellSource = gameObject.AddComponent<AudioSource>();
            spellSource.playOnAwake = false;
        }
        spellSource.loop = false;
        spellSource.volume = spellCastVolume;
        spellSource.spatialBlend = 0f; // force 2D
        spellSource.dopplerLevel = 0f;
    }

    private void Update()
    {
        // Don't play SFX while paused.
        if (Time.timeScale == 0f)
        {
            StopFootsteps();
            return;
        }

        if (debugWarnings && Time.time >= nextDebugWarningTime)
        {
            if (FindFirstObjectByType<AudioListener>() == null)
                Debug.LogWarning("[PlayerSoundController] No AudioListener found in scene. Add one to your main camera.");
            if (AudioListener.volume <= 0.0001f)
                Debug.LogWarning("[PlayerSoundController] AudioListener.volume is 0 (global mute). Check your SettingsManager / volume slider.");
            nextDebugWarningTime = Time.time + 2f;
        }

        bool groundedNow = controller != null && controller.isGrounded;
        if (groundedNow) lastGroundedTime = Time.time;
        bool grounded = groundedNow || (Time.time - lastGroundedTime <= groundedGraceSeconds);
        Vector3 v = controller != null ? controller.velocity : Vector3.zero;
        float planarSpeed = new Vector3(v.x, 0f, v.z).magnitude;

        // Fallback: CharacterController.velocity can be unreliable depending on how movement is applied.
        // Use position delta to detect planar motion.
        Vector3 delta = transform.position - lastPosition;
        float dt = Mathf.Max(0.0001f, Time.deltaTime);
        float planarSpeedFromDelta = new Vector3(delta.x, 0f, delta.z).magnitude / dt;
        planarSpeed = Mathf.Max(planarSpeed, planarSpeedFromDelta);
        lastPosition = transform.position;

        bool shouldFootstep = grounded && planarSpeed >= minPlanarSpeed;

        if (!shouldFootstep)
        {
            StopFootsteps();
            return;
        }

        // Prefer loop clip if provided.
        if (footstepLoopClip != null)
        {
            if (footstepsSource.clip != footstepLoopClip) footstepsSource.clip = footstepLoopClip;
            footstepsSource.volume = footstepVolume;
            footstepsSource.mute = false;

            if (scaleLoopPitchWithSpeed)
            {
                // Slight pitch scaling by speed helps "walk vs run" feel without extra clips.
                float t = Mathf.InverseLerp(minPlanarSpeed, runSpeedThreshold, planarSpeed);
                footstepsSource.pitch = Mathf.Lerp(0.95f, 1.15f, t);
            }
            else
            {
                footstepsSource.pitch = 1f;
            }

            if (!footstepsSource.isPlaying) footstepsSource.Play();
            return;
        }

        // Otherwise, play one-shot footsteps.
        if (useDistanceBasedOneShots)
        {
            stepDistanceAccumulator += new Vector3(delta.x, 0f, delta.z).magnitude;
            float stepDistance = (planarSpeed >= runSpeedThreshold) ? runStepDistance : walkStepDistance;
            stepDistance = Mathf.Max(0.1f, stepDistance);

            // Limit how often we can trigger (prevents bursts if you teleport or get a big delta).
            float minInterval = (planarSpeed >= runSpeedThreshold) ? runStepInterval : walkStepInterval;
            minInterval = Mathf.Max(0.05f, minInterval);

            if (stepDistanceAccumulator >= stepDistance && Time.time >= nextFootstepTime)
            {
                AudioClip clip = PickRandom(footstepClips);
                if (clip != null)
                {
                    footstepsSource.pitch = Random.Range(0.97f, 1.03f);
                    footstepsSource.mute = false;
                    footstepsSource.PlayOneShot(clip, footstepVolume);
                }

                // Keep remainder distance so cadence stays stable.
                stepDistanceAccumulator %= stepDistance;
                nextFootstepTime = Time.time + minInterval;
            }
        }
        else
        {
            // Time-based fallback
            float interval = (planarSpeed >= runSpeedThreshold) ? runStepInterval : walkStepInterval;
            if (Time.time >= nextFootstepTime)
            {
                AudioClip clip = PickRandom(footstepClips);
                if (clip != null)
                {
                    footstepsSource.pitch = Random.Range(0.97f, 1.03f);
                    footstepsSource.mute = false;
                    footstepsSource.PlayOneShot(clip, footstepVolume);
                }
                nextFootstepTime = Time.time + Mathf.Max(0.05f, interval);
            }
        }
    }

    private static AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    private void StopFootsteps()
    {
        nextFootstepTime = 0f;
        stepDistanceAccumulator = 0f;
        if (footstepsSource != null && footstepsSource.isPlaying) footstepsSource.Stop();
    }

    /// <summary>
    /// Call this whenever the player successfully casts a spell.
    /// </summary>
    public void PlaySpellCastSfx()
    {
        if (Time.timeScale == 0f) return;
        if (spellSource == null) return;
        if (spellCastClip == null) return;

        spellSource.PlayOneShot(spellCastClip, spellCastVolume);
    }
}

