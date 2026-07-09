using UnityEngine;

public class CameraEffectsTester : MonoBehaviour
{
    [ContextMenu("Test Small Shake")]
    void TestSmallShake() => CameraEffects.Instance.Shake(CameraShakeStrength.Small);

    [ContextMenu("Test Medium Shake")]
    void TestMediumShake() => CameraEffects.Instance.Shake(CameraShakeStrength.Medium);

    [ContextMenu("Test Large Shake")]
    void TestLargeShake() => CameraEffects.Instance.Shake(CameraShakeStrength.Large);

    [ContextMenu("Test Zoom In")]
    void TestZoomIn() => CameraEffects.Instance.Zoom(10f, 1f);

    [ContextMenu("Reset Zoom")]
    void TestResetZoom() => CameraEffects.Instance.ResetZoom(1f);
}
