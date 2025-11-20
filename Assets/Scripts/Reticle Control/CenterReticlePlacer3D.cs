using UnityEngine;

// File: Assets/Scripts/UI/CenterReticlePlacer3D.cs
public class CenterReticlePlacer3D : UnityEngine.MonoBehaviour
{
    public UnityEngine.Transform reticle;          // your 3D reticle object (can be this.transform)
    public UnityEngine.Camera cam;                 // usually Main Camera

    // Placement
    public bool stickToSurface = true;             // ON: raycast from screen center onto world
    public UnityEngine.LayerMask surfaceMask = ~0; // layers considered "world" (exclude reticle's layer)
    public float fixedDistance = 3f;               // used when stickToSurface = false or when ray misses
    public float surfaceOffset = 0.01f;            // lift off surface to avoid z-fighting

    // Facing camera
    public bool rotateToFaceCamera = true;         // billboard
    public bool yawOnly = false;                   // keep flat on ground (rotate around Y only)
    public UnityEngine.Vector3 rotationOffsetEuler; // use (0,180,0) if mesh faces backwards

    // Visual scale
    public bool scaleWithDistance = true;          // keeps on-screen size stable-ish
    public float sizeAt1m = 0.05f;                 // meters (local scale multiplier at 1m)
    UnityEngine.Vector3 _baseScale;

    void Awake()
    {
        if (!cam) cam = UnityEngine.Camera.main;
        if (!reticle) reticle = transform;
        _baseScale = reticle.localScale;

        // Avoid self-raycast problems
        var col = reticle.GetComponent<UnityEngine.Collider>();
        if (col) col.enabled = false;
    }

    void LateUpdate()
    {
        if (!cam || !reticle) return;

        // 1) Decide target position along the center ray
        UnityEngine.Ray ray = cam.ViewportPointToRay(new UnityEngine.Vector3(0.5f, 0.5f, 0f));
        UnityEngine.Vector3 pos;

        if (stickToSurface && UnityEngine.Physics.Raycast(ray, out var hit, 5000f, surfaceMask))
        {
            pos = hit.point + hit.normal * surfaceOffset;
        }
        else
        {
            pos = cam.transform.position + cam.transform.forward * fixedDistance;
        }

        reticle.position = pos;

        // 2) Face the camera (billboard)
        if (rotateToFaceCamera)
        {
            UnityEngine.Quaternion rot;
            if (yawOnly)
            {
                UnityEngine.Vector3 dir = (cam.transform.position - pos);
                dir.y = 0f;
                if (dir.sqrMagnitude > 1e-6f)
                    rot = UnityEngine.Quaternion.LookRotation(dir.normalized, UnityEngine.Vector3.up);
                else
                    rot = reticle.rotation;
            }
            else
            {
                rot = UnityEngine.Quaternion.LookRotation(
                    (cam.transform.position - pos).normalized,
                    cam.transform.up
                );
            }
            reticle.rotation = rot * UnityEngine.Quaternion.Euler(rotationOffsetEuler);
        }

        // 3) Scale with distance (optional)
        float d = UnityEngine.Vector3.Distance(cam.transform.position, pos);
        float k = scaleWithDistance ? (d * sizeAt1m) : 1f;
        reticle.localScale = _baseScale * k;
    }
}
