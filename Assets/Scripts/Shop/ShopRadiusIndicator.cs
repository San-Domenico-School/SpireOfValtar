// v1 Gleb created
// Draws a spinning dashed circle on the ground around the shop using a LineRenderer.
// Uses the same gold colour as the UI (236, 165, 41). Attach to the shop cube GameObject.
// The circle appears only while the player is NOT inside the radius (fades out when close
// enough to open the shop, so it doesn't clutter the interaction prompt).

using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShopRadiusIndicator : MonoBehaviour
{
    [Header("Circle Settings")]
    [SerializeField] private float radius         = 5f;   // Should match SpellShopTrigger.interactionRadius
    [SerializeField] private int   totalSegments  = 64;   // Total points around the circle
    [SerializeField] private int   dashCount      = 16;   // How many dashes (gaps between them)
    [SerializeField] private float spinSpeed      = 30f;  // Degrees per second
    [SerializeField] private float groundOffset   = 0.05f;// Tiny lift so it doesn't z-fight the floor

    [Header("Visual")]
    [SerializeField] private float lineWidth = 0.06f;
    // Gold colour matching UI: rgb(236, 165, 41)
    private static readonly Color GoldColour = new Color(236f / 255f, 165f / 255f, 41f / 255f, 1f);

    private LineRenderer lr;
    private float        currentAngle = 0f;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lr.loop             = false;
        lr.useWorldSpace    = true;
        lr.widthMultiplier  = lineWidth;
        lr.startColor       = GoldColour;
        lr.endColor         = GoldColour;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows   = false;

        // Use the built-in default unlit line material so colour shows correctly
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = GoldColour;

        // Sorting so it draws on top of the floor
        lr.sortingOrder = 1;
    }

    private void Update()
    {
        currentAngle += spinSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        DrawDashedCircle();
    }

    private void DrawDashedCircle()
    {
        // Each "dash" occupies half a slot; the other half is a gap.
        // We build a list of line segments (pairs of points) rather than one
        // continuous line, so we set positionCount to dashCount * 2.

        float segmentAngle  = 360f / totalSegments;           // degrees per segment
        float segmentsPerDash = (float)totalSegments / dashCount; // segments in one dash+gap
        float dashSegments  = segmentsPerDash * 0.5f;         // half slot = dash

        // Collect all points (start + end of every dash)
        int   pointCount = dashCount * 2;
        lr.positionCount = pointCount;

        // Ground Y: use the shop object's position but drop to floor level via a downward raycast
        float groundY = GetGroundY();

        for (int d = 0; d < dashCount; d++)
        {
            float startDeg = currentAngle + d * segmentsPerDash * segmentAngle;
            float endDeg   = startDeg + dashSegments * segmentAngle;

            lr.SetPosition(d * 2,     AngleToPoint(startDeg, groundY));
            lr.SetPosition(d * 2 + 1, AngleToPoint(endDeg,   groundY));
        }
    }

    private Vector3 AngleToPoint(float degrees, float groundY)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float x   = transform.position.x + Mathf.Cos(rad) * radius;
        float z   = transform.position.z + Mathf.Sin(rad) * radius;
        return new Vector3(x, groundY + groundOffset, z);
    }

    private float GetGroundY()
    {
        // Raycast straight down from the shop object to find the floor
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
        {
            return hit.point.y;
        }
        // Fallback: use the shop's own Y
        return transform.position.y;
    }

    // Show / hide the indicator from SpellShopTrigger
    public void SetVisible(bool visible)
    {
        lr.enabled = visible;
    }
}
