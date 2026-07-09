using UnityEngine;

[ExecuteAlways]
public class CameraConfineBound : MonoBehaviour
{
    [SerializeField] private float boundsWidth = 20f;
    [SerializeField] private float boundsHeight = 10f;

    private const float borderThickness = 0.15f;
    private static readonly Color boundColor = new Color(0f, 1f, 1f, 0.6f);

    private const float dimAlpha = 0.04f;
    private const float brightAlpha = 0.8f;
    private const float fadeSpeed = 8f;

    private GameObject[] borders = new GameObject[4]; // top, bottom, left, right
    private Material[] mats = new Material[4];
    private bool visible = true;
    private float targetAlpha = dimAlpha;

    public float MinX => transform.position.x - boundsWidth * 0.5f;
    public float MaxX => transform.position.x + boundsWidth * 0.5f;
    public float MinY => transform.position.y - boundsHeight * 0.5f;
    public float MaxY => transform.position.y + boundsHeight * 0.5f;

    private void OnEnable() { CreateVisual(); }
    private void OnDisable() { DestroyVisual(); }

    private void OnValidate()
    {
        // if (!Application.isPlaying)
        // {
        //     UnityEditor.EditorApplication.delayCall += () =>
        //     {
        //         if (this == null) return;
        //         DestroyVisual();
        //         if (isActiveAndEnabled) CreateVisual();
        //     };
        // }
    }

    private void CreateVisual()
    {
        if (borders[0] != null) return;

        for (int i = 0; i < 4; i++)
        {
            borders[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
            borders[i].name = "_ConfineBound_" + i;
            borders[i].hideFlags = HideFlags.DontSave;

            DestroyImmediate(borders[i].GetComponent<Collider>());

            var rend = borders[i].GetComponent<MeshRenderer>();
            mats[i] = CameraTriggerRange.MakeTransparent(rend, boundColor);
            mats[i].renderQueue = 3001;
            rend.enabled = visible;
        }

        SyncVisual();
    }

    private void SyncVisual()
    {
        if (borders[0] == null) return;

        float cx = transform.position.x;
        float cy = transform.position.y;
        float hw = boundsWidth * 0.5f;
        float hh = boundsHeight * 0.5f;
        const float z = -5f;

        // top
        borders[0].transform.position = new Vector3(cx, cy + hh, z);
        borders[0].transform.localScale = new Vector3(boundsWidth, borderThickness, 1f);
        // bottom
        borders[1].transform.position = new Vector3(cx, cy - hh, z);
        borders[1].transform.localScale = new Vector3(boundsWidth, borderThickness, 1f);
        // left
        borders[2].transform.position = new Vector3(cx - hw, cy, z);
        borders[2].transform.localScale = new Vector3(borderThickness, boundsHeight, 1f);
        // right
        borders[3].transform.position = new Vector3(cx + hw, cy, z);
        borders[3].transform.localScale = new Vector3(borderThickness, boundsHeight, 1f);
    }

    private void DestroyVisual()
    {
        for (int i = 0; i < 4; i++)
        {
            if (mats[i] != null)
            {
                if (Application.isPlaying) Destroy(mats[i]);
                else DestroyImmediate(mats[i]);
                mats[i] = null;
            }
            if (borders[i] != null)
            {
                if (Application.isPlaying) Destroy(borders[i]);
                else DestroyImmediate(borders[i]);
                borders[i] = null;
            }
        }
    }

    private void Update()
    {
        SyncVisual();

        if (mats[0] == null || !Application.isPlaying) return;
        float t = 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime);
        for (int i = 0; i < 4; i++)
        {
            var c = mats[i].color;
            c.a = Mathf.Lerp(c.a, targetAlpha, t);
            mats[i].color = c;
        }
    }

    public void SetActive(bool active)
    {
        targetAlpha = active ? brightAlpha : dimAlpha;
    }

    public void SetVisible(bool v)
    {
        visible = v;
        for (int i = 0; i < 4; i++)
        {
            if (borders[i] != null)
                borders[i].GetComponent<MeshRenderer>().enabled = visible;
        }
    }
}
