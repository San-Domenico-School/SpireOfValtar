// v1 Gleb created
// Draws a spinning dashed circle on the ground around the shop using one LineRenderer per dash.
// Uses the same gold colour as the UI (236, 165, 41). Attach to the shop cube GameObject.
// The circle appears only while the player is NOT inside the radius.

using System.Collections.Generic;
using UnityEngine;

public class ShopRadiusIndicator : MonoBehaviour
{
    [Header("Circle Settings")]
    [SerializeField] private float radius        = 5f;   // Should match SpellShopTrigger.interactionRadius
    [SerializeField] private int   dashCount     = 8;    // Number of dashes around the circle
    [SerializeField] [Range(0.05f, 0.49f)]
                     private float dashFraction  = 0.35f;// How much of each slot is dash (rest is gap)
    [SerializeField] private int   pointsPerDash = 12;   // Smoothness of each dash arc
    [SerializeField] private float spinSpeed     = 40f;  // Degrees per second
    [SerializeField] private float groundOffset  = 0.05f;// Tiny lift so it doesn't z-fight the floor

    [Header("Visual")]
    [SerializeField] private float lineWidth = 0.08f;

    // Gold colour matching UI: rgb(236, 165, 41)
    private static readonly Color GoldColour = new Color(236f / 255f, 165f / 255f, 41f / 255f, 1f);

    private readonly List<LineRenderer> dashRenderers = new List<LineRenderer>();
    private float currentAngle = 0f;
    private bool  isVisible    = true;

    private void Awake()
    {
        BuildDashRenderers();
    }

    private void BuildDashRenderers()
    {
        // Destroy any old ones first
        foreach (var old in dashRenderers)
            if (old != null) Destroy(old.gameObject);
        dashRenderers.Clear();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = GoldColour;

        for (int i = 0; i < dashCount; i++)
        {
            GameObject go = new GameObject($"Dash_{i}");
            go.transform.SetParent(transform, false);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace   = true;
            lr.loop            = false;
            lr.widthMultiplier = lineWidth;
            lr.startColor      = GoldColour;
            lr.endColor        = GoldColour;
            lr.material        = mat;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows  = false;
            lr.sortingOrder    = 1;
            lr.positionCount   = pointsPerDash;

            dashRenderers.Add(lr);
        }
    }

    private void Update()
    {
        currentAngle += spinSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        float groundY        = GetGroundY();
        float slotDeg        = 360f / dashCount;          // degrees per slot (dash + gap)
        float dashDeg        = slotDeg * dashFraction;    // degrees covered by the dash arc

        for (int i = 0; i < dashRenderers.Count; i++)
        {
            float startDeg = currentAngle + i * slotDeg;
            float endDeg   = startDeg + dashDeg;

            LineRenderer lr = dashRenderers[i];
            for (int p = 0; p < pointsPerDash; p++)
            {
                float t   = (float)p / (pointsPerDash - 1);
                float deg = Mathf.Lerp(startDeg, endDeg, t);
                lr.SetPosition(p, AngleToPoint(deg, groundY));
            }
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
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
            return hit.point.y;
        return transform.position.y;
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;
        foreach (var lr in dashRenderers)
            if (lr != null) lr.enabled = visible;
    }
}
