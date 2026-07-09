using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    private Image fadeImage;
    private Coroutine fadeRoutine;

    public static ScreenFader Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        CreateFadeUI();
    }

    private void CreateFadeUI()
    {
        GameObject canvasObj = new GameObject("RuntimeFadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("RuntimeBlackFade");
        imageObj.transform.SetParent(canvasObj.transform, false);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;

        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void FadeOut(float duration = 1f)
    {
        StartFade(0f, 1f, duration);
    }

    public void FadeIn(float duration = 1f)
    {
        StartFade(1f, 0f, duration);
    }

    private void StartFade(float from, float to, float duration)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Fade(from, to, duration));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        Color color = fadeImage.color;
        color.a = from;
        fadeImage.color = color;

        while (t < duration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(from, to, t / duration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = to;
        fadeImage.color = color;
    }
}