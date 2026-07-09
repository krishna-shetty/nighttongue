using UnityEngine;

public class TongueStretch : MonoBehaviour
{
    private Transform joint;
    private float scaleTarget = 1f;
    private float targetLength = 2f;
    [SerializeField]private TongueController controller;
    [SerializeField]private Transform TamiTarget;
    
    [SerializeField] private Transform TIP;

    private float baseLength = 2.85f;
    public enum Axis { X, Y, Z }
    public Axis stretchAxis = Axis.X;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(joint == null) joint = transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
       /* bool aiming = controller != null && controller.IsAimingActive;
        bool left = Input.GetMouseButton(0);
        bool right = Input.GetMouseButton(1);
        if (!(aiming && (left || right))) return;*/
           targetLength = controller.IsAttached ? Vector3.Distance(joint.position, controller.EndPoint) 
                : Vector3.Distance(joint.position, TamiTarget.position);   
           scaleTarget = targetLength / baseLength;
           var s = joint.localScale;     
           s.x = scaleTarget;  
           joint.localScale = s;
           targetLength+=Vector3.Distance(TIP.position, controller.EndPoint);
           scaleTarget = targetLength / baseLength;
           s = joint.localScale;
           s.x = scaleTarget;
           joint.localScale = s;


    }
}
