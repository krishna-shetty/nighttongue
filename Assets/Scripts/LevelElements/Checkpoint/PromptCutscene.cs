using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class PromptCutscene : MonoBehaviour
{
    [Header("When using the CutscenePrompter prefab, make sure to set the Yes/No button click events")]
    [Header("and the prompt cutscene field in CheckpointRecorder")]
    // yes -> PlayVideo() and set text of CheckpointRecorder
    // no -> EnableCanvas(false) and set text of CheckpointRecorder

    public VideoPlayer VideoPlayer;
    public Canvas PromptCanvas;
    public GameObject VideoDisplay;
    public CanvasGroup VideoDisplayCanvasGroup;
    public GameObject PlayerCapsule;
    public GameObject PlayerSphere;
    public SoundtrackManager WwiseGameObject;

    public float ZoomOrthoSize = 5f;
    public float ZoomToVideoDuration = 3f;
    public float ZoomNearPlane = -5f;

    private GameObject _player;
    private TongueTransformHandler _tongueHandler;
    private PlayerInputHandler _inputHandler;
    private GameObject _mainCamera;
    private GameObject _UICamera;
    private CM3Switcher _CM3Switcher;

    private bool _isPrepared = false;
    private bool _playQueued = false;

    [Header("Skip")]
    [SerializeField]
    private HoldToSkipController _holdToSkip;

    [Header("Watch Condition")]
    public bool canWatch = true;

    public void SetCanWatch(bool value)
    {
        canWatch = value;
    }

    private void Awake()
    {
        _player = GameObject.Find("Player");
        _tongueHandler = _player.GetComponent<TongueTransformHandler>();
        _inputHandler = _player.GetComponent<PlayerInputHandler>();
        _mainCamera = GameObject.Find("Main Camera");
        _UICamera = GameObject.Find("UI Camera");
        _CM3Switcher = GameObject.Find("Camera").GetComponent<CM3Switcher>();

        if (VideoPlayer != null)
        {
            VideoPlayer.playOnAwake = false;
            VideoPlayer.waitForFirstFrame = true;
            VideoPlayer.sendFrameReadyEvents = true;
        }

        if (VideoDisplay != null)
        {
            VideoDisplay.SetActive(true);
        }

        if (VideoDisplayCanvasGroup != null)
        {
            VideoDisplayCanvasGroup.alpha = 0f;
            VideoDisplayCanvasGroup.interactable = false;
            VideoDisplayCanvasGroup.blocksRaycasts = false;
        }

        EnableCanvas(false);
    }

    private void Start()
    {
        if (_holdToSkip != null)
        {
            _holdToSkip.SetActive(false);
        }

        PreparePreviewFrame();
    }

    private void OnEnable()
    {
        if (VideoPlayer != null)
        {
            VideoPlayer.started += OnVideoStart;
            VideoPlayer.loopPointReached += OnVideoEnd;
            VideoPlayer.prepareCompleted += OnVideoPrepared;
        }

        if (_inputHandler != null)
        {
            _inputHandler.OnPickup += PlayVideo;
        }

        if (_holdToSkip != null)
        {
            _holdToSkip.OnSkipTriggered += SkipVideo;
        }
    }

    private void OnDisable()
    {
        if (VideoPlayer != null)
        {
            VideoPlayer.started -= OnVideoStart;
            VideoPlayer.loopPointReached -= OnVideoEnd;
            VideoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        if (_inputHandler != null)
        {
            _inputHandler.OnPickup -= PlayVideo;
        }

        if(_holdToSkip != null)
        {
            _holdToSkip.OnSkipTriggered -= SkipVideo;
        }
    }

    private void SkipVideo()
    {
        if (VideoPlayer != null && VideoPlayer.isPlaying)
        {
            OnVideoEnd(VideoPlayer);
        }
    }

    private void PreparePreviewFrame()
    {
        if (VideoPlayer == null)
            return;

        _isPrepared = false;
        _playQueued = false;

        SetVideoDisplayVisible(false);

        VideoPlayer.Pause();
        VideoPlayer.time = 0;
        VideoPlayer.frame = 0;
        VideoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (vp == null)
            return;

        vp.frame = 0;
        vp.Pause();

        _isPrepared = true;

        // 预览帧准备好后再显示，避免白闪
        SetVideoDisplayVisible(true);

        if (_playQueued)
        {
            _playQueued = false;
            VideoPlayer.Play();
        }
    }

    public void PlayVideo()
    {
        if (PromptCanvas == null || !PromptCanvas.gameObject.activeInHierarchy)
            return;

        if (VideoPlayer == null)
            return;

        SetVideoDisplayVisible(true);

        if (!_isPrepared)
        {
            _playQueued = true;
            VideoPlayer.Prepare();
            return;
        }

        VideoPlayer.Play();
    }

    private void SetPlayerModelActive(bool active)
    {
        if (_tongueHandler.IsTransformed)
        {
            if (PlayerSphere != null)
                PlayerSphere.SetActive(active);
        }
        else
        {
            if (PlayerCapsule != null)
                PlayerCapsule.SetActive(active);
        }
    }

    private void OnVideoStart(VideoPlayer vp)
    {
        LevelManager.Instance.SetCanPause(false);
        if (_inputHandler != null)
            _inputHandler.SetFrozen(true);

        if (_CM3Switcher != null && VideoDisplay != null)
            _CM3Switcher.ZoomIn(VideoDisplay.transform.position, ZoomOrthoSize, ZoomToVideoDuration, ZoomNearPlane);

        EnableCanvas(false);

        if (_UICamera != null)
            _UICamera.SetActive(false);

        if (VideoDisplay != null)
            VideoDisplay.SetActive(true);

        SetPlayerModelActive(false);

        if (_holdToSkip != null)
        {
            _holdToSkip.SetActive(true);
        }       
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        LevelManager.Instance.SetCanPause(true);
        if (_holdToSkip != null)
        {
            _holdToSkip.SetActive(false );
        }

        // 先隐藏，避免重置时闪白
        SetVideoDisplayVisible(false);

        if (VideoPlayer != null)
        {
            VideoPlayer.Pause();
            VideoPlayer.time = 0;
            VideoPlayer.frame = 0;
            VideoPlayer.Prepare();
        }

        if (_UICamera != null)
            _UICamera.SetActive(true);

        SetPlayerModelActive(true);

        if (_CM3Switcher != null)
            _CM3Switcher.ZoomOut(ZoomToVideoDuration);

        if (_inputHandler != null)
            _inputHandler.SetFrozen(false);

        // 重新准备第一帧，等准备好后再显示
        PreparePreviewFrame();
    }

    public void EnableCanvas(bool enabled)
    {
        if (enabled && !canWatch)
            return;

        if (PromptCanvas != null)
        {
            PromptCanvas.gameObject.SetActive(enabled);
        }

        if (enabled)
        {
            SetVideoDisplayVisible(true);
        }
    }

    private void SetVideoDisplayVisible(bool visible)
    {
        if (VideoDisplayCanvasGroup != null)
        {
            VideoDisplayCanvasGroup.alpha = visible ? 1f : 0f;
            VideoDisplayCanvasGroup.interactable = false;
            VideoDisplayCanvasGroup.blocksRaycasts = false;
            return;
        }

        if (VideoDisplay != null)
        {
            VideoDisplay.SetActive(visible);
        }
    }
}