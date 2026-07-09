using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class HeartButtonAnimator : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image img;
    private Vector3 originalScale;
    private Vector2 originalPos;
    private Vector2 targetOriginalPos;
    private Color originalColor;
    private bool initialized = false;

    [Header("1. Remote Sprite (The one that reacts)")]
    [SerializeField] private RectTransform verticalTarget;
    [SerializeField] private Vector2 targetShift = new Vector2(-15f, -15f);

    [Header("2. Button Hover Animation")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private Vector2 hoverShift = new Vector2(0, 10f);
    [SerializeField] private Color hoverColor = new Color(1f, 0.5f, 0.5f);
    [SerializeField] private float duration = 0.25f;

    [Header("3. Click Animation")]
    [SerializeField] private float clickScale = 0.95f;

    void Awake()
    {
        // Initialize in Awake so values are ready before any events fire
        rectTransform = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        originalScale = transform.localScale;
        originalPos = rectTransform.anchoredPosition;
        originalColor = img.color;

        if (verticalTarget != null)
            targetOriginalPos = verticalTarget.anchoredPosition;

        initialized = true;
    }

    public void OnHoverEnter()
    {
        if (!initialized) return;

        // Kill active tweens before starting new ones to prevent drift
        rectTransform.DOKill();
        transform.DOKill();
        if (verticalTarget != null) verticalTarget.DOKill();

        rectTransform.DOAnchorPos(originalPos + hoverShift, duration).SetEase(Ease.OutBack);
        transform.DOScale(originalScale * hoverScale, duration).SetEase(Ease.OutBack);
        img.DOColor(hoverColor, duration);

        if (verticalTarget != null)
            verticalTarget.DOAnchorPos(targetOriginalPos + targetShift, duration).SetEase(Ease.OutSine);
    }

    public void OnHoverExit()
    {
        if (!initialized) return;

        rectTransform.DOKill();
        transform.DOKill();
        if (verticalTarget != null) verticalTarget.DOKill();

        rectTransform.DOAnchorPos(originalPos, duration).SetEase(Ease.InOutSine);
        transform.DOScale(originalScale, duration).SetEase(Ease.InOutSine);
        img.DOColor(originalColor, duration);

        if (verticalTarget != null)
            verticalTarget.DOAnchorPos(targetOriginalPos, duration).SetEase(Ease.InOutSine);
    }

    public void OnPressDown()
    {
        if (!initialized) return;
        transform.DOKill();
        transform.DOScale(originalScale * clickScale, 0.1f);
    }

    public void OnPressUp()
    {
        if (!initialized) return;
        transform.DOKill();
        transform.DOScale(originalScale * hoverScale, 0.1f);
    }

    public void SimulateHoverEnter() => OnHoverEnter();
    public void SimulateHoverExit() => OnHoverExit();

    private void OnDestroy()
    {
        transform.DOKill();
        if (rectTransform != null) rectTransform.DOKill();
        if (img != null) img.DOKill();
        if (verticalTarget != null) verticalTarget.DOKill();
    }
}