using UnityEngine;

[ExecuteAlways]
public class CameraTriggerRange : MonoBehaviour
{
    [SerializeField] private float rangeWidth = 5f;

    private const float lineThickness = 0.3f;
    private const float lineHeight = 10f;
    private static readonly Color rangeColor = new Color(1f, 1f, 0f, 0.6f);

    private const float dimAlpha = 0.04f;
    private const float brightAlpha = 0.8f;
    private const float fadeSpeed = 8f;
    private const float glowRadius = 1f;

    private GameObject lineLeft;
    private GameObject lineRight;
    private Material matLeft;
    private Material matRight;
    private bool visible = true;
    private Transform player;

    public float MinX => transform.position.x - rangeWidth * 0.5f;
    public float MaxX => transform.position.x + rangeWidth * 0.5f;

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
        if (lineLeft != null) return;

        lineLeft = CreateLine("_TriggerRangeL", out matLeft);
        lineRight = CreateLine("_TriggerRangeR", out matRight);

        SyncVisual();
    }

    private GameObject CreateLine(string name, out Material lineMat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name;
        go.hideFlags = HideFlags.DontSave;

        DestroyImmediate(go.GetComponent<Collider>());

        lineMat = new Material(Shader.Find("Sprites/Default"));
        lineMat.color = rangeColor;
        lineMat.renderQueue = 3000;

        var rend = go.GetComponent<MeshRenderer>();
        rend.sharedMaterial = lineMat;
        rend.enabled = visible;

        return go;
    }

    private void SyncVisual()
    {
        if (lineLeft == null) return;
        float y = transform.position.y;
        float z = -5f;
        lineLeft.transform.position = new Vector3(MinX, y, z);
        lineLeft.transform.localScale = new Vector3(lineThickness, lineHeight, 1f);
        lineRight.transform.position = new Vector3(MaxX, y, z);
        lineRight.transform.localScale = new Vector3(lineThickness, lineHeight, 1f);
    }

    private void DestroyVisual()
    {
        DestroyMat(ref matLeft);
        DestroyMat(ref matRight);
        DestroyLine(ref lineLeft);
        DestroyLine(ref lineRight);
    }

    private void DestroyMat(ref Material m)
    {
        if (m == null) return;
        if (Application.isPlaying) Destroy(m);
        else DestroyImmediate(m);
        m = null;
    }

    private void DestroyLine(ref GameObject go)
    {
        if (go == null) return;
        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
        go = null;
    }

    private void Update()
    {
        SyncVisual();

        if (!Application.isPlaying) return;

        if (player == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        float targetLeft = dimAlpha;
        float targetRight = dimAlpha;

        if (player != null)
        {
            float px = player.position.x;
            bool insideRange = px >= MinX && px <= MaxX;
            if (insideRange)
            {
                targetLeft = brightAlpha;
                targetRight = brightAlpha;
            }
        }

        float t = 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime);
        FadeMat(matLeft, targetLeft, t);
        FadeMat(matRight, targetRight, t);
    }

    private void FadeMat(Material m, float target, float t)
    {
        if (m == null) return;
        var c = m.color;
        c.a = Mathf.Lerp(c.a, target, t);
        m.color = c;
    }

    public void SetVisible(bool v)
    {
        visible = v;
        if (lineLeft != null)
            lineLeft.GetComponent<MeshRenderer>().enabled = visible;
        if (lineRight != null)
            lineRight.GetComponent<MeshRenderer>().enabled = visible;
    }

    public void SetActive(bool active)
    {
    }

    public static Material MakeTransparent(MeshRenderer rend, Color color)
    {
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        rend.sharedMaterial = mat;
        return mat;
    }
}
