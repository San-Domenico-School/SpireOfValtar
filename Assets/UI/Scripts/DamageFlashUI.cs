using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

/************************************
 * Creates a red flash effect on screen edges when player takes damage.
 * Uses UI Toolkit to display an overlay that fades out over time.
 * Gleb 01/09/26
 * Version 1.0
 * 
 * Add this script to the same GameObject as the UIDocument.
 * Assign the UIDocument in the Inspector.
 * Assign the PlayerHealth in the Inspector.
 * The script will automatically find the PlayerHealth component in the scene.
 * The script will automatically find the UIDocument component in the scene.
 ************************************/
public class DamageFlashUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerHealth playerHealth;
    
    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 1f;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.5f); // Red with transparency
    [SerializeField] private int edgeThickness = 100; // Distance from edge where gradient starts fading
    [SerializeField] private int gradientTextureSize = 512; // Size of the gradient texture
    
    private VisualElement root;
    private VisualElement gradientOverlay;
    private Texture2D gradientTexture;
    
    private int previousHealth;
    private Coroutine flashCoroutine;
    
    void Start()
    {
        // Get UIDocument if not assigned
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        // Find PlayerHealth if not assigned
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }
        
        if (uiDocument == null)
        {
            Debug.LogWarning("DamageFlashUI: No UIDocument found. Please assign one in the Inspector.");
            return;
        }
        
        if (playerHealth == null)
        {
            Debug.LogWarning("DamageFlashUI: No PlayerHealth found. Please assign one in the Inspector.");
            return;
        }
        
        // Initialize UI elements
        InitializeUI();
        
        // Store initial health
        previousHealth = playerHealth.health;
    }
    
    void Update()
    {
        if (playerHealth == null) return;
        
        // Check if health decreased (player took damage)
        if (playerHealth.health < previousHealth)
        {
            TriggerFlash();
        }
        
        previousHealth = playerHealth.health;
    }
    
    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        
        // Create gradient texture
        CreateGradientTexture();
        
        // Create full-screen overlay
        gradientOverlay = new VisualElement();
        gradientOverlay.style.position = Position.Absolute;
        gradientOverlay.style.top = 0;
        gradientOverlay.style.bottom = 0;
        gradientOverlay.style.left = 0;
        gradientOverlay.style.right = 0;
        gradientOverlay.style.backgroundColor = new StyleColor(Color.clear);
        gradientOverlay.style.backgroundImage = new StyleBackground(gradientTexture);
        gradientOverlay.style.opacity = 0f; // Start invisible
        
        // Add overlay to root (invisible by default)
        root.Add(gradientOverlay);
    }
    
    private void CreateGradientTexture()
    {
        gradientTexture = new Texture2D(gradientTextureSize, gradientTextureSize, TextureFormat.RGBA32, false);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        
        // Create gradient: red at edges, transparent in center
        for (int y = 0; y < gradientTextureSize; y++)
        {
            for (int x = 0; x < gradientTextureSize; x++)
            {
                // Calculate distance from nearest edge
                float distFromEdge = Mathf.Min(
                    x, // Distance from left edge
                    gradientTextureSize - x, // Distance from right edge
                    y, // Distance from top edge
                    gradientTextureSize - y  // Distance from bottom edge
                );
                
                // Calculate alpha based on distance from edge
                // Closer to edge = more opaque, further = more transparent
                float normalizedDist = distFromEdge / edgeThickness;
                float alpha = Mathf.Clamp01(1f - normalizedDist);
                
                // Apply smooth falloff curve for better visual effect
                alpha = Mathf.Pow(alpha, 2f); // Quadratic falloff for smoother gradient
                
                Color pixelColor = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
                gradientTexture.SetPixel(x, y, pixelColor);
            }
        }
        
        gradientTexture.Apply();
    }
    
    public void TriggerFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }
    
    private IEnumerator FlashCoroutine()
    {
        // Show the gradient overlay at full opacity
        gradientOverlay.style.opacity = 1f;
        
        // Fade out over time
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float opacity = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            gradientOverlay.style.opacity = opacity;
            
            yield return null;
        }
        
        // Ensure overlay is fully transparent at the end
        gradientOverlay.style.opacity = 0f;
        
        flashCoroutine = null;
    }
    
    void OnDestroy()
    {
        // Clean up texture to prevent memory leaks
        if (gradientTexture != null)
        {
            Destroy(gradientTexture);
        }
    }
}

