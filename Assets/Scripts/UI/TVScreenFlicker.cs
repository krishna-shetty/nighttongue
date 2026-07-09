using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITVAbruptFlicker : MonoBehaviour
{
    private Image img;
    private Vector3 originalScale;

    [Header("1. Flicker Intensity (Alpha)")]
    [Range(0, 1)][SerializeField] private float minAlpha = 0.15f;
    [Range(0, 1)][SerializeField] private float maxAlpha = 0.8f;

    [Header("2. Timing (The 'Snap' Secret)")]
    [SerializeField] private float minHoldTime = 0.02f; // Micro-stutter
    [SerializeField] private float maxHoldTime = 0.3f;  // Longer scene hold

    [Header("3. Color Palette")]
    [SerializeField]
    private Color[] tvColors = new Color[]
    {
        new Color(0.7f, 0.85f, 1.0f), // Cool Blue
        new Color(1.0f, 0.95f, 0.8f), // Warm White
        new Color(0.6f, 0.7f, 1.0f),  // Deep Blue
        new Color(0.1f, 0.1f, 0.1f)   // "Black" screen (flicker out)
    };

    void Start()
    {
        img = GetComponent<Image>();
        originalScale = transform.localScale;
        if (img != null) RunFlicker();
    }

    void RunFlicker()
    {
        if (img == null) return;

        // 1. Pick a random color and alpha
        Color nextColor = tvColors[Random.Range(0, tvColors.Length)];
        nextColor.a = Random.Range(minAlpha, maxAlpha);

        // 2. ABRUPT CHANGE: We don't use a duration for the color. 
        // We set it instantly to mimic a camera cut on the TV.
        img.color = nextColor;

        // 3. SCALE BLOOM: We keep a tiny bit of smoothing here 
        // so the 'light' feels like it's traveling through air.
        float targetScale = Random.Range(1f, 1.5f);
        transform.DOScale(originalScale * targetScale, 0.05f).SetEase(Ease.OutFlash);

        // 4. RANDOM WAIT: Use a random delay before the next "cut"
        float waitTime = Random.Range(minHoldTime, maxHoldTime);

        // This 'Timer' creates the abrupt rhythm
        DOVirtual.DelayedCall(waitTime, () => RunFlicker());
    }

    private void OnDestroy()
    {
        transform.DOKill();
        // DOTween's DelayedCalls should also be killed if the object is destroyed
        DOTween.Kill(this);
    }
}