using UnityEngine;
using DG.Tweening; // Ensure DOTween is installed

public class SpriteMasterAnimator : MonoBehaviour
{
    [Header("1. Fade Settings")]
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 0.9f; // Slower is usually better for "breathing"
    [Range(0, 1)][SerializeField] private float minAlpha = 0f;
    [SerializeField] private Ease fadeEase = Ease.InOutCubic; // The "In and Out" secret

    [Header("2. Pendulum Settings")]
    [SerializeField] private bool useSwing = false;
    [SerializeField] private float swingAngle = 30f;
    [SerializeField] private float swingDuration = 2f;

    [Header("3. Vertical Bob Settings")]
    [SerializeField] private bool useBob = false;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobDuration = 1.5f;

    [Header("4. Lean/Stretch Settings")]
    [SerializeField] private bool useLean = false;
    [SerializeField] private float leanAmount = 15f;
    [SerializeField] private float leanDuration = 1.2f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (useFade) StartFade();
        if (useSwing) StartSwing();
        if (useBob) StartBob();
        if (useLean) StartLean();
    }

    void StartFade()
    {
        // Fades from current alpha to minAlpha and back
        sr.DOFade(minAlpha, fadeDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StartSwing()
    {
        // Set initial rotation to one side
        transform.localRotation = Quaternion.Euler(0, 0, -swingAngle);

        // Swing to the other side
        transform.DOLocalRotate(new Vector3(0, 0, swingAngle), swingDuration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StartBob()
    {
        // Move up and down relative to start position
        transform.DOMoveY(transform.position.y + bobHeight, bobDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StartLean()
    {
        // To simulate the parallelogram without vertex editing, 
        // we rotate and slightly offset horizontally.
        // Works best if the Pivot is set to "Bottom" in Sprite Editor.
        transform.DOLocalRotate(new Vector3(0, 0, leanAmount), leanDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        // Clean up tweens when the object is destroyed to prevent memory leaks
        transform.DOKill();
        if (sr != null) sr.DOKill();
    }
}