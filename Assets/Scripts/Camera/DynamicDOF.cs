using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicDOF : MonoBehaviour
{
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private CinemachineCamera activeCamera;

    private DepthOfField dof;
    private CinemachinePositionComposer composer;

    void Start()
    {
        if (postProcessVolume.profile.TryGet(out dof))
            dof.active = true;

        if (activeCamera != null)
            composer = activeCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;
    }

    void LateUpdate()
    {
        if (dof == null || composer == null) return;

        float targetDistance = Mathf.Max(0.1f, composer.CameraDistance - 10f);
        float currentDistance = dof.focusDistance.value;

        // Smoothly interpolate toward target
        float smoothSpeed = 5f; // increase for faster focus tracking
        float newDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothSpeed);

        dof.focusDistance.Override(newDistance);
    }


    public void SetActiveCamera(CinemachineCamera cam)
    {
        activeCamera = cam;
        composer = cam != null ? cam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer : null;
    }
}
