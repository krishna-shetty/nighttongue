using UnityEngine;

public class SlopeForce : MonoBehaviour, IForceSource
{
    [SerializeField] private float _minAngleUntransformed = 0.5f;
    [SerializeField] private float _slopeForceMultiplier = 5f;
    [SerializeField] private float _checkDistance = 0.25f;
    [SerializeField] private LayerMask _layer;

    private float _minAngleTransformed;
    private TongueTransformHandler _tongueTransformHandler;
    private PlayerController _context;
    private CharacterController _cc;
    private bool _isTransformed = false;

    private Vector3 _lastGroundNormal = Vector3.up;

    private void Awake()
    {
        _tongueTransformHandler = GetComponent<TongueTransformHandler>();
        
        if (_tongueTransformHandler == null)
        {
            Debug.LogError("SlopeForce: TongueTransformHandler component not found on the GameObject.");
        }
        else
        {
            _tongueTransformHandler.OnTransformStateChanged += HandleTongueTransform;
        }

        _context = GetComponent<PlayerController>();
        if (_context == null)
        {
            Debug.LogError("SlopeForce: PlayerController component not found on the GameObject.");
        }

        _cc = GetComponent<CharacterController>();
        if (_cc == null)
        {
            Debug.LogError("SlopeForce: CharacterController component not found on the GameObject.");
        }
    }

    private void Start()
    {
        if (_context)
            _context.RegisterForceSource(this);
    }

    private void FixedUpdate()
    {
        _lastGroundNormal = _context.GroundDetector.GroundNormal;
    }

    public Vector3 GetForce()
    {
        if (_tongueTransformHandler == null)
        {
            return Vector3.zero;
        }

        float minAngle = _isTransformed ? _tongueTransformHandler.ActiveAbility.MinSlopeAngle : _minAngleUntransformed;
        float forceMultiplier = _isTransformed ? _tongueTransformHandler.ActiveAbility.SlopeForceMultiplier : _slopeForceMultiplier;

        if (_context.GroundDetector.IsOnSteepSlope())
        {
            Vector3 groundNormal = _context.GroundDetector.GroundNormal;

            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;


            if (slopeAngle >= minAngle)
            {
                float forceMagnitude = Physics.gravity.magnitude * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * forceMultiplier;

                return slopeDirection * forceMagnitude;
            }
        }

        return Vector3.zero;
    }

    private void HandleTongueTransform(TongueTransformEventArgs args)
    {
        _isTransformed = args.IsTransformed;
    }

    private void OnDestroy()
    {
        if (_tongueTransformHandler)
            _tongueTransformHandler.OnTransformStateChanged -= HandleTongueTransform;

        if (_context)
            _context.UnregisterForceSource(this);
    }

}
