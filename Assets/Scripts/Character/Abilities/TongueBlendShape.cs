using UnityEngine;
using System.Collections;


public class TongueBlendShape : MonoBehaviour
{
    public SkinnedMeshRenderer tongueMeshRenderer;
    public int blendShapeIndex = 0;
    public SwingAbilitySO ability;
    public TongueController tongue;
    public string shapeName = "stretch.mid_stretch";
   /* float t = 0f;
    
    bool isPlayBlendShape = false;
    float u = 0f;*/
    int idx = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!tongueMeshRenderer) tongueMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        Debug.Log("Tongue Mesh Renderer: " + tongueMeshRenderer);
        if (tongueMeshRenderer && tongueMeshRenderer.sharedMesh) idx = tongueMeshRenderer.sharedMesh.GetBlendShapeIndex(shapeName);
        Debug.Log("Blend Shape Index for " + shapeName + ": " + idx);
    }

    private void OnEnable()
    {
        if (tongue != null)
        {
            tongue.OnTongueExtension += HandleTongueExtend;
            tongue.OnTongueRetraction += HandleTongueRetract;
        }
    }

    private void OnDisable()
    {
        if (tongue != null)
        {
            tongue.OnTongueExtension -= HandleTongueExtend;
            tongue.OnTongueRetraction -= HandleTongueRetract;
        }
    }

    private IEnumerator BlendShapeRoutine(float start, float end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = Mathf.Lerp(start, end, t);

            tongueMeshRenderer.SetBlendShapeWeight(idx, value);

            yield return null;
        }

        tongueMeshRenderer.SetBlendShapeWeight(idx, end);
    }


    private void HandleTongueExtend(Vector3 target, float duration)
    {
        StartCoroutine(BlendShapeRoutine(0f, 100f, duration));


    }
    
    private void HandleTongueRetract(float duration)
    {
        StartCoroutine(BlendShapeRoutine(100f, 0f, duration));

    }
}
