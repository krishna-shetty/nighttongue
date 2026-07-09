using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ParallaxEffect : MonoBehaviour
{
    [Range(-1f, 1f), Tooltip("How much the object moves relative to the camera (0 = doesn't move, 1 = moves with camera). Recommended to not exceed 0.2")]
    public float MoveFactor = 0f;

    private CinemachineBrain _brain;
    private Transform _camera;
    private Vector3 _camStart;
    private Vector3 _objStart;

    void OnEnable()
    {
        if (!_brain) _brain = Camera.main.GetComponent<CinemachineBrain>();
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    void Awake()
    {
        if (!_brain) _brain = Camera.main.GetComponent<CinemachineBrain>();
        _camStart = _brain.OutputCamera.transform.position;
        _objStart = transform.position;
    }

    void OnCameraUpdated(CinemachineBrain brain)
    {
        var offset = brain.OutputCamera.transform.position - _camStart;
        var x = offset.x * MoveFactor;
        var y = offset.y * MoveFactor;
        transform.position = new Vector3(_objStart.x + x, _objStart.y + y, transform.position.z);
    }
}
