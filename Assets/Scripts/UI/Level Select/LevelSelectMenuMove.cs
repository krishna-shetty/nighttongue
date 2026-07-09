using UnityEngine;
using DG.Tweening;
using Yarn.Unity;

public class UIContainerSlide : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("Animation Style")]
    [SerializeField] private Ease slideEase = Ease.OutBack;
    [Range(0.1f, 5f)] // Adds a slider in the Inspector for convenience
    [SerializeField] private float slideDuration = 1.2f;

    [Header("Position Settings")]
    [SerializeField] private float startY = 1000f; // Off-screen top
    [SerializeField] private float endY = 0f;      // Middle of screen
    [SerializeField] private float duration = 1.2f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Ensure it starts at the top immediately on wake
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = startY;
        rectTransform.anchoredPosition = pos;
    }

    [YarnCommand("SlideContainer")]
    public void SlideIn()
    {
        Debug.Log("Yarn is sliding the container now!");

        // Use DOAnchorPosY to move the parent RectTransform
        rectTransform.DOAnchorPosY(endY, duration)
            .SetEase(slideEase) // Now uses the variable from the Inspector
            .SetUpdate(true); // Works even if the game is paused
    }
}