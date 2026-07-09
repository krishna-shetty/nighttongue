using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class SliderButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                                               IPointerDownHandler, IPointerUpHandler,
                                               IDragHandler, IInitializePotentialDragHandler
{
    [Header("Hover")]
    public float hoverScale = 1.2f;
    public float hoverDuration = 0.2f;
    public Ease hoverEase = Ease.OutBack;

    [Header("Press")]
    public float pressScale = 0.9f;
    public float pressDuration = 0.1f;
    public Ease pressEase = Ease.OutQuad;

    [Header("Detection")]
    public float detectionPadding = 20f;

    private Vector3 _originalScale;
    private bool _isPressed = false;
    private bool _isHovered = false;
    private RectTransform _rectTransform;
    private Slider _parentSlider;

    void Awake()
    {
        _originalScale = transform.localScale;
        _rectTransform = GetComponent<RectTransform>();
        _parentSlider = GetComponentInParent<Slider>();
    }

    void Update()
    {
        if (_isPressed) return;

        Rect expandedRect = new Rect(
            _rectTransform.rect.x - detectionPadding,
            _rectTransform.rect.y - detectionPadding,
            _rectTransform.rect.width + detectionPadding * 2,
            _rectTransform.rect.height + detectionPadding * 2
        );

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform,
            Input.mousePosition,
            null,
            out localPoint
        );

        bool mouseOver = expandedRect.Contains(localPoint);

        if (mouseOver && !_isHovered)
        {
            _isHovered = true;
            transform.DOKill();
            transform.DOScale(_originalScale * hoverScale, hoverDuration)
                .SetEase(hoverEase)
                .SetUpdate(true);
        }
        else if (!mouseOver && _isHovered)
        {
            _isHovered = false;
            transform.DOKill();
            transform.DOScale(_originalScale, hoverDuration)
                .SetEase(hoverEase)
                .SetUpdate(true);
        }
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform, screenPoint, eventCamera, out localPoint);

        Rect expandedRect = new Rect(
            _rectTransform.rect.x - detectionPadding,
            _rectTransform.rect.y - detectionPadding,
            _rectTransform.rect.width + detectionPadding * 2,
            _rectTransform.rect.height + detectionPadding * 2
        );

        return expandedRect.Contains(localPoint);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isPressed || _isHovered) return;
        _isHovered = true;
        transform.DOKill();
        transform.DOScale(_originalScale * hoverScale, hoverDuration)
            .SetEase(hoverEase)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isPressed) return;
        _isHovered = false;
        transform.DOKill();
        transform.DOScale(_originalScale, hoverDuration)
            .SetEase(hoverEase)
            .SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        transform.DOKill();
        transform.DOScale(_originalScale * pressScale, pressDuration)
            .SetEase(pressEase)
            .SetUpdate(true);

        if (_parentSlider != null)
            ExecuteEvents.Execute(_parentSlider.gameObject, eventData, ExecuteEvents.pointerDownHandler);
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (_parentSlider != null)
            ExecuteEvents.Execute(_parentSlider.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_parentSlider != null)
            ExecuteEvents.Execute(_parentSlider.gameObject, eventData, ExecuteEvents.dragHandler);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        transform.DOKill();

        // Let Update() handle the exit — don't reset _isHovered here
        // Check if mouse is still over the button
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform,
            Input.mousePosition,
            null,
            out localPoint
        );

        Rect expandedRect = new Rect(
            _rectTransform.rect.x - detectionPadding,
            _rectTransform.rect.y - detectionPadding,
            _rectTransform.rect.width + detectionPadding * 2,
            _rectTransform.rect.height + detectionPadding * 2
        );

        if (expandedRect.Contains(localPoint))
        {
            // Mouse still over button, return to hover scale
            _isHovered = true;
            transform.DOScale(_originalScale * hoverScale, hoverDuration)
                .SetEase(hoverEase)
                .SetUpdate(true);
        }
        else
        {
            // Mouse already outside, return to original scale
            _isHovered = false;
            transform.DOScale(_originalScale, hoverDuration)
                .SetEase(hoverEase)
                .SetUpdate(true);
        }

        if (_parentSlider != null)
            ExecuteEvents.Execute(_parentSlider.gameObject, eventData, ExecuteEvents.pointerUpHandler);
    }
}