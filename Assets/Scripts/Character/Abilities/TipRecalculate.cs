using UnityEngine;
using UnityEngine.Animations.Rigging;

public class TipRecalculate : MonoBehaviour
{
    [SerializeField] private Transform lowTongue;
    [SerializeField] private GameObject rig;
    [SerializeField] private TongueController controller;
    private RigBuilder rigbuilder;
    private Vector3 baseLocalScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        baseLocalScale = transform.localScale;
        rigbuilder = rig.GetComponent<RigBuilder>();

    }
    
    void LateUpdate()
    {
        if(controller == null || rigbuilder == null) return;
        if (lowTongue == null) return;

        if (controller.IsAttached)
        {
            rigbuilder.enabled = false;
            float parentX = lowTongue.localScale.x;
            //Debug.Log($"parentX={parentX}");
            rigbuilder.enabled = true;
            //if (Mathf.Abs(parentX) < 1e-6f) parentX = 1f;
            Vector3 s = baseLocalScale;
            s.x = baseLocalScale.x / parentX;
            transform.localScale = s;
            //Debug.Log($"tip local={transform.localScale}  lossy={transform.lossyScale}");
            //Debug.Log($"parent local={lowTongue.localScale}  lossy={lowTongue.lossyScale}");
        }



    }
}
