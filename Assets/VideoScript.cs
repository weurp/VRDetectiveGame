using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

// Main VR Video Manager - attach to a Canvas or empty GameObject with RawImage
public class VRVideoManager : MonoBehaviour
{
    [Header("Video Display Settings")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay; // UI RawImage to show video
    public RenderTexture renderTexture; // Render texture for video output

    [Header("Video Positioning")]
    public Transform playerHead; // XR Camera/Head transform
    public float distanceFromPlayer = 3f;
    public float heightOffset = 0f;
    public Vector2 videoSize = new Vector2(400, 300); // Width x Height

    [Header("Video Settings")]
    public bool autoHideAfterVideo = true;
    public float fadeSpeed = 2f;

    [Header("Welcome Video Settings")]
    public bool playWelcomeVideo = true;
    public VideoClip welcomeVideoClip;
    public float welcomeDelay = 2f;

    private static VRVideoManager instance;
    private Coroutine currentVideoCoroutine;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        instance = this;
        SetupVideoPlayer();
        SetupCanvas();
    }

    void Start()
    {
        // Play welcome video when game starts
        if (playWelcomeVideo && welcomeVideoClip != null)
        {
            StartCoroutine(PlayWelcomeVideoDelayed());
        }
    }

    void SetupVideoPlayer()
    {
        // Create VideoPlayer if not assigned
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        // Create RenderTexture if not assigned
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(1920, 1080, 16);
            renderTexture.Create();
        }

        // Setup VideoPlayer
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        // Assign render texture to RawImage
        if (videoDisplay != null)
        {
            videoDisplay.texture = renderTexture;
        }
    }

    void SetupCanvas()
    {
        // Get or add CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Start invisible
        canvasGroup.alpha = 0f;

        // Set canvas size
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = videoSize;
        }
    }

    private IEnumerator PlayWelcomeVideoDelayed()
    {
        yield return new WaitForSeconds(welcomeDelay);
        PlayVideo(welcomeVideoClip);
    }

    void Update()
    {
        // Position video in front of player
        if (playerHead != null && canvasGroup.alpha > 0)
        {
            Vector3 targetPosition = playerHead.position +
                                   playerHead.forward * distanceFromPlayer +
                                   Vector3.up * heightOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 2f);

            // Make video face the player
            transform.LookAt(playerHead);
            transform.Rotate(0, 180, 0);
        }
    }

    // Static method to play video from anywhere
    public static void PlayVideo(VideoClip clip)
    {
        if (instance != null)
        {
            instance.StartVideoPlayback(clip);
        }
    }

    public static void PlayVideoFromPath(string videoPath)
    {
        if (instance != null)
        {
            instance.StartVideoPlaybackFromPath(videoPath);
        }
    }

    public static void StopVideo()
    {
        if (instance != null)
        {
            instance.StopVideoPlayback();
        }
    }

    public void StartVideoPlayback(VideoClip clip)
    {
        if (clip == null || videoPlayer == null) return;

        // Stop current video if playing
        if (currentVideoCoroutine != null)
        {
            StopCoroutine(currentVideoCoroutine);
        }

        videoPlayer.clip = clip;
        currentVideoCoroutine = StartCoroutine(PlayVideoCoroutine());
    }

    public void StartVideoPlaybackFromPath(string videoPath)
    {
        if (string.IsNullOrEmpty(videoPath) || videoPlayer == null) return;

        // Stop current video if playing
        if (currentVideoCoroutine != null)
        {
            StopCoroutine(currentVideoCoroutine);
        }

        videoPlayer.url = videoPath;
        currentVideoCoroutine = StartCoroutine(PlayVideoCoroutine());
    }

    public void StopVideoPlayback()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        if (currentVideoCoroutine != null)
        {
            StopCoroutine(currentVideoCoroutine);
            currentVideoCoroutine = null;
        }

        StartCoroutine(FadeVideo(canvasGroup.alpha, 0f));
    }

    private IEnumerator PlayVideoCoroutine()
    {
        // Fade in
        yield return StartCoroutine(FadeVideo(0f, 1f));

        // Start video
        videoPlayer.Play();

        // Wait for video to finish (if auto-hide is enabled)
        if (autoHideAfterVideo)
        {
            // Wait for video to finish
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            // Small delay after video ends
            yield return new WaitForSeconds(0.5f);

            // Fade out
            yield return StartCoroutine(FadeVideo(1f, 0f));
        }
    }

    private IEnumerator FadeVideo(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        float duration = 1f / fadeSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}
