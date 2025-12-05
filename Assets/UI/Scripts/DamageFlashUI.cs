using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

/************************************
 * Creates a red flash effect on screen edges when player takes damage.
 * Uses UI Toolkit to display an overlay that fades out over time.
 * Version 1.0
 * 
 * Add this script to the same GameObject as the UIDocument.
 * Assign the UIDocument in the Inspector.
 * Assign the PlayerHealth in the Inspector.
 * The script will automatically find the PlayerHealth component in the scene.
 * The script will automatically find the UIDocument component in the scene.
 * The script will automatically find the PlayerHealth component in the scene.
 * Gleb
 * December 5, 2025
 ************************************/
public class DamageFlashUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerHealth playerHealth;
    
    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 1f;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.5f); // Red with transparency
    [SerializeField] private int edgeThickness = 20; // Thickness of the red edges in pixels
    
    private VisualElement root;
    private VisualElement topEdge;
    private VisualElement bottomEdge;
    private VisualElement leftEdge;
    private VisualElement rightEdge;
    
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
        
        // Create edge overlays
        topEdge = new VisualElement();
        bottomEdge = new VisualElement();
        leftEdge = new VisualElement();
        rightEdge = new VisualElement();
        
        // Style the edges
        StyleEdge(topEdge);
        StyleEdge(bottomEdge);
        StyleEdge(leftEdge);
        StyleEdge(rightEdge);
        
        // Position the edges
        topEdge.style.position = Position.Absolute;
        topEdge.style.top = 0;
        topEdge.style.left = 0;
        topEdge.style.right = 0;
        topEdge.style.height = edgeThickness;
        topEdge.style.backgroundColor = new StyleColor(Color.clear);
        
        bottomEdge.style.position = Position.Absolute;
        bottomEdge.style.bottom = 0;
        bottomEdge.style.left = 0;
        bottomEdge.style.right = 0;
        bottomEdge.style.height = edgeThickness;
        bottomEdge.style.backgroundColor = new StyleColor(Color.clear);
        
        leftEdge.style.position = Position.Absolute;
        leftEdge.style.top = 0;
        leftEdge.style.bottom = 0;
        leftEdge.style.left = 0;
        leftEdge.style.width = edgeThickness;
        leftEdge.style.backgroundColor = new StyleColor(Color.clear);
        
        rightEdge.style.position = Position.Absolute;
        rightEdge.style.top = 0;
        rightEdge.style.bottom = 0;
        rightEdge.style.right = 0;
        rightEdge.style.width = edgeThickness;
        rightEdge.style.backgroundColor = new StyleColor(Color.clear);
        
        // Add edges to root (they're invisible by default)
        root.Add(topEdge);
        root.Add(bottomEdge);
        root.Add(leftEdge);
        root.Add(rightEdge);
    }
    
    private void StyleEdge(VisualElement edge)
    {
        edge.style.position = Position.Absolute;
        edge.style.backgroundColor = new StyleColor(Color.clear);
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
        // Set edges to red
        topEdge.style.backgroundColor = new StyleColor(flashColor);
        bottomEdge.style.backgroundColor = new StyleColor(flashColor);
        leftEdge.style.backgroundColor = new StyleColor(flashColor);
        rightEdge.style.backgroundColor = new StyleColor(flashColor);
        
        // Fade out over time
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / flashDuration);
            
            Color currentColor = flashColor;
            currentColor.a = alpha;
            
            topEdge.style.backgroundColor = new StyleColor(currentColor);
            bottomEdge.style.backgroundColor = new StyleColor(currentColor);
            leftEdge.style.backgroundColor = new StyleColor(currentColor);
            rightEdge.style.backgroundColor = new StyleColor(currentColor);
            
            yield return null;
        }
        
        // Ensure edges are fully transparent at the end
        topEdge.style.backgroundColor = new StyleColor(Color.clear);
        bottomEdge.style.backgroundColor = new StyleColor(Color.clear);
        leftEdge.style.backgroundColor = new StyleColor(Color.clear);
        rightEdge.style.backgroundColor = new StyleColor(Color.clear);
        
        flashCoroutine = null;
    }
}

