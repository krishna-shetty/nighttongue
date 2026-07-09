using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathCircleFader : MonoBehaviour
{
    public static DeathCircleFader Instance { get; private set; }

    [SerializeField] private Shader circleFadeShader;
    [SerializeField] private float fadeToMaskDuration = 0.15f;
    [SerializeField] private float closeCircleDuration = 0.35f;
    [SerializeField] private float fullScreenRadius = 1.5f;
    [SerializeField] private float startRadius = 0.15f;
    [SerializeField] private float softness = 0f;

    private Material fadeMaterial;
    private Image fadeImage;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        CreateRuntimeUI();
    }

    private void CreateRuntimeUI()
    {
        GameObject canvasObj = new GameObject("DeathFadeCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("DeathCircleFadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.raycastTarget = false;

        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeMaterial = new Material(circleFadeShader);
        fadeImage.material = fadeMaterial;

        SetAlpha(0f);
        SetRadius(startRadius);
        fadeMaterial.SetFloat("_Softness", softness);
    }

    public void StartDeathFade(Transform target)
    {
        StartCoroutine(DeathFadeRoutine(target));
    }

    private IEnumerator DeathFadeRoutine(Transform target)
    {
        UpdateCenter(target);

        SetAlpha(1f);
        SetRadius(fullScreenRadius);

        float t = 0f;

        while (t < fadeToMaskDuration)
        {
            t += Time.deltaTime;

            float p = Mathf.Clamp01(t / fadeToMaskDuration);

            float radius = Mathf.Lerp(fullScreenRadius, startRadius, p);

            SetRadius(radius);
            UpdateCenter(target);

            yield return null;
        }

        SetAlpha(1f);
        SetRadius(startRadius);
        UpdateCenter(target);
    }

    public IEnumerator CloseCircleToBlack(Transform target)
    {
        float t = 0f;

        while (t < closeCircleDuration)
        {
            t += Time.deltaTime;
            float radius = Mathf.Lerp(startRadius, 0f, t / closeCircleDuration);

            SetRadius(radius);
            UpdateCenter(target);

            yield return null;
        }

        SetRadius(0f);
        SetAlpha(1f);
    }

    private void UpdateCenter(Transform target)
    {
        if (target == null || Camera.main == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

        float x = screenPos.x / Screen.width;
        float y = screenPos.y / Screen.height;

        fadeMaterial.SetVector("_Center", new Vector4(x, y, 0f, 0f));
    }

    private void SetAlpha(float alpha)
    {
        fadeMaterial.SetColor("_Color", new Color(0f, 0f, 0f, alpha));
    }

    private void SetRadius(float radius)
    {
        fadeMaterial.SetFloat("_Radius", radius);
    }
}