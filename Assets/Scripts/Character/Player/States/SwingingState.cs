using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines.Interpolators;

public class SwingingState : PlayerState
{
    private Collider _swingCollider;
    private Vector3 _swingPoint;

    private SwingAbilitySO _activeAbility;
    private JumpActionSO _jumpAction = null;
    private float _swingRestLength;     // tongue length will interpolate towards target length... this is the current length
    private static float _swingTargetLength;
    private static float _initialSwingLength = -1.0f;
    private float _swingLengthSpeed = 0.0f;
    private float _swingPreviousLength;

    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;
    private SwingingMovementHandler _movementHandler;
    private TongueController _tongueController;
    private Animator _animator;
    
    private bool _minLengthSfxFired = false;
    private bool _maxLengthSfxFired = false;


    public SwingingState(PlayerController controller, SwingAbilitySO swingAbility, Collider swingTarget, JumpActionSO jump = null)
    {
        Context = controller;
        _animator = Context.GetComponentInChildren<Animator>();
        _activeAbility = swingAbility;
        _swingCollider = swingTarget;
        if (jump) _jumpAction = jump;

        // hard cast because this should always return that handler
        _movementHandler = (SwingingMovementHandler)MovementHandlerFactory.GetHandler(MovementHandlerType.SWINGING, controller);
        _movementHandler.SetSwingingState(this);
    }

    protected override void OnEnter()
    {
        if (_swingCollider == null)
        {
            Debug.LogWarning("SwingingState :: Swing collider is null.");
            Context.QueueNextState(InvalidSwingTransition());
            Context.RequestAbilityCancel();
            return;
        }

        _swingPoint = RecalculateSwingPoint();

        if (!IsSwingValid())
        {
            BaseState nextState = InvalidSwingTransition();
            if (nextState != null)
            {
                Context.QueueNextState(nextState);
                Context.RequestAbilityCancel();
            }
            else
            {
                Debug.LogError("SwingingState :: No valid next state found after grapple validation.");
            }
            return;
        }

        var tongueTf = Context.transform.Find("Tongue");
        if (tongueTf == null)
        {
            Debug.LogError("SwingingState: Tongue GameObject not found as child.");
            return;
        }

        _tongueController = tongueTf.GetComponent<TongueController>();
        if (_tongueController == null)
        {
            Debug.LogError("SwingingState: TongueController missing on Tongue.");
            return;
        }

        _tongueController.AttachTongue(_swingPoint, true);
        if (_initialSwingLength < 0.0f)
        {   // on the first swing, we'll read the initial length from the _activeAbility.
            // after that, we'll keep the swing length constant as long as the player is in the air.
            // whenever the player lands, we'll reset the swing length to this value.
            _initialSwingLength = _activeAbility.InitialTongueLength;
            _swingTargetLength = _initialSwingLength;
        }
        _swingLengthSpeed = 0.0f;

        _swingRestLength = Vector3.Distance(Context.transform.position, _swingPoint);
        CalculateInitialTangentialVelocity();

        Context.RaiseSwingStarted();
    }

    public static void ResetLength()
    {
        if (_initialSwingLength > 0.0f)
        {
            _swingTargetLength = _initialSwingLength;
        }
    }

    private bool IsSwingValid()
    {
        Vector3 playerPos = Context.transform.position;
        Vector3 toSwingPoint = _swingPoint - playerPos;

        float distance = toSwingPoint.magnitude;

        Vector3 dir = toSwingPoint / distance;

        Vector3 origin = playerPos;

        if (Physics.Raycast(
                origin,
                dir,
                out RaycastHit hit,
                _activeAbility.MaxRappleDistance + 0.1f,
                LayerMask.GetMask("Terrain"),
                QueryTriggerInteraction.Ignore))
        {
            if (hit.distance < distance) return false;
        }
        return true;
    }


    public override BaseState GetNextState()
    {
        if (Context.RequestedJump || Context.HasJumpBuffered)
        {
            if (_jumpAction) return new JumpingState(Context, jump: _jumpAction);
            else return new JumpingState(Context);
        }

        if (Context.GetCurrentStateAbility() != _activeAbility)
        {
            if (Context.GroundDetector.IsGrounded) return new GroundedState(Context);
            else return new FallingState(Context);
        }

        return null; // Stay in SwingingState
    }

    private BaseState InvalidSwingTransition()
    {
        if (Context.RequestedJump || Context.HasJumpBuffered)
        {
            return new JumpingState(Context);
        }

        if (!Context.GroundDetector.IsGrounded)
        {
            return new FallingState(Context);
        }
        else
        {
            return new GroundedState(Context);
        }
    }

    public override void FixedUpdateState()
    {
        if (!IsSwingValid())
        {
            Context.QueueNextState(InvalidSwingTransition());
            Context.RequestAbilityCancel();
            return;
        }

        _swingPoint = RecalculateSwingPoint();
        _tongueController.AttachTongue(_swingPoint, true);

        Context.ApplyPhysics();
        AdjustSwingLength();
        HandleSFX();
        _movementHandler.Apply(Gravity, ActiveMoveAction);
        Context.UpdateBuffers();
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            Context.RequestAbilityCancel();
        }
        Context.ResetFlags();

        Vector3 swingDelta = _swingPoint - Context.transform.position;
        float angle = Mathf.Atan2(-swingDelta.x, swingDelta.y);
        angle = Mathf.Clamp(Mathf.Rad2Deg * angle, -45.0f, 45.0f);
        _animator.SetFloat("SwingAngle", angle);
    }

    public override void UpdateState()
    {
        //_movementHandler.SimulateStep();

    }

    private Vector3 RecalculateSwingPoint()
    {
        return _swingCollider.ClosestPoint(Context.transform.position);
    }

    private void CalculateInitialTangentialVelocity()
    {
        Vector3 playerPos = Context.transform.position;
        Vector3 ropeVec = playerPos - _swingPoint;

        if (ropeVec.magnitude < Context.Settings.FloatPrecisionThreshold)
        {
            Debug.LogWarning("Rope vector is too small, cannot calculate tangential velocity.");
            return;
        }
        Vector3 ropeDir = ropeVec.normalized;
        Vector3 tangentialDir = new Vector3(-ropeDir.y, ropeDir.x, 0);

        Vector3 initialVel = new Vector3(Context.Velocity.x, Context.Velocity.y, 0);
        float initialTangentialSpeed = Vector3.Dot(tangentialDir, initialVel);

        // Clamp the initial tangential speed to prevent excessively high speeds
        initialTangentialSpeed = Mathf.Clamp(initialTangentialSpeed, -_activeAbility.MaxInitialSwingSpeed, _activeAbility.MaxInitialSwingSpeed);
        
        Vector3 initialTangentialVel = tangentialDir * initialTangentialSpeed;

        Context.Velocity.x = initialTangentialVel.x;
        Context.Velocity.y = initialTangentialVel.y;
    }
    private void AdjustSwingLength()
    {
        int sign = Math.Sign(Context.VerticalInput);
        
        if (sign != 0)
        {
            float rappleSpeed = (Math.Sign(Context.VerticalInput) > 0) ? _activeAbility.RappelUpSpeed : _activeAbility.RappelDownSpeed;
            _swingTargetLength -= Math.Sign(Context.VerticalInput) * rappleSpeed * Time.fixedDeltaTime;
            _swingTargetLength = Mathf.Clamp(_swingTargetLength, 1f, _activeAbility.MaxRappleDistance);
        }
        _swingRestLength = Mathf.SmoothDamp(_swingRestLength, _swingTargetLength, ref _swingLengthSpeed, _activeAbility.TongueLerpTime);

        Physics.SyncTransforms();
    }

    private void HandleSFX()
    {
        HorizontalSFX();
        VerticalSFX();
    }

    private void HorizontalSFX()
    {
        float velocityX = Context.Velocity.x;
        int sign;
        if (velocityX != 0.0f) { sign = (velocityX > 0.0f) ? 1 : -1; }
        else { sign = 0; }
        if (sign == Context.PreviousVelocityXSign) return;
        if (sign == 1) Context.RaiseSwingForward();
        else if (sign == -1) Context.RaiseSwingBackward();
        Context.PreviousVelocityXSign = sign;
    }

    private void VerticalSFX()
    {
        int sign = Math.Sign(Context.VerticalInput);
        
        bool atMinLength = _swingTargetLength <= 1f;
        bool atMaxLength = _swingTargetLength >= _activeAbility.MaxRappleDistance;

        if (sign > 0 && atMinLength && !_minLengthSfxFired)
        {
            _minLengthSfxFired = true;
            _maxLengthSfxFired = false;
            Context.RaiseSwingMin();
            Debug.Log("Swing: Reached minimum length");
        }
        if (sign < 0 && atMaxLength && !_maxLengthSfxFired)
        {
            _maxLengthSfxFired = true;
            _minLengthSfxFired = false;
            Context.RaiseSwingMax();
            Debug.Log("Swing: Reached maximum length");
        }
        
        if (sign == Context.PreviousVerticalInputSign) return;
        
        if (sign > 0 && !atMinLength)
        {
            Context.RaiseSwingAscend();
            Debug.Log("Swing: Ascending");
        }
        else if (sign < 0 && !atMaxLength)
        {
            Context.RaiseSwingDescend();
            Debug.Log("Swing: Descending");
        }
        else if (sign == 0)
        {
            _minLengthSfxFired = false;
            _maxLengthSfxFired = false;
            Context.RaiseSwingNeutral();
            Debug.Log("Swing: Neutral");
        }
        Context.PreviousVerticalInputSign = sign;
    }

    protected override void OnExit()
    {
        Context.RaiseSwingEnded(); // SFX

        if (_tongueController != null)
            _tongueController.AimTongue();
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }

    public Vector3 GetSwingPoint() { return _swingPoint; }
    public SwingAbilitySO GetActiveAbilitySO() { return _activeAbility; }
    public float GetSwingRestLength() { return _swingRestLength; }
}
