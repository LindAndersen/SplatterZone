using UnityEngine;
using UnityEngine.UI;

public class WindowHighlight : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.cyan;
    public float glowIntensity = 2f;
    public float pulseSpeed = 2f;
    
    [Header("Key Prompt")]
    public GameObject keyPromptSprite;
    public Sprite keyPromptIcon;
    public Vector3 promptOffset = new Vector3(0, 1.5f, 0);

    private Image keyPromptImage;
    
    private Renderer windowRenderer;
    private Color originalEmissionColor;
    private Material glowMaterial;
    private bool isPulsing = false;
    private Camera mainCamera;

    void Start()
    {
        // Find the renderer on the parent (window)
        windowRenderer = GetComponentInParent<Renderer>();

        if (windowRenderer != null)
        {
            glowMaterial = windowRenderer.sharedMaterial;
            originalEmissionColor = glowMaterial.GetColor("_EmissionColor");
            Debug.Log("WindowHighlight: Found window renderer, original emission: " + originalEmissionColor);
        }
        else
        {
            Debug.LogWarning("WindowHighlight: No Renderer found on parent!");
        }

        mainCamera = Camera.main;

        if (keyPromptSprite == null)
        {
            // Create a WorldSpace Canvas with an Image for the key prompt at the root of the scene
            GameObject canvasGO = new GameObject("KeyPromptCanvas");
            canvasGO.transform.SetParent(null); // No parent, root of scene
            canvasGO.transform.localScale = Vector3.one * 0.002f;
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            var raycaster = canvasGO.AddComponent<GraphicRaycaster>();

            // Create the Image
            GameObject imageGO = new GameObject("KeyPromptImage");
            imageGO.transform.SetParent(canvasGO.transform);
            imageGO.transform.localScale = Vector3.one;
            imageGO.transform.localPosition = Vector3.zero;
            keyPromptImage = imageGO.AddComponent<Image>();
            // Set sprite if assigned
            if (keyPromptIcon != null)
                keyPromptImage.sprite = keyPromptIcon;
            keyPromptImage.color = Color.white;

            // Set the reference
            keyPromptSprite = canvasGO;
        }
        else
        {
            // Try to find the Image component in the assigned object
            keyPromptImage = keyPromptSprite.GetComponentInChildren<Image>();
            if (keyPromptImage != null && keyPromptIcon != null)
                keyPromptImage.sprite = keyPromptIcon;
        }
        keyPromptSprite.SetActive(false);
    }

    void Update()
    {
        if (isPulsing && glowMaterial != null)
        {
            // Pulse between original emission color and glow color
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            Color currentColor = Color.Lerp(originalEmissionColor, glowColor * glowIntensity, t);
            glowMaterial.SetColor("_EmissionColor", currentColor);
        }

        if (keyPromptSprite != null && keyPromptSprite.activeSelf && mainCamera != null)
        {
            // Position the prompt above the trigger
            keyPromptSprite.transform.position = transform.position + promptOffset;

            // Make the prompt face the camera
            keyPromptSprite.transform.LookAt(mainCamera.transform);
            keyPromptSprite.transform.Rotate(0, 180, 0); // Flip to face player
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EnablePulsing();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            DisablePulsing();
        }
    }

    void EnablePulsing()
    {
        if (glowMaterial != null)
        {
            Debug.Log("WindowHighlight: Starting pulse on window");
            glowMaterial.EnableKeyword("_EMISSION");
            glowMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            isPulsing = true;
        }

        if (keyPromptSprite != null)
        {
            keyPromptSprite.SetActive(true);
        }
    }

    void DisablePulsing()
    {
        if (glowMaterial != null)
        {
            Debug.Log("WindowHighlight: Stopping pulse on window");
            isPulsing = false;
            glowMaterial.SetColor("_EmissionColor", originalEmissionColor);
        }

        if (keyPromptSprite != null)
        {
            keyPromptSprite.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        // Draw a sphere showing where the key prompt will be positioned
        Vector3 promptPosition = transform.position + promptOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(promptPosition, 0.1f);
        
        // Draw a line from the trigger to the prompt position
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, promptPosition);
    }
}
