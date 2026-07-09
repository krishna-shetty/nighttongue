using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// temporary script for flashing red on taking damage
public class DamageVisualizer : MonoBehaviour
{
    public PlayerHealth Health;
    public List<Renderer> TargetRenderers;
    public float FlashFrequency = 10f;

    private List<Color> originalColors;
    private Coroutine _currentRoutine;
    private bool _isChangedColor = false;

    void Awake()
    {
        if (!Health)
            Debug.LogError("DamageVisualizer :: Please assign the player health component.");

        if (TargetRenderers == null || TargetRenderers.Count == 0)
            TargetRenderers = new(GetComponentsInChildren<Renderer>(includeInactive: true));

        originalColors = new(TargetRenderers.Count);
        foreach (var renderer in TargetRenderers)
            originalColors.Add(renderer.material.color);
    }

    void OnEnable()
    {
        Health.OnDamage += HandleDamage;
    }

    void OnDisable()
    {
        Health.OnDamage -= HandleDamage;
    }

    void HandleDamage(GameObject target, GameObject source, bool hasKnockback)
    {
        if (_currentRoutine != null) {
            StopCoroutine(_currentRoutine);
            ResetRendererColors();
        }
        _currentRoutine = StartCoroutine(FlashRoutine(Health.GetCurrentIFrames()));
    }

    IEnumerator FlashRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float phase = Mathf.PingPong(elapsed * FlashFrequency, 1f);

            if (phase < 0.5f && !_isChangedColor) SetRendererColors(Color.red);
            else if (_isChangedColor) ResetRendererColors();

            yield return null;
        }

        ResetRendererColors();
        _currentRoutine = null;
    }

    private void SetRendererColors(Color color)
    {
        MaterialPropertyBlock block = new();
        foreach (var renderer in TargetRenderers)
        {
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color); // some renderers may use "_Color"
            renderer.SetPropertyBlock(block);
        }

        _isChangedColor = true;
    }

    private void ResetRendererColors()
    {
        MaterialPropertyBlock block = new();
        for (int i = 0; i < TargetRenderers.Count; i++)
        {
            TargetRenderers[i].GetPropertyBlock(block);
            block.SetColor("_BaseColor", originalColors[i]);
            TargetRenderers[i].SetPropertyBlock(block);
        }

        _isChangedColor = false;
    }
}
