using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this for TextMeshPro
using System.Collections;

// Main VR Text Manager - attach to a Canvas or empty GameObject
public class VRTextManager : MonoBehaviour
{
    [Header("Text Display Settings")]
    public TextMeshProUGUI displayText; // Drag your TextMeshPro component here
    // public Text displayText; // Use this if you prefer regular UI Text
    public float textDuration = 3f; // How long text stays visible
    public float fadeSpeed = 2f; // Speed of fade in/out
    
    [Header("Text Positioning")]
    public Transform playerHead; // XR Camera/Head transform
    public float distanceFromPlayer = 2f;
    public float heightOffset = 0.5f;
    
    [Header("Welcome Message Settings")]
    public bool showWelcomeMessage = true;
    public string welcomeMessage = "Welcome to VR!";
    public float welcomeDelay = 2f; // Delay before showing welcome message
    
    private static VRTextManager instance;
    private Coroutine currentTextCoroutine;
    
    void Awake()
    {
        // Singleton pattern so we can call from anywhere
        instance = this;
        
        // Make sure text starts invisible
        if (displayText != null)
        {
            Color color = displayText.color;
            color.a = 0f;
            displayText.color = color;
        }
    }
    
    void Start()
    {
        // Show welcome message when game starts
        if (showWelcomeMessage && !string.IsNullOrEmpty(welcomeMessage))
        {
            StartCoroutine(ShowWelcomeMessageDelayed());
        }
    }
    
    private IEnumerator ShowWelcomeMessageDelayed()
    {
        yield return new WaitForSeconds(welcomeDelay);
        DisplayText(welcomeMessage);
    }
    
    void Update()
    {
        // Make text always face the player
        if (displayText != null && playerHead != null)
        {
            transform.LookAt(playerHead);
            transform.Rotate(0, 180, 0); // Flip so text faces player
            
            // Position text in front of player
            Vector3 targetPosition = playerHead.position + 
                                   playerHead.forward * distanceFromPlayer + 
                                   Vector3.up * heightOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 2f);
        }
    }
    
    // Static method to show text from anywhere
    public static void ShowText(string message)
    {
        if (instance != null)
        {
            instance.DisplayText(message);
        }
    }
    
    public void DisplayText(string message)
    {
        if (displayText == null) return;
        
        // Stop current text display if running
        if (currentTextCoroutine != null)
        {
            StopCoroutine(currentTextCoroutine);
        }
        
        // Start new text display
        currentTextCoroutine = StartCoroutine(ShowTextCoroutine(message));
    }
    
    private IEnumerator ShowTextCoroutine(string message)
    {
        displayText.text = message;
        
        // Fade in
        yield return StartCoroutine(FadeText(0f, 1f));
        
        // Wait for duration
        yield return new WaitForSeconds(textDuration);
        
        // Fade out
        yield return StartCoroutine(FadeText(1f, 0f));
    }
    
    private IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        Color color = displayText.color;
        float elapsed = 0f;
        float duration = 1f / fadeSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            displayText.color = color;
            yield return null;
        }
        
        color.a = endAlpha;
        displayText.color = color;
    }
}