using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public enum CameraShakeStrength { Small, Medium, Large }

public class CameraEffects : MonoBehaviour
{
    public static CameraEffects Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CinemachineCamera activeCamera;

    [Header("Shake Settings")]
    [SerializeField] private float smallShake = 0.5f;
    [SerializeField] private float mediumShake = 1f;
    [SerializeField] private float largeShake = 2f;
    [SerializeField] private float shakeDuration = 1f;

    [SerializeField] private CameraShakeStrength _onPlayerDamageShake = CameraShakeStrength.Small;

    private float originalFOV;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void OnEnable()
    {
        DamageReceiver.OnAnyDamageTaken += HandleGlobalDamage;
    }

    private void OnDisable()
    {
        DamageReceiver.OnAnyDamageTaken -= HandleGlobalDamage;
    }

    private void HandleGlobalDamage(int damage, GameObject source)
    {
        if (source.CompareTag("Player"))
        {
            Shake(_onPlayerDamageShake);
        }
    }

    private void Start()
    {
        if (activeCamera != null)
            originalFOV = activeCamera.Lens.FieldOfView;
    }

    // --- Shake ---
    public void Shake(CameraShakeStrength strength)
    {
        float amplitude = strength switch
        {
            CameraShakeStrength.Small => smallShake,
            CameraShakeStrength.Medium => mediumShake,
            CameraShakeStrength.Large => largeShake,
            _ => 1f
        };

        StartCoroutine(DoShake(amplitude));
    }

    private IEnumerator DoShake(float amplitude)
    {
        var noise = activeCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null)
        {
            Debug.LogWarning("No Perlin Noise on camera for shake!");
            yield break;
        }

        noise.AmplitudeGain = amplitude;
        yield return new WaitForSeconds(shakeDuration);
        noise.AmplitudeGain = 0;
    }

    // --- Zoom ---
    public void Zoom(float targetFOV, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DoZoom(targetFOV, duration));
    }

    private IEnumerator DoZoom(float targetFOV, float duration)
    {
        float startFOV = activeCamera.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Use SmoothStep for smoother ease-in/out
            t = t * t * (3f - 2f * t);
            activeCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }

        // Ensure exact final value
        activeCamera.Lens.FieldOfView = targetFOV;
    }


    public void ResetZoom(float duration = 2f)
    {
        Zoom(originalFOV, duration);
    }

    public void SetActiveCam(CinemachineCamera cam)
    {
        activeCamera = cam;
        originalFOV = activeCamera.Lens.FieldOfView;
    }
}
