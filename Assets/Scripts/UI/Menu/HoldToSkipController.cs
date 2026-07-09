using UnityEngine;
using UnityEngine.UI;
using System;

public class HoldToSkipController : MonoBehaviour
{
    [Header("UI")]
    public GameObject skipButton;
    public Image holdProgressFill;

    [Header("Settings")]
    public float holdTime = 3f;
    public float inputTimeout = 2f;

    public event Action OnSkipTriggered;

    private float holdTimer = 0f;
    private float lastInputTime = 0f;
    private bool _isCancelHeld = false;
    private bool _active = true;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancelStarted += HandleCancelStarted;
            InputManager.Instance.OnCancelCanceled += HandleCancelCanceled;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancelStarted -= HandleCancelStarted;
            InputManager.Instance.OnCancelCanceled -= HandleCancelCanceled;
        }
    }

    private void HandleCancelStarted() => _isCancelHeld = true;
    private void HandleCancelCanceled() => _isCancelHeld = false;

    public void SetActive(bool active)
    {
        Debug.Log("HoldToSkipController :: HoldToSkipController active set to " +  active);
        _active = active;
        if (!active) HideSkipButton();
    }

    void Update()
    {
        if (!_active) return;

        DetectPlayerInput();
        HandleSkipButtonVisibility();

        if (skipButton != null && skipButton.activeSelf)
            HandleHoldSkip();
    }

    void DetectPlayerInput()
    {
        bool hasInput = Input.anyKey
            || Input.GetAxis("Horizontal") != 0
            || Input.GetAxis("Vertical") != 0
            || Input.GetAxis("Mouse X") != 0
            || Input.GetAxis("Mouse Y") != 0;

        if (hasInput)
        {
            lastInputTime = Time.time;
            if (skipButton != null && !skipButton.activeSelf)
                skipButton.SetActive(true);
        }
    }

    void HandleSkipButtonVisibility()
    {
        if (skipButton != null && skipButton.activeSelf)
            if (Time.time - lastInputTime > inputTimeout)
                HideSkipButton();
    }

    public void HideSkipButton()
    {
        if (skipButton != null)
            skipButton.SetActive(false);

        holdTimer = 0f;

        if (holdProgressFill != null)
            holdProgressFill.fillAmount = 0f;
    }

    void HandleHoldSkip()
    {
        if (Input.anyKey && !_isCancelHeld)
        {
            holdTimer += Time.deltaTime;

            if (holdProgressFill != null)
                holdProgressFill.fillAmount = Mathf.Clamp01(holdTimer / holdTime);

            if (holdTimer >= holdTime)
            {
                SetActive(false);
                OnSkipTriggered?.Invoke();
            }
        }
        else
        {
            holdTimer = 0f;

            if (holdProgressFill != null)
                holdProgressFill.fillAmount = 0f;
        }
    }
}