using System.Collections;
using UnityEngine;

public class FadeOverlay : MonoBehaviour
{
    public static FadeOverlay Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    public Coroutine FadeToBlack(float duration)
    {
        return StartCoroutine(FadeRoutine(0f, 1f, duration));
    }

    public Coroutine FadeFromBlack(float duration)
    {
        return StartCoroutine(FadeRoutine(1f, 0f, duration));
    }

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;
        canvasGroup.alpha = from;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
        }
        else
        {
            float t = 0f;
            while (t < duration)
            {
                if (canvasGroup == null)
                    yield break;

                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        if (canvasGroup != null && Mathf.Approximately(to, 0f))
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}