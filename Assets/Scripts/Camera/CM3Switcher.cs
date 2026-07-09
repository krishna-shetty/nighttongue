using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CM3Switcher : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineCamera CapsuleCam;
    [SerializeField]
    public CinemachineCamera BallCam;

    [Header("Free Camera")]
    [SerializeField]
    private KeyCode _freeCamToggleKey = KeyCode.F1;
    [SerializeField]
    private KeyCode _detachedCamToggleKey = KeyCode.F3;

    [Header("References")]
    [SerializeField]
    private TongueTransformHandler _transformHandler;
    [Tooltip("Auto-found in parent if left empty.")]
    [SerializeField]
    private PlayerInputHandler _inputHandler;
    [SerializeField]
    private GameObject _cameraParent;


    // ── private ───────────────────────────────────────────────────────────────
    private CinemachineBrain _brain;
    private bool _isFreeCam = false;
    private bool _isDetachedCam = false;
    private CinemachineCamera _lastActiveCMCam;

    // ── zoom stuff ────────────────────────────────────────────────────────────
    private Vector3 _lastCamPos;
    private float _lastCamOrtho;
    private float _lastCamNearPlane;

    [System.Obsolete]
    private void Start()
    {
        // Cinemachine brain
        _brain = Camera.main.GetComponent<CinemachineBrain>();
        if (_brain == null)
            Debug.LogWarning("[CM3Switcher] No CinemachineBrain found on Camera.main.");

        if (_inputHandler == null)
            _inputHandler = GetComponentInParent<PlayerInputHandler>();
        if (_inputHandler == null)
            Debug.LogWarning("[CM3Switcher] No PlayerInputHandler found in parent — player won't be frozen in free-cam.");

        // Default state
        CapsuleCam.Priority = 20;
        BallCam.Priority = 10;
        _lastActiveCMCam = CapsuleCam;

        if (_transformHandler != null)
            _transformHandler.OnTransformStateChanged += HandleTransform;

        _cameraParent = transform.parent.gameObject;

        _lastCamPos = Camera.main.transform.position;
        _lastCamOrtho = Camera.main.orthographicSize;
        _lastCamNearPlane = Camera.main.nearClipPlane;
    }

    [System.Obsolete]
    private void OnDestroy()
    {
        if (_transformHandler != null)
            _transformHandler.OnTransformStateChanged -= HandleTransform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(_freeCamToggleKey))
            ToggleFreeCam();

        if (Input.GetKeyDown(_detachedCamToggleKey))
            ToggleDetachedCam();
    }

    // ── Free cam toggle ───────────────────────────────────────────────────────
    void ToggleFreeCam()
    {
        if (_isFreeCam) ExitFreeCam();
        else EnterFreeCam();
    }

    void EnterFreeCam()
    {
        _isFreeCam = true;

        if (_brain != null) _brain.enabled = false;

        var cam = Camera.main;
        cam.orthographic = true;
        cam.ResetProjectionMatrix();

        CapsuleCam.Priority = 0;
        BallCam.Priority = 0;
    }

    void ExitFreeCam()
    {
        _isFreeCam = false;

        RestoreCMCam(_lastActiveCMCam);

        if (_brain != null) _brain.enabled = true;

        Debug.LogWarning("unfreezing input");
    }

    // ── Detached cam toggle ───────────────────────────────────────────────────

    void ToggleDetachedCam()
    {
        if (_isDetachedCam) ExitDetachedCam();
        else EnterDetachedCam();
    }

    void EnterDetachedCam()
    {
        _isDetachedCam = true;
        transform.SetParent(null);
        Debug.Log("Camera Detached");
    }

    void ExitDetachedCam()
    {
        _isDetachedCam = false;
        transform.SetParent(_cameraParent.transform, true);
        Debug.Log("Camera Re-attached");
    }

    [System.Obsolete]
    void HandleTransform(TongueTransformEventArgs e)
    {
        if (e.IsTransformed)
        {
            _lastActiveCMCam = BallCam;
            if (!_isFreeCam)
            {
                BallCam.Priority = 20;
                CapsuleCam.Priority = 10;
            }
            CameraEffects.Instance.SetActiveCam(BallCam);
            FindObjectOfType<DynamicDOF>()?.SetActiveCamera(BallCam);
        }
        else
        {
            _lastActiveCMCam = CapsuleCam;
            if (!_isFreeCam)
            {
                CapsuleCam.Priority = 20;
                BallCam.Priority = 10;
            }
            CameraEffects.Instance.SetActiveCam(CapsuleCam);
            FindObjectOfType<DynamicDOF>()?.SetActiveCamera(CapsuleCam);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void RestoreCMCam(CinemachineCamera camToActivate)
    {
        if (camToActivate == BallCam)
        {
            BallCam.Priority = 20;
            CapsuleCam.Priority = 10;
        }
        else
        {
            CapsuleCam.Priority = 20;
            BallCam.Priority = 10;
        }
    }

    public void ZoomIn(Vector3 targetPos, float orthoSize, float duration, float nearPlane)
    {
        _lastCamPos = Camera.main.transform.position;
        _lastCamOrtho = Camera.main.orthographicSize;
        _lastCamNearPlane = Camera.main.nearClipPlane;
        EnterFreeCam();
        StartCoroutine(ZoomCoroutine(targetPos, orthoSize, duration, nearPlane));
    }

    private IEnumerator ZoomCoroutine(Vector3 targetPos, float targetOrtho, float duration, float targetNear, bool revertZoom = false)
    {
        Vector3 startPos = Camera.main.transform.position;
        float startOrtho = Camera.main.orthographicSize;
        float startNear = Camera.main.nearClipPlane;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Camera.main.transform.position = Vector3.Lerp(startPos, targetPos, t);
            Camera.main.orthographicSize = Mathf.Lerp(startOrtho, targetOrtho, t);
            Camera.main.nearClipPlane = Mathf.Lerp(startNear, targetNear, t);
            yield return null;
        }

        Camera.main.transform.position = targetPos;
        Camera.main.orthographicSize = targetOrtho;
        Camera.main.nearClipPlane = targetNear;

        if (revertZoom)
        {
            ExitFreeCam();
            RestoreCMCam(_lastActiveCMCam);
        }
    }

    public void ZoomOut(float duration)
    {
        StartCoroutine(ZoomCoroutine(_lastCamPos, _lastCamOrtho, duration, _lastCamNearPlane, true));
    }
}