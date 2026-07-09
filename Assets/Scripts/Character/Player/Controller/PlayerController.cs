using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : StateMachine, IForceReceiver
{
    [Header("State Machine Context")]
    public JumpActionSO JumpAction;
    public MoveActionSO MoveAction;
    public JumpActionSO SwingJumpAction = null;
    public PlayerTimeSettingsSO TimeSettings;
    public StateMultiplierSO StateMultiplier;
    public GlobalPhysicsSettingsSO Settings;
    [SerializeField] private bool _usePrecisionMovement = false;

    // Input Handler
    [SerializeField]
    private PlayerInputHandler _inputHandler;

    // Ability Related Data
    private AbilityUser _abilityUser;
    private AbilitySO _currentStateAbility; // the abilitySO linked to movement state (if applicable)
    private AbilitySO _currentStateOwnerAbility; // ability that owns the current movement state (universal or same)
    private bool _isCancelingAbility = false;

    // Knockback
    public float KnockbackStrength;
    public float KnockbackTime = 0.4f; // how long to freeze input for
    [HideInInspector] public Vector2 KnockbackDirection;

    // Flags
    public bool RequestedJump = false;
    public bool HasCoyoteBuffered = false;
    public bool HasJumpBuffered = false;
    public bool RequestedGrapple = false;
    public bool ResistsWind = false;

    // Buffer Timers
    private float _elapsedJumpBufferTime = 0f;
    private float _elapsedCoyoteTime = 0f;

    // Gravity Fields
    [HideInInspector] public PlayerGravityProfile GravityProfile;

    // Grapple Mask
    public LayerMask WhatIsGrappable;

    // Swing data
    public LayerMask WhatIsSwingable;

    public float2 Velocity;
    public bool IsJumpHeld { get; private set; }

    public float HorizontalInput;
    public float VerticalInput;
    public GroundDetector GroundDetector;
    private PlayerCollisionHandler _collider;
    private PushOffOverhang _pushOff;
    private Rigidbody _rigidbody;
    private PlayerHealth _health;

    private readonly List<IForceSource> activeForces = new();
    public int LastBallMoveSign { get; private set; } = 1;
    // ====== Movement Profile ======
    public event Action<MoveActionSO, JumpActionSO> OnActionProfileChanged;

    private MoveActionSO _currentMove;
    private JumpActionSO _currentJump;

    private readonly Stack<(MoveActionSO move, JumpActionSO jump)> _profileStack
        = new Stack<(MoveActionSO, JumpActionSO)>();

    public MoveActionSO CurrentMove => _currentMove ?? MoveAction;
    public JumpActionSO CurrentJump => _currentJump ?? JumpAction;

    // SFX events and setup
    public event Action OnJump;
    public event Action OnWalkStep;
    public event Action OnGrappleStart;
    public event Action OnSwingStart;
    public event Action OnSwingEnd;
    public event Action OnSwingForward;
    public event Action OnSwingBackward;
    public event Action OnSwingAscend;
    public event Action OnSwingDescend;
    public event Action OnSwingNeutral;
    public event Action OnSwingMin;
    public event Action OnSwingMax;
    public event Action OnJumpStarted;
    public event Action OnBallStart;
    public event Action OnBallStartAnimation;
    public event Action OnBallEnd;
    public event Action OnGroundedStart;
    public event Action OnGrappleEnd;
    public event Action OnTongueDragAttach;
    public event Action OnTongueDragDetach;

    public int PreviousVelocityXSign;
    public int PreviousVerticalInputSign;

    // SFX Hooks --Ethan
    public void RaiseJumpStarted() { OnJumpStarted?.Invoke(); }

    public void RaiseSwingStarted()
    {
        PreviousVelocityXSign = 0;
        PreviousVerticalInputSign = 0;
        OnSwingStart?.Invoke();
    }

    public void RaiseSwingAscend() { OnSwingAscend?.Invoke(); }
    public void RaiseSwingDescend() { OnSwingDescend?.Invoke(); }
    public void RaiseSwingNeutral() { OnSwingNeutral?.Invoke(); }
    public void RaiseSwingMin() { OnSwingMin?.Invoke(); }
    public void RaiseSwingMax() { OnSwingMax?.Invoke(); }

    public void RaiseSwingEnded()
    {
        OnSwingEnd?.Invoke();
        PreviousVelocityXSign = 0;
        PreviousVerticalInputSign = 0;
    }
    public void RaiseEndBallAbility() { OnBallEnd?.Invoke(); }
    public void RaiseStartBallAbility() { OnBallStart?.Invoke(); }
    public void RaiseStartBallAnimation() { OnBallStartAnimation?.Invoke(); }
    public void RaiseGroundedStart() { OnGroundedStart?.Invoke(); }
    public void RaiseSwingForward() { OnSwingForward?.Invoke(); }
    public void RaiseSwingBackward() { OnSwingBackward?.Invoke(); }
    public void RaiseGrappleEnded() { OnGrappleEnd?.Invoke(); }
    public void RaiseDragAttach() { OnTongueDragAttach?.Invoke(); }
    public void RaiseDragDetach() { OnTongueDragDetach?.Invoke(); }

    private void ApplyProfileToPhysics()
    {
        GravityProfile = PlayerGravityProfile.Create(CurrentMove, CurrentJump);
        Vector3 gravityVector = new(0f, GravityProfile.GetFastFallGravity(), 0f);
        // PhysicsManager.Instance.SetGravity(gravityVector);
        OnActionProfileChanged?.Invoke(CurrentMove, CurrentJump);
    }

    public void SetProfile(MoveActionSO move, JumpActionSO jump)
    {
        _profileStack.Clear();
        _currentMove = move ?? MoveAction;
        _currentJump = jump ?? JumpAction;
        ApplyProfileToPhysics();
    }

    public void PushProfile(MoveActionSO move, JumpActionSO jump)
    {
        _profileStack.Push((CurrentMove, CurrentJump));
        _currentMove = move ?? CurrentMove;
        _currentJump = jump ?? CurrentJump;
        ApplyProfileToPhysics();
    }

    public void PopProfile()
    {
        if (_profileStack.Count > 0)
        {
            var prev = _profileStack.Pop();
            _currentMove = prev.move;
            _currentJump = prev.jump;
        }
        else
        {
            _currentMove = MoveAction;
            _currentJump = JumpAction;
        }
        ApplyProfileToPhysics();
    }

    private void HandleTransformProfileChange(TongueTransformEventArgs e)
    {
        if (e.IsTransformed)
            PushProfile(e.MoveAction, e.JumpAction);
        else
            PopProfile();
    }

    /// <summary>
    /// For when an ability button press triggers a movement state change
    /// (all ability-related movement state changes should route through here)
    /// </summary>
    public void HandleMovementStateRequest(MovementStateIntent intent)
    {
        switch (intent)
        {
            case {
                TargetState: MovementHandlerType.GRAPPLING,
                ActiveAbility: GrappleAbilitySO ability,
                Data: Vector3 point
            }:
                //Debug.Log("TRANSITIONING STATE ON ABILITY REQUEST: " + CurrentState + " to GrapplingState");
                TransitionToState(new GrapplingState(this, ability, point));
                _currentStateAbility = ability;
                _currentStateOwnerAbility = intent.OwnerAbility;
                break;
            case {
                TargetState: MovementHandlerType.SWINGING,
                ActiveAbility: SwingAbilitySO ability,
                Data: Collider target,
            }:
                if (CurrentState is not SwingingState)
                {
                    // Debug.Log("TRANSITIONING STATE ON ABILITY REQUEST: " + CurrentState + " to SwingingState");
                    TransitionToState(new SwingingState(this, ability, target, jump: SwingJumpAction));
                    _currentStateAbility = ability;
                    _currentStateOwnerAbility = intent.OwnerAbility;
                }
                break;
            default:
                Debug.Log("PlayerController :: Attempted to transition to " + intent.TargetState + " with invalid parameters.");
                break;
        }
    }

    /// <summary>
    /// For triggering ability release when movement cancels an ability i.e. grapple hits terrain
    /// (this will eventually also execute movement state change)
    /// </summary>
    public void RequestAbilityCancel()
    {
        AbilitySO abilityToCancel = _currentStateOwnerAbility ?? _currentStateAbility;

        if (_currentStateAbility != null && !_isCancelingAbility)
        {
            //Debug.Log("REQUESTED ABILITY CANCEL FROM: " + CurrentState);
            _isCancelingAbility = true;
            if (abilityToCancel && _abilityUser.ReleaseActiveHandler(abilityToCancel))
            {
                _currentStateAbility = null;
                _currentStateOwnerAbility = null;
            }    
            _isCancelingAbility = false;
        }
    }

    private void HandleAbilityCanceled(AbilitySO ability)
    {
        // Trigger movement state change if the linked ability releases
        if (_currentStateAbility == ability)
        {
            _currentStateAbility = null; // make sure this is null before GetNextState
            _currentStateOwnerAbility = null;

            BaseState nextState = QueuedState ?? CurrentState.GetNextState();
            if (nextState != null)
            {
                //Debug.Log("TRANSITIONING STATE ON ABILITY CANCEL: " + CurrentState + " to " + nextState);
                TransitionToState(nextState);
                if (QueuedState != null) QueuedState = null;
            }
        }
    }

    private void HandleJumpRequested()
    {
        if (_abilityUser.GetActiveAbility() is TongueTransformSO) return;

        if (GroundDetector.IsGrounded || HasCoyoteBuffered)
        {
            RequestedJump = true;
            OnJump?.Invoke(); //SFX
        }
        else
        {
            HasJumpBuffered = true;
            _elapsedJumpBufferTime = 0f;
        }
    }

    private void HandleJumpReleased()
    {
        IsJumpHeld = false;
    }

    private void HandleJumpHeld()
    {
        IsJumpHeld = true;
    }

    private void HandleKnockbackDamage(GameObject victim, GameObject source, bool knockback)
    {
        if (!knockback) return;
        KnockbackDirection = victim.transform.position - source.transform.position; // point away from damage source
        //Debug.Log("knockback on " + CurrentState + " " + KnockbackDirection.normalized);
        if (KnockbackDirection.x <= 0) KnockbackDirection = new(-1, 1);
        else KnockbackDirection = Vector2.one;

        TransitionToState(new KnockbackState(this, CurrentMove, CurrentJump));
        _inputHandler.FreezeInput(KnockbackTime);
        _abilityUser.FreezeAbilities(KnockbackTime);
    }

    public void UpdateBuffers()
    {
        if (HasJumpBuffered)
        {
            _elapsedJumpBufferTime += Time.fixedDeltaTime;
            if (_elapsedJumpBufferTime >= TimeSettings.JumpBufferTime)
            {
                HasJumpBuffered = false;
                _elapsedJumpBufferTime = 0f;
            }
        }

        if (HasCoyoteBuffered)
        {
            _elapsedCoyoteTime += Time.fixedDeltaTime;
            if (_elapsedCoyoteTime >= TimeSettings.CoyoteTime)
            {
                HasCoyoteBuffered = false;
                _elapsedCoyoteTime = 0f;
            }
        }
    }

    public void ActivateCoyoteTime()
    {
        if (GroundDetector.WasGrounded)
        {
            HasCoyoteBuffered = true;
            _elapsedCoyoteTime = 0f;
        }
    }

    /// <summary>
    /// Currently only calls GroundDetector.Refresh() to update grounded/just landed states, doesn't actually apply any physics
    /// <br/>**Move to ResetFlags()?
    /// </summary>
    public void ApplyPhysics()
    {
        GroundDetector.Refresh();
    }

    public void ResetFlags()
    {
        RequestedJump = false;
    }

    public void RegisterForceSource(IForceSource source)
    {
        if (!activeForces.Contains(source))
        {
            activeForces.Add(source);
            
            Debug.Log("Registered force source: " + source.ToString());
        }
    }

    public void UnregisterForceSource(IForceSource source)
    {
        if (activeForces.Contains(source))
        {
            activeForces.Remove(source);
            
            Debug.Log("Unregistered force source: " + source.ToString());
        }
    }

    public void UnregisterAllForceSources()
    {
        activeForces.Clear();
    }

    public Vector3 GetTotalExternalForce()
    {
        Vector3 totalForce = Vector3.zero;
        foreach (var source in activeForces)
        {
            if (ResistsWind && source is WindVolume)
            {
                continue;
            }

            
            totalForce += source.GetForce();
        }
        return totalForce;
    }

    public bool IsInState<T>() where T : BaseState
    {
        return CurrentState is T;
    }

    public void ApplyVelocity(Vector2 velocity)
    {
        Velocity[0] += velocity.x;
        Velocity[1] += velocity.y;
    }

    private void Awake()
    {
        _currentMove = MoveAction;
        _currentJump = JumpAction;
        UpdateType = UpdateType.Fixed;

        GroundDetector = GetComponent<GroundDetector>();
        _rigidbody = GetComponent<Rigidbody>();
        _pushOff = GetComponent<PushOffOverhang>();
        _collider = GetComponent<PlayerCollisionHandler>();
        _inputHandler = GetComponent<PlayerInputHandler>();
        _abilityUser = GetComponent<AbilityUser>();
        _health = GetComponent<PlayerHealth>();
        WhatIsGrappable = GetComponent<GrappleHandler>().WhatIsGrappable;

        _rigidbody.isKinematic = true;
        _rigidbody.freezeRotation = true;
        RequestedGrapple = false;
        ResistsWind = false;

        HorizontalInput = _inputHandler.MoveInput.x;
        VerticalInput = _inputHandler.MoveInput.y;
    }

    void Start()
    {
        // Subscribe to input events ONCE
        _inputHandler.OnJumpRequested += HandleJumpRequested;
        _inputHandler.OnJumpHeld += HandleJumpHeld;
        _inputHandler.OnJumpReleased += HandleJumpReleased;

        var tongue = GetComponent<TongueTransformHandler>();
        if (tongue != null)
            tongue.OnTransformStateChanged += HandleTransformProfileChange;

        _abilityUser.OnAbilityCanceled += HandleAbilityCanceled;
        if (_health) _health.OnDamage += HandleKnockbackDamage;

        GravityProfile = PlayerGravityProfile.Create(MoveAction, JumpAction);
        Vector3 gravityVector = new(0f, GravityProfile.GetFastFallGravity(), 0f);
        PhysicsManager.Instance.SetGravity(gravityVector);
        RespawnManager.Instance.InitializePlayer();

        // Initialize starting state
        CurrentState = new GroundedState(this);
        TransitionToState(CurrentState);
    }

    void OnDestroy()
    {
        // Unsubscribe from input events
        if (_inputHandler != null)
        {
            _inputHandler.OnJumpRequested -= HandleJumpRequested;
            _inputHandler.OnJumpHeld -= HandleJumpHeld;
            _inputHandler.OnJumpReleased -= HandleJumpReleased;
        }
    }

    private void Update()
    {
        HorizontalInput = _inputHandler.MoveInput.x;
        VerticalInput = _inputHandler.MoveInput.y;
        IsJumpHeld = _inputHandler.IsJumpHeld;

        if (_abilityUser.GetActiveAbility() is TongueTransformSO)
        {
            if (Velocity.x > 0.01f)
                LastBallMoveSign = 1;
            else if (Velocity.x < -0.01f)
                LastBallMoveSign = -1;
        }
    }

    public PlayerCollisionHandler GetCollider() { return _collider; }

    public bool UsingPrecisionMovement() { return _usePrecisionMovement; }

    public PushOffOverhang PushOff() { return _pushOff; }

    public BaseState GetCurrentState() { return CurrentState; }

    public AbilitySO GetCurrentStateAbility() { return _currentStateAbility; }
}