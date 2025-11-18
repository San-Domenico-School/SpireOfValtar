namespace UI.Center
{
    public class CenterReticle : UnityEngine.MonoBehaviour
    {
        public UnityEngine.RectTransform reticle;     // UI Image RectTransform
        public UnityEngine.Camera cam;                // usually Main Camera
        public UnityEngine.LayerMask hoverMask = ~0;  // which layers count as "hovered"
        public float raycastMaxDistance = 1000f;

        public float baseSize = 18f;
        public float hoverSize = 24f;
        public UnityEngine.Color baseColor  = new UnityEngine.Color(1,1,1,0.7f);
        public UnityEngine.Color hoverColor = UnityEngine.Color.yellow;

        void Awake()
        {
            if (cam == null) cam = UnityEngine.Camera.main;
            if (reticle != null) reticle.anchoredPosition = UnityEngine.Vector2.zero;
        }

        void Update()
        {
            if (reticle == null || cam == null) return;

            // Ray from screen center (viewport 0..1)
            var ray = cam.ViewportPointToRay(new UnityEngine.Vector3(0.5f, 0.5f, 0f));

            bool hovering = UnityEngine.Physics.Raycast(ray, out _, raycastMaxDistance, hoverMask);

            // visuals
            var img = reticle.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = hovering ? hoverColor : baseColor;

            float size = hovering ? hoverSize : baseSize;
            reticle.sizeDelta = new UnityEngine.Vector2(size, size);
        }
    }
}


