using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DissolveController : MonoBehaviour
{
    [Header("Material")]
    public Material dissolveMat;
    public RawImage targetImage;

    [Header("Settings")]
    public float dissolveTime = 1f;

    private Coroutine routine;
    private static readonly int DissolveStrengthID = Shader.PropertyToID("_DissolveStrength");

    private void Awake()
    {
        if (dissolveMat != null)
        {
            dissolveMat = Instantiate(dissolveMat);
        }

        if (targetImage != null)
        {
            targetImage.material = dissolveMat;
        }

        ResetDissolve();
    }

    public void StartDissolve(Action onComplete = null)
    {
        if (dissolveMat == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (routine != null)
        {
            StopCoroutine(routine);
        }

        ResetDissolve();
        routine = StartCoroutine(DissolveRoutine(onComplete));
    }

    private IEnumerator DissolveRoutine(Action onComplete)
    {
        float t = 0f;

        while (t < dissolveTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / dissolveTime);

            dissolveMat.SetFloat(DissolveStrengthID, progress);
            yield return null;
        }

        dissolveMat.SetFloat(DissolveStrengthID, 1f);
        routine = null;
        onComplete?.Invoke();
    }

    public void ResetDissolve()
    {
        if (dissolveMat != null)
        {
            dissolveMat.SetFloat(DissolveStrengthID, 0f);
        }
    }

    private void OnDestroy()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}