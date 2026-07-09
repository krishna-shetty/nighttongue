using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[System.Serializable]
public class ConfineZone
{
    public CameraTriggerRange triggerRange;
    public CameraConfineBound confineBounds;
}

[ExecuteAlways]
public class CameraConfineSwitcher : MonoBehaviour
{
    [SerializeField] private List<ConfineZone> zones = new List<ConfineZone>();

    [Tooltip("How fast the camera glides to the new confine position.")]
    [SerializeField] private float transitionSpeed = 5f;

    [SerializeField] private bool showVisuals = true;


    private Transform player;

    private float defaultMinX;
    private float defaultMaxX;
    private float defaultMinY;
    private float defaultMaxY;

    private struct ZoneData
    {
        public CameraTriggerRange triggerRange;
        public CameraConfineBound confineBound;
        public bool isActive;
    }
    private List<ZoneData> zoneData = new List<ZoneData>();

    private float activeMinX;
    private float activeMaxX;
    private float activeMinY;
    private float activeMaxY;

    private float smoothOffsetX;
    private float smoothOffsetY;
    private float prevActiveMinX;
    private float prevActiveMaxX;
    private float prevActiveMinY;
    private float prevActiveMaxY;
    private bool zoneJustSwitched;

    private GameObject debugDot;
    private bool wasShowVisuals;

    private Vector3 lastCameraPos;
    private CinemachineBrain brain;

    private void Start()
    {
        wasShowVisuals = showVisuals;
        ApplyVisibility();

        if (!Application.isPlaying) return;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        var confiners = FindObjectsByType<CinemachineConfiner3D>(FindObjectsSortMode.None);

        Collider existingDefault = null;
        if (confiners.Length > 0 && confiners[0].BoundingVolume != null)
            existingDefault = confiners[0].BoundingVolume;

        foreach (var c in confiners)
            c.enabled = false;

        var cinemachineCams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        foreach (var cam in cinemachineCams)
        {
            var ext = cam.gameObject.AddComponent<CameraConfineExtension>();
            ext.switcher = this;
        }

        if (existingDefault != null)
        {
            var b = existingDefault.bounds;
            defaultMinX = b.min.x;
            defaultMaxX = b.max.x;
            defaultMinY = b.min.y;
            defaultMaxY = b.max.y;
        }
        else
        {
            defaultMinX = -50000f;
            defaultMaxX = 50000f;
            defaultMinY = -50000f;
            defaultMaxY = 50000f;
        }

        foreach (var zone in zones)
        {
            if (zone.triggerRange == null || zone.confineBounds == null)
                continue;

            zoneData.Add(new ZoneData
            {
                triggerRange = zone.triggerRange,
                confineBound = zone.confineBounds,
                isActive = false
            });
        }

        activeMinX = defaultMinX;
        activeMaxX = defaultMaxX;
        activeMinY = defaultMinY;
        activeMaxY = defaultMaxY;
        prevActiveMinX = activeMinX;
        prevActiveMaxX = activeMaxX;
        prevActiveMinY = activeMinY;
        prevActiveMaxY = activeMaxY;

        brain = FindObjectsByType<CinemachineBrain>(FindObjectsSortMode.None)[0];

        debugDot = new GameObject("_CameraXDot");
        var sr = debugDot.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCameraSprite();
        sr.sortingOrder = 9999;
        sr.enabled = showVisuals;

        // Evaluate zones immediately so the camera starts at the player's position
        if (player != null)
        {
            float playerX = player.position.x;
            for (int i = 0; i < zoneData.Count; i++)
            {
                var z = zoneData[i];
                bool inside = playerX >= z.triggerRange.MinX && playerX <= z.triggerRange.MaxX;
                if (inside)
                {
                    z.isActive = true;
                    z.triggerRange.SetActive(true);
                    z.confineBound.SetActive(true);
                    zoneData[i] = z;
                }
            }
            RecalculateBounds();
            prevActiveMinX = activeMinX;
            prevActiveMaxX = activeMaxX;
            prevActiveMinY = activeMinY;
            prevActiveMaxY = activeMaxY;
        }
    }

    private void Update()
    {
        if (showVisuals != wasShowVisuals)
        {
            wasShowVisuals = showVisuals;
            ApplyVisibility();
        }

        if (!Application.isPlaying || player == null)
            return;

        float playerX = player.position.x;
        bool anyChanged = false;

        const float hysteresis = 0.15f;

        for (int i = 0; i < zoneData.Count; i++)
        {
            var z = zoneData[i];
            float minX = z.triggerRange.MinX;
            float maxX = z.triggerRange.MaxX;

            bool insideRange = z.isActive
                ? playerX >= minX - hysteresis && playerX <= maxX + hysteresis
                : playerX >= minX && playerX <= maxX;

            if (insideRange == z.isActive)
                continue;

            z.isActive = insideRange;
            z.triggerRange.SetActive(z.isActive);
            z.confineBound.SetActive(z.isActive);
            zoneData[i] = z;
            anyChanged = true;
        }

        if (anyChanged)
        {
            prevActiveMinX = activeMinX;
            prevActiveMaxX = activeMaxX;
            prevActiveMinY = activeMinY;
            prevActiveMaxY = activeMaxY;
            RecalculateBounds();
            zoneJustSwitched = true;
        }
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying) return;

        if (Mathf.Abs(smoothOffsetX) > 0.001f)
            smoothOffsetX = Mathf.Lerp(smoothOffsetX, 0f, 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime));
        else
            smoothOffsetX = 0f;

        if (Mathf.Abs(smoothOffsetY) > 0.001f)
            smoothOffsetY = Mathf.Lerp(smoothOffsetY, 0f, 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime));
        else
            smoothOffsetY = 0f;

        if (debugDot != null)
            debugDot.transform.position = new Vector3(lastCameraPos.x, lastCameraPos.y, -10);
    }

    private void ApplyVisibility()
    {
        foreach (var zone in zones)
        {
            if (zone.triggerRange != null)
                zone.triggerRange.SetVisible(showVisuals);
            if (zone.confineBounds != null)
                zone.confineBounds.SetVisible(showVisuals);
        }

        if (debugDot != null)
            debugDot.GetComponent<MeshRenderer>().enabled = showVisuals;
    }

    private void RecalculateBounds()
    {
        activeMinX = defaultMinX;
        activeMaxX = defaultMaxX;
        activeMinY = defaultMinY;
        activeMaxY = defaultMaxY;

        bool anyActive = false;
        for (int i = 0; i < zoneData.Count; i++)
        {
            if (!zoneData[i].isActive)
                continue;

            if (!anyActive)
            {
                activeMinX = zoneData[i].confineBound.MinX;
                activeMaxX = zoneData[i].confineBound.MaxX;
                activeMinY = zoneData[i].confineBound.MinY;
                activeMaxY = zoneData[i].confineBound.MaxY;
                anyActive = true;
            }
            else
            {
                activeMinX = Mathf.Max(activeMinX, zoneData[i].confineBound.MinX);
                activeMaxX = Mathf.Min(activeMaxX, zoneData[i].confineBound.MaxX);
                activeMinY = Mathf.Max(activeMinY, zoneData[i].confineBound.MinY);
                activeMaxY = Mathf.Min(activeMaxY, zoneData[i].confineBound.MaxY);
            }
        }
    }

    public Vector3 ClampPosition(Vector3 rawPos, CinemachineVirtualCameraBase vcam)
    {
        float clampedX = Mathf.Clamp(rawPos.x, activeMinX, activeMaxX);
        float clampedY = Mathf.Clamp(rawPos.y, activeMinY, activeMaxY);

        if (zoneJustSwitched)
        {
            zoneJustSwitched = false;
            smoothOffsetX = lastCameraPos.x - clampedX;
            smoothOffsetY = lastCameraPos.y - clampedY;
        }

        float finalX = clampedX + smoothOffsetX;
        float finalY = clampedY + smoothOffsetY;

        if (brain != null && brain.ActiveVirtualCamera as CinemachineVirtualCameraBase == vcam)
            lastCameraPos = new Vector3(finalX, finalY, rawPos.z);

        return new Vector3(finalX, finalY, rawPos.z);
    }

    private static Sprite CreateCameraSprite()
    {
        // 32x32 camera icon bitmask (rasterized from SVG, row 0 = bottom)
        uint[] rows = {
            0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
            0x0FFFFFF0,0x1FFFFFF8,0x1FFFFFF8,0x1FFFFFF8,0x1FFFFFF8,
            0x1FFFFFF8,0x1FFC3FF8,0x1FF81FF8,0x1FF00FF8,0x1FF00FF8,
            0x1FE007F8,0x1FF00FF8,0x1FF00FF8,0x1FF81FF8,0x1FFC3FF8,
            0x1FFFFFF8,0x1FFFFFF8,0x1FFFFFF8,0x1FFFFFF8,0x0FFFFFF0,
            0x003FFC00,0x003FFC00,0x001FF800,0x00000000,0x00000000,
            0x00000000,0x00000000,
        };

        int w = 32, h = 32;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[w * h];
        var fill = Color.red;
        var clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            uint r = rows[y];
            for (int x = 0; x < w; x++)
                pixels[y * w + x] = ((r >> (31 - x)) & 1) == 1 ? fill : clear;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 10.67f);
    }
}

public class CameraConfineExtension : CinemachineExtension
{
    [HideInInspector] public CameraConfineSwitcher switcher;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body || switcher == null)
            return;

        state.RawPosition = switcher.ClampPosition(state.RawPosition, vcam);
    }
}