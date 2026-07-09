using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraZoomTrigger : MonoBehaviour
{
    [SerializeField] private float zoomInFOV = 40f;
    [SerializeField] private float zoomDuration = 1f;
    [SerializeField] private float zoomOutDuration = 1f;

    private bool isZoomed = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("enter trigger");
        if (other.CompareTag("PlayerModel") && !isZoomed)
        {
            isZoomed = true;
            if (CameraEffects.Instance != null)
                CameraEffects.Instance.Zoom(zoomInFOV, zoomDuration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerModel") && isZoomed)
        {
            isZoomed = false;
            if (CameraEffects.Instance != null)
                CameraEffects.Instance.ResetZoom(zoomOutDuration);
        }
    }
}
