using UnityEngine;

public class GrapplingState : PlayerState
{
    private float _initialVerticalSpeed;
    private float _initialHorizontalSpeed;
    private float _jumpGravity;
    private Vector3 _grapplePoint;
    private Vector3 _initialPlayerPos;
    private GrappleAbilitySO _activeAbility;
    private PlayerCollisionHandler _collider;
    private GrappleHandler _grappleHandler;
    private float _grappleEntryTimer = 0f;
    private float _grappleIgnoreDuration = 0.05f;

    private SwingHandler _swingHandler;
    private MovementHandlerBase _movementHandler;
    private TongueController _tongueController;

    public GrapplingState(PlayerController controller, GrappleAbilitySO grappleAbility, Vector3 grapplePoint)
    {
        Context = controller;
        _grapplePoint = grapplePoint;
        _initialPlayerPos = Context.transform.position;
        _grappleIgnoreDuration = Context.Settings.GrappleIgnoreDuration;
        _activeAbility = grappleAbility;
        _movementHandler = MovementHandlerFactory.GetHandler(MovementHandlerType.GRAPPLING, controller);
    }

    private float GetHorizontalDistance()
    {
        float horizontalDistance = _grapplePoint.x - _initialPlayerPos.x;
        horizontalDistance += Mathf.Sign(horizontalDistance) * _activeAbility.OffsetDistance;
        return horizontalDistance;
    }

    private float GetVerticalDistance()
    {
        float verticalDistance = _grapplePoint.y - _initialPlayerPos.y;
        verticalDistance += Mathf.Sign(verticalDistance) * _activeAbility.OffsetHeight;
        return verticalDistance;
    }

    private float CalculateVerticalHeight()
    {
        float highestPoint = 0;

        float verticalDistance = GetVerticalDistance();
        if (verticalDistance < 0)
        {
            highestPoint = _activeAbility.OvershootHeight;
        }
        else
        {
            highestPoint = verticalDistance + _activeAbility.OvershootHeight;
        }

        return highestPoint;
    }

    private void CalculateJumpVelocity()
    {
        float highestPoint = CalculateVerticalHeight();
        
        float verticalDistance = GetVerticalDistance();
        float horizontalDistance = GetHorizontalDistance();

        _initialVerticalSpeed = Mathf.Sqrt(-2 * Context.GravityProfile.GetJumpGravity() * highestPoint);
        float timeUp = Mathf.Sqrt((-2 * highestPoint) / Context.GravityProfile.GetJumpGravity());
        float timeDown = Mathf.Sqrt((-2 * (highestPoint - verticalDistance) / Context.GravityProfile.GetFastFallGravity()));
        _initialHorizontalSpeed = horizontalDistance / (timeUp + timeDown);
    }

    protected override void InitializeGravity()
    {
        _jumpGravity = Context.GravityProfile.CalculateJumpGravity(ActiveMoveAction, ActiveJumpAction);
        Gravity = _jumpGravity;
    }

    private void ApplyGrapple()
    {
        CalculateJumpVelocity();
        Gravity = Context.GravityProfile.CalculateJumpGravity(ActiveMoveAction, ActiveJumpAction);
        Context.Velocity.y = _initialVerticalSpeed;
        Context.Velocity.x = _initialHorizontalSpeed;
    }

    private bool IsGrappleValid()
    {
        float distanceToTarget = Vector3.Distance(_initialPlayerPos, _grapplePoint);
        if (distanceToTarget > _activeAbility.MaxGrappleDistance)
        {
            Debug.LogWarning("GrapplingState :: Grapple target is out of range.");
            return false;
        }
        return Physics.Raycast(_initialPlayerPos, (_grapplePoint - _initialPlayerPos).normalized, _activeAbility.MaxGrappleDistance, Context.WhatIsGrappable);
    }

    protected override void OnEnter()
    {
        _collider = Context.GetComponent<PlayerCollisionHandler>();
        if (_collider == null)
        {
            Debug.LogError("GrapplingState :: PlayerCollisionHandler component not found on PlayerController.");
        }

        GameObject tongue = Context.transform.Find("Tongue").gameObject;
        if (tongue == null)
        {
            Debug.LogError("GrappleHandler: Tongue GameObject not found. Please ensure it is attached as a child to this GameObject.");
        }
        else
        {
            _tongueController = tongue.GetComponent<TongueController>();
            if (_tongueController == null)
            {
                Debug.LogError("GrappleHandler :: TongueController component not found on the Tongue GameObject.");
            }
            _tongueController.AttachTongue(_grapplePoint);
        }

        _collider.OnCollisionDetected += OnPlayerCollision;
        if (IsGrappleValid())
        {
            ApplyGrapple();
            Context.RequestedGrapple = false;
        }
        else
        {
            Context.RequestAbilityCancel();
        }
    }

    private void OnPlayerCollision()
    {
        if (_grappleEntryTimer < _grappleIgnoreDuration)
        {
            return;
        }

        Context.RequestAbilityCancel();
    }

    protected override void OnExit()
    {
        Context.RaiseGrappleEnded();
        
        if (Context != null)
        {
            _collider.OnCollisionDetected -= OnPlayerCollision;
        }
    }

    public override void UpdateState()
    {
        //_movementHandler.SimulateStep();
    }

    public override void FixedUpdateState()
    {
        Context.ApplyPhysics();

        _grappleEntryTimer += Time.fixedDeltaTime;
        if (Context.Velocity.y <= 0f)
        {
            Gravity = Context.GravityProfile.GetFastFallGravity();
        }
        _movementHandler.Apply(Gravity, ActiveMoveAction);

        Context.ResetFlags();
    }

    public override BaseState GetNextState()
    {
        if (Context.RequestedJump || Context.HasJumpBuffered)
        {
            return new JumpingState(Context);
        }

        if (Context.GroundDetector.IsGrounded)
        {
            return new GroundedState(Context);
        }
        else
        {
            return new FallingState(Context);
        }
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
