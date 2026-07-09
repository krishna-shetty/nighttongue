using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

public class TweenAnimations : MonoBehaviour
{
    [Header("Text Expansion")]
    public float ExpandDuration = 0.2f;
    public float ExpandScale = 1.2f;
    [Tooltip("For dynamic animation speeds. Curves can be viewed at https://easings.net/.")]
    public Ease ExpandEaseType = Ease.OutElastic;

    [Header("Text Fade")]
    public float FadeDuration = 0.3f;
    public Ease FadeEaseType = Ease.OutQuad;

    [Header("Moving Tami Cursor")]
    public GameObject Cursor;
    public float MoveDuration = 0.25f;
    [Tooltip("For dynamic animation speeds. Curves can be viewed at https://easings.net/.")]
    public Ease MoveEaseType = Ease.OutQuad;

    private RectTransform _cursorT;
    private Dictionary<Transform, Vector3> _originalScale;
    private Dictionary<Transform, Tween> _activeTweens;

    void Start()
    {
        DOTween.Init();
        _originalScale = new();
        _activeTweens = new();
        if (Cursor) _cursorT = Cursor.GetComponent<RectTransform>();
    }

    public void ExpandText(Transform t)
    {
        _originalScale.TryAdd(t, t.localScale);
        var tween = t.DOScale(_originalScale[t] * ExpandScale, ExpandDuration).SetEase(ExpandEaseType).SetUpdate(true);
        _activeTweens.TryAdd(t, tween);
        tween.OnComplete(() => { _activeTweens.Remove(t); });
    }

    public void ShrinkText(Transform t)
    {
        if (!_originalScale.ContainsKey(t))
        {
            Debug.Log("TweenAnimations :: ShrinkText called on a transform that was never expanded. Did you mean to do that?");
            t.localScale /= ExpandScale;
            return;
        }
        KillActiveTween(t);
        t.localScale = _originalScale[t];
    }

    public void FadeText(TextMeshProUGUI text)
    {
        Debug.Log($"FadeText called on {text.name}");
        text.DOKill();
        Color targetColor = new Color32(0xF1, 0x9F, 0x9D, 0xFF);
        DOTween.To(() => text.color, x => text.color = x, targetColor, FadeDuration)
            .SetEase(FadeEaseType)
            .SetUpdate(true);
    }

    public void RevertFadeText(TextMeshProUGUI text)
    {
        Debug.Log($"RevertFadeText called on {text.name}");
        text.DOKill();
        Color revertColor = new Color32(0xEA, 0xDD, 0xDB, 0xFF);
        DOTween.To(() => text.color, x => text.color = x, revertColor, FadeDuration)
            .SetEase(FadeEaseType)
            .SetUpdate(true);
    }

    public void MoveCursor(RectTransform target)
    {
        if (!_cursorT) return;
        KillActiveTween(_cursorT);
        _cursorT.DOAnchorPos(target.anchoredPosition, MoveDuration).SetEase(MoveEaseType).SetUpdate(true);
    }

    private void KillActiveTween(Transform t)
    {
        if (_activeTweens.TryGetValue(t, out Tween tween))
        {
            tween.Kill();
            _activeTweens.Remove(t);
        }
    }
}