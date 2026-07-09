using UnityEngine;
using System;

public class PlayerCollisionHandler : MonoBehaviour
{
    private CollideSlideCharacterCollisionResolver _collideResolver;
    public event Action OnCollisionDetected;
    [SerializeField] private int _maxCollideAndSlideDepth = 5;
    [SerializeField] private float _maxSlopeAngle = 55f; // Maximum slope angle to consider for sliding
    [SerializeField] private LayerMask _layer;
    [SerializeField] private GlobalPhysicsSettingsSO _settings;
    [SerializeField] private float _skinWidth = 0.02f; // Default skin width
    [SerializeField] private Vector3 _point1 = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 _point2 = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private float _capsuleRadius = 0.5f;
    [SerializeField] private float _sphereRadius = 0.6812f;
    private float _capsuleHeight;
    [SerializeField] private GameObject _sphereObject, _capsuleObject;
    private TongueTransformHandler _tongueTransformHandler;
    private bool _isTransformed = false;
    private CharacterController _collisionController;

    /// <summary>
    /// </summary>

    private void Awake()
    {
        if (_settings != null)
        {
            _skinWidth = _settings.SkinWidth;
        }
        else
        {
            Debug.LogWarning($"GlobalPhysicsSettings not assigned! Using default skin width of {_skinWidth}");
        }
        _capsuleRadius -= _skinWidth; // Adjust radius to account for skin width
    }

    private void Start()
    {
        if (_tongueTransformHandler == null)
        {
            Debug.LogWarning("TongueTransformHandler component not found on PlayerController.");
            return;
        }
        (float, float) sphereSize = MeshSize.GetSize(_sphereObject);
        _sphereRadius = sphereSize.Item1 / 2;
        _collisionController = GetComponent<CharacterController>();
        UpdateCapsuleFromTransform();
    }

    private void OnEnable()
    {
        _tongueTransformHandler = GetComponent<TongueTransformHandler>();
        if (_tongueTransformHandler != null)
        {
            _tongueTransformHandler.OnTransformStateChanged += HandleTransformStateChange;
        }
    }

    private void OnDisable()
    {
        if (_tongueTransformHandler != null)
        {
            _tongueTransformHandler.OnTransformStateChanged -= HandleTransformStateChange;
        }
    }

    private void UpdateCapsuleFromTransform()
    {
        _capsuleHeight = MeshSize.GetSize(_capsuleObject).Item2;
        Vector3 up = Vector3.up * (_capsuleHeight * 0.5f);
        _point1 = -up;
        _point2 = up;
        float radius = MeshSize.GetSize(_capsuleObject).Item1 * 0.5f;
        _capsuleRadius = radius - _skinWidth;
    }

    public void ToggleTransform()
    {
        if (_isTransformed)
        {
            _collisionController.height = 0;
            _collisionController.radius = _sphereRadius;
        }
        else
        {
            _collisionController.height = _capsuleHeight;
            _collisionController.radius = _capsuleRadius;
        }
    }

    private void UpdateChangePosition()
    {
        if (_isTransformed)
        {
            // capsule to sphere
            float newYPos = transform.position.y - _capsuleHeight / 2 + _sphereRadius;
            Vector3 newPosition = transform.position;
            newPosition.y = newYPos;
            transform.position = newPosition;
        }
        else
        {
            // sphere to capsule
            float newYPos = transform.position.y - _sphereRadius + _capsuleHeight / 2;
            Debug.Log(newYPos);
            Vector3 newPosition = transform.position;
            newPosition.y = newYPos;
            transform.position = newPosition; 
        }
    }

    private void HandleTransformStateChange(TongueTransformEventArgs args)
    {
        _isTransformed = args.IsTransformed;
        ToggleTransform();
        UpdateChangePosition();
    }

    public void NotifyCollision()
    {
        OnCollisionDetected?.Invoke();
    }

    private bool CapsuleSpaceCheck()
    {
        Vector3 EndSphere1 = transform.position;
        EndSphere1.y = EndSphere1.y - _sphereRadius + _capsuleRadius;
        Vector3 EndSphere2 = transform.position;
        EndSphere2.y = EndSphere2.y - _sphereRadius + _capsuleHeight - _capsuleRadius;
        Debug.Log(!Physics.CheckCapsule(EndSphere1, EndSphere2, _capsuleRadius, _layer));
        return !Physics.CheckCapsule(EndSphere1, EndSphere2, _capsuleRadius, _layer);
    }
}
