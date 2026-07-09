using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[System.Serializable]
public class DepthZone
{
    public CameraTriggerRange triggerRange;

    [Tooltip("How much to change the orthographic size when this zone is active. Negative = zoom in (closer), positive = zoom out (further).")]
    public float orthoSizeDelta;

    [Tooltip("How much to shift the camera in Y when this zone is active.")]
    public float yDelta;
}

public class CameraDepthSwitcher : MonoBehaviour
{
    [SerializeField] private List<DepthZone> zones = new();

    [Tooltip("How fast the camera transitions to the new ortho size.")]
    [SerializeField] private float transitionSpeed = 5f;

    [SerializeField] private bool showVisuals = true;

    private Transform player;
    private float smoothOrtho;
    private float targetOrtho;
    private float smoothY;
    private float targetY;
    private bool wasShowVisuals;

    private CinemachineCamera capsuleCam;
    private CinemachineCamera ballCam;
    private float capsuleBaseOrtho;
    private float ballBaseOrtho;

    private Camera uiCamera;
    private float uiBaseOrtho;

    private struct ZoneData
    {
        public CameraTriggerRange triggerRange;
        public float orthoSizeDelta;
        public float yDelta;
        public bool isActive;
    }
    private readonly List<ZoneData> zoneData = new();

    private const float hysteresis = 0.15f;

    private void OnValidate()
    {
        ApplyVisibility();
    }

    private void Start()
    {
        wasShowVisuals = showVisuals;
        ApplyVisibility();

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("[CameraDepthSwitcher] No Player found!");

        foreach (var zone in zones)
        {
            if (zone.triggerRange == null) continue;

            zoneData.Add(new ZoneData
            {
                triggerRange = zone.triggerRange,
                orthoSizeDelta = zone.orthoSizeDelta,
                yDelta = zone.yDelta,
                isActive = false
            });
        }

        var camSwitcher = FindObjectsByType<CM3Switcher>(FindObjectsSortMode.None);
        if (camSwitcher.Length > 0)
        {
            var cs = camSwitcher[0];
            if (cs.CapsuleCam != null)
            {
                capsuleCam = cs.CapsuleCam;
                capsuleBaseOrtho = capsuleCam.Lens.OrthographicSize;
                var ext = capsuleCam.gameObject.AddComponent<CameraDepthExtension>();
                ext.switcher = this;
            }
            if (cs.BallCam != null)
            {
                ballCam = cs.BallCam;
                ballBaseOrtho = ballCam.Lens.OrthographicSize;
                var ext = ballCam.gameObject.AddComponent<CameraDepthExtension>();
                ext.switcher = this;
            }
        }
        else
        {
            Debug.LogWarning("[CameraDepthSwitcher] No CM3Switcher found — no cameras to attach to.");
        }

        var uiCamObj = GameObject.Find("UI Camera");
        if (uiCamObj != null)
        {
            uiCamera = uiCamObj.GetComponent<Camera>();
            if (uiCamera != null)
                uiBaseOrtho = uiCamera.orthographicSize;
        }
    }

    public float GetOrthoSizeDelta() => smoothOrtho;
    public float GetYDelta() => smoothY;

    private void Update()
    {
        if (showVisuals != wasShowVisuals)
        {
            wasShowVisuals = showVisuals;
            ApplyVisibility();
        }

        if (player == null) return;

        float playerX = player.position.x;

        for (int i = 0; i < zoneData.Count; i++)
        {
            var z = zoneData[i];
            float minX = z.triggerRange.MinX;
            float maxX = z.triggerRange.MaxX;

            bool insideRange = z.isActive
                ? playerX >= minX - hysteresis && playerX <= maxX + hysteresis
                : playerX >= minX && playerX <= maxX;

            if (insideRange != z.isActive)
            {
                z.isActive = insideRange;
                z.triggerRange.SetActive(z.isActive);
                zoneData[i] = z;
            }
        }

        // Use the last active zone's deltas (or 0 if none active)
        float newOrtho = 0f;
        float newY = 0f;
        for (int i = 0; i < zoneData.Count; i++)
        {
            if (zoneData[i].isActive)
            {
                newOrtho = zoneData[i].orthoSizeDelta;
                newY = zoneData[i].yDelta;
            }
        }
        targetOrtho = newOrtho;
        targetY = newY;

        // Smooth transition
        float t = transitionSpeed <= 0f ? 1f : 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime);
        smoothOrtho = Mathf.Abs(smoothOrtho - targetOrtho) > 0.001f
            ? Mathf.Lerp(smoothOrtho, targetOrtho, t) : targetOrtho;
        smoothY = Mathf.Abs(smoothY - targetY) > 0.001f
            ? Mathf.Lerp(smoothY, targetY, t) : targetY;

        // Apply ortho size directly to the CinemachineCamera Lens (same approach as CameraEffects.Zoom)
        if (capsuleCam != null)
            capsuleCam.Lens.OrthographicSize = capsuleBaseOrtho + smoothOrtho;
        if (ballCam != null)
            ballCam.Lens.OrthographicSize = ballBaseOrtho + smoothOrtho;

        // Scale UI camera proportionally to the main camera
        if (uiCamera != null && capsuleBaseOrtho > 0f)
        {
            float ratio = (capsuleBaseOrtho + smoothOrtho) / capsuleBaseOrtho;
            uiCamera.orthographicSize = uiBaseOrtho * ratio;
        }

    }

    private void ApplyVisibility()
    {
        foreach (var zone in zones)
        {
            if (zone.triggerRange != null)
                zone.triggerRange.SetVisible(showVisuals);
        }
    }
}

public class CameraDepthExtension : CinemachineExtension
{
    [HideInInspector] public CameraDepthSwitcher switcher;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body || switcher == null)
            return;

        float yDelta = switcher.GetYDelta();
        if (Mathf.Abs(yDelta) > 0.001f)
            state.PositionCorrection += new Vector3(0, yDelta, 0);
    }
}
