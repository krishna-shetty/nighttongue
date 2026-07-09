using UnityEngine;

public class HealthBarRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 30f; // degrees per second
    [SerializeField] private bool _isClockwise = true;

    private RectTransform _transform;

    private void Awake()
    {
        _transform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        var dir = _isClockwise ? -1f : 1f;
        _transform.Rotate(0f, 0f, _rotationSpeed * Time.unscaledDeltaTime * dir);
    }
}
