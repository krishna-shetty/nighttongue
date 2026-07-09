using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuStartSequence : MonoBehaviour
{
    [Header("Main Menu")]
    public CanvasGroup mainMenuCanvasGroup;
    public Button startButton;

    [Header("Video")]
    public GameObject transitionVideoRoot;
    public CanvasGroup transitionVideoCanvasGroup;
    public RawImage transitionVideoRawImage;
    public VideoPlayer transitionVideoPlayer;

    [Header("Settings")]
    public float fadeOutTime = 0.4f;

    private bool isStarting = false;
    private bool firstFrameReady = false;
    private bool isPreloading = false;

    [SerializeField]
    private GameObject buttonParent;

    private void Start()
    {
        if (startButton != null)
            startButton.interactable = true;

        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;
        }

        PrepareBackgroundVideo();
    }

    public void OnStartClicked()
    {
        if (isStarting)
            return;
        Debug.Log("test");
        Button[] otherButton = buttonParent.GetComponentsInChildren<Button>(true);
        foreach (Button currButton in otherButton)
        {
            currButton.enabled = false;
        }
        StartCoroutine(StartSequence());
    }

    private void PrepareBackgroundVideo()
    {
        if (transitionVideoRoot != null)
            transitionVideoRoot.SetActive(true);

        if (transitionVideoCanvasGroup != null)
        {
            transitionVideoCanvasGroup.alpha = 0f;
            transitionVideoCanvasGroup.interactable = false;
            transitionVideoCanvasGroup.blocksRaycasts = false;
        }

        if (transitionVideoRawImage != null)
            transitionVideoRawImage.enabled = false;

        if (transitionVideoPlayer == null)
            return;

        isPreloading = true;
        firstFrameReady = false;

        transitionVideoPlayer.Stop();
        transitionVideoPlayer.time = 0;
        transitionVideoPlayer.frame = 0;

        transitionVideoPlayer.playOnAwake = false;
        transitionVideoPlayer.waitForFirstFrame = true;
        transitionVideoPlayer.sendFrameReadyEvents = true;

        transitionVideoPlayer.prepareCompleted -= OnVideoPrepared;
        transitionVideoPlayer.loopPointReached -= OnVideoFinished;
        transitionVideoPlayer.frameReady -= OnFrameReady;

        transitionVideoPlayer.prepareCompleted += OnVideoPrepared;
        transitionVideoPlayer.loopPointReached += OnVideoFinished;
        transitionVideoPlayer.frameReady += OnFrameReady;

        transitionVideoPlayer.Prepare();
    }

    private IEnumerator StartSequence()
    {
        isStarting = true;

        if (startButton != null)
            startButton.interactable = false;

        if (mainMenuCanvasGroup != null)
        {
            float t = 0f;
            float startAlpha = mainMenuCanvasGroup.alpha;

            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeOutTime);
                mainMenuCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, p);
                yield return null;
            }

            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
        }

        while (!firstFrameReady)
            yield return null;

        if (transitionVideoCanvasGroup != null)
            transitionVideoCanvasGroup.alpha = 1f;

        if (transitionVideoRawImage != null)
            transitionVideoRawImage.enabled = true;

        if (transitionVideoPlayer != null)
        {
            transitionVideoPlayer.Play();
        }
        else
        {
            OnVideoFinished(null);
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (vp == null)
            return;

        vp.Play();
    }

    private void OnFrameReady(VideoPlayer vp, long frameIdx)
    {
        if (!isPreloading || firstFrameReady)
            return;

        firstFrameReady = true;

        if (vp != null)
            vp.Pause();

        if (transitionVideoRawImage != null)
            transitionVideoRawImage.enabled = true;

        if (transitionVideoCanvasGroup != null)
            transitionVideoCanvasGroup.alpha = 1f;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (transitionVideoPlayer != null)
        {
            transitionVideoPlayer.prepareCompleted -= OnVideoPrepared;
            transitionVideoPlayer.loopPointReached -= OnVideoFinished;
            transitionVideoPlayer.frameReady -= OnFrameReady;
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
        else
        {
            Debug.LogError("LevelManager.Instance is null.");
        }
    }

    private void OnDisable()
    {
        if (transitionVideoPlayer != null)
        {
            transitionVideoPlayer.prepareCompleted -= OnVideoPrepared;
            transitionVideoPlayer.loopPointReached -= OnVideoFinished;
            transitionVideoPlayer.frameReady -= OnFrameReady;
        }
    }
}