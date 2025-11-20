namespace UI.MouseFollow
{
    public class ReticleController : UnityEngine.MonoBehaviour
    {
        public UnityEngine.RectTransform reticle;   // UI Image RectTransform
        public UnityEngine.Camera cam;              // usually Main Camera
        public UnityEngine.LayerMask hoverMask = ~0;// which layers count as "hovering" (default: everything)

        public bool hideSystemCursor = true;
        public float followSmoothing = 1f;          // 0 = instant, higher = snappier
        public float baseSize = 20f;
        public float hoverSize = 28f;
        public UnityEngine.Color baseColor  = new UnityEngine.Color(1,1,1,0.85f);
        public UnityEngine.Color hoverColor = UnityEngine.Color.yellow;

        public float raycastMaxDistance = 1000f;

        UnityEngine.Vector2 _uiPos;

        void Awake()
        {
            if (cam == null) cam = UnityEngine.Camera.main;
            if (hideSystemCursor) UnityEngine.Cursor.visible = false;
            if (reticle != null) reticle.sizeDelta = new UnityEngine.Vector2(baseSize, baseSize);
        }

        void OnDisable()
        {
            if (hideSystemCursor) UnityEngine.Cursor.visible = true;
        }

        void Update()
        {
            if (reticle == null || cam == null) return;

            // follow mouse
            UnityEngine.Vector2 targetPos = UnityEngine.Input.mousePosition;
            _uiPos = UnityEngine.Vector2.Lerp(
                _uiPos,
                targetPos,
                1f - UnityEngine.Mathf.Exp(-followSmoothing * UnityEngine.Time.unscaledDeltaTime)
            );
            reticle.position = _uiPos;

            // hover check (raycast from camera through cursor)
            bool hovering = false;
            var ray = cam.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (UnityEngine.Physics.Raycast(ray, out var _, raycastMaxDistance, hoverMask))
                hovering = true;

            // visuals
            var img = reticle.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = hovering ? hoverColor : baseColor;

            float size = hovering ? hoverSize : baseSize;
            reticle.sizeDelta = new UnityEngine.Vector2(size, size);
        }
    }
}
