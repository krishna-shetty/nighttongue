using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IntroVideoController : MonoBehaviour
{
    [Header("UI")]
    public GameObject videoRawImage;
    public GameObject mainMenu;
    public GameObject startButton;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public VideoClip endingClip;

    [Header("Dissolve")]
    public DissolveController dissolveController;

    [Header("Skip")]
    public HoldToSkipController holdToSkip;

    private bool isTransitioning = false;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancelPerformed += HandleCancel;
        }

        if (LevelManager.Instance != null)
            LevelManager.Instance.SetCanPause(false);

        if (holdToSkip != null)
            holdToSkip.OnSkipTriggered += EnterMainMenu;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancelPerformed -= HandleCancel;
        }

        if (LevelManager.Instance != null)
            LevelManager.Instance.SetCanPause(true);

        if (holdToSkip != null)
            holdToSkip.OnSkipTriggered -= EnterMainMenu;
    }

    private void HandleCancel()
    {
        if (videoPlayer == null)
            return;

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    void Start()
    {
        if (LevelManager.Instance.playedIntro)
        {
            EnterMainMenu();
            GameObject.Find("Opening Animation").SetActive(false);
        }
        else if (LevelManager.Instance.playEndingInsteadOfIntro)
        {
            videoPlayer.clip = endingClip;
            LevelManager.Instance.PreloadScene("Credits");
        }

        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoEnd;

        if (dissolveController != null)
            dissolveController.ResetDissolve();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (LevelManager.Instance.playEndingInsteadOfIntro)
        {
            LevelManager.Instance.LoadPreloadedScene();
            return;
            //AkUnitySoundEngine.PostEvent("MX_State_CRD", gameObject);
        }

        EnterMainMenu();
        AkUnitySoundEngine.PostEvent("MX_State_CRD", gameObject);
    }

    void EnterMainMenu()
    {
        LevelManager.Instance.playedIntro = true;

        if (isTransitioning)
            return;

        isTransitioning = true;

        if (holdToSkip != null)
            holdToSkip.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.Stop();
        }

        if (dissolveController != null)
        {
            if (mainMenu != null)
                mainMenu.SetActive(true);

            AkUnitySoundEngine.PostEvent("MX_State_MEN", gameObject);

            dissolveController.StartDissolve(() =>
            {
                FinishToMainMenu();
            });
        }
        else
        {
            AkUnitySoundEngine.PostEvent("MX_State_MEN", gameObject);
            FinishToMainMenu();
        }
    }

    void FinishToMainMenu()
    {
        if (videoRawImage != null)
            Destroy(videoRawImage);

        if (videoPlayer != null)
            Destroy(videoPlayer.gameObject);

        if (startButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(startButton);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;

        if (InputManager.Instance != null)
            InputManager.Instance.OnCancelPerformed -= HandleCancel;

        if (holdToSkip != null)
            holdToSkip.OnSkipTriggered -= EnterMainMenu;
    }
}