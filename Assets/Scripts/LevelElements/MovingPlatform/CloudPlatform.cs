using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A one-shot moving platform that sits idle until Activate() is called,
/// then moves through anchor points and fades out at the last one.
/// Designed to be added to the MovingPlatform prefab but disabled by default.
/// When enabled, auto-disables the regular MovingPlatform script.
/// </summary>
public class CloudPlatform : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("Place anchor point transforms in order. Platform moves through them and disappears at the last one.")]
    public List<Transform> anchorPoints = new();

    [Header("Speed")]
    public float moveSpeed = 3f;

    [Header("Disappear")]
    [Tooltip("Time in seconds to fade out after reaching the last anchor point.")]
    public float fadeOutDuration = 1f;

    [SerializeField] private bool debugVisualsVisible = true;

    private MovingPlatform _movingPlatform;
    private bool _activated;
    private int _currentAnchorIndex;
    private const float arriveEpsilon = 0.02f;

    private void OnValidate()
    {
        SetPointsVisibility(debugVisualsVisible);
    }

    private void SetPointsVisibility(bool visible)
    {
        if (anchorPoints == null) return;
        foreach (var point in anchorPoints)
        {
            if (point == null) continue;
            var mr = point.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = visible;
        }
    }

    private void OnEnable()
    {
        _movingPlatform = GetComponent<MovingPlatform>();
        if (_movingPlatform != null)
            _movingPlatform.enabled = false;
    }

    private void OnDisable()
    {
        if (_movingPlatform != null)
            _movingPlatform.enabled = true;
    }

    /// <summary>
    /// Wire this to ButtonPuzzle.OnPuzzleSolved (or any UnityEvent).
    /// </summary>
    public void Activate()
    {
        if (_activated) return;
        _activated = true;
        _currentAnchorIndex = 0;
    }

    private void FixedUpdate()
    {
        if (!_activated || anchorPoints == null || anchorPoints.Count == 0) return;

        if (_currentAnchorIndex >= anchorPoints.Count)
            return;

        Transform target = anchorPoints[_currentAnchorIndex];
        if (target == null) return;

        // Move only in X/Y, keep the platform's original Z
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        if (Vector3.SqrMagnitude(transform.position - targetPos) <= arriveEpsilon * arriveEpsilon)
        {
            transform.position = targetPos;
            _currentAnchorIndex++;

            // Reached the last anchor point — start fading out
            if (_currentAnchorIndex >= anchorPoints.Count)
            {
                StartCoroutine(FadeOutAndDeactivate());
            }
        }
    }

    private IEnumerator FadeOutAndDeactivate()
    {
        var renderers = GetComponentsInChildren<Renderer>();

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);

            foreach (var rend in renderers)
            {
                if (rend is SpriteRenderer sr)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
                else
                {
                    foreach (var mat in rend.materials)
                    {
                        if (mat.HasProperty("_BaseColor"))
                        {
                            Color c = mat.GetColor("_BaseColor");
                            c.a = alpha;
                            mat.SetColor("_BaseColor", c);
                        }
                    }
                }
            }

            yield return null;
        }

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (anchorPoints == null || anchorPoints.Count == 0) return;

        Gizmos.color = Color.cyan;
        Vector3 prev = transform.position;
        for (int i = 0; i < anchorPoints.Count; i++)
        {
            if (anchorPoints[i] == null) continue;
            Gizmos.DrawLine(prev, anchorPoints[i].position);
            Gizmos.DrawSphere(anchorPoints[i].position, 0.15f);
            prev = anchorPoints[i].position;
        }
    }
}
