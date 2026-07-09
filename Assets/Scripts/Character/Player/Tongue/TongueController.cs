using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

//#define ENABLE_AIM

public class TongueController : MonoBehaviour
{

    [SerializeField] private LayerMask _aimPlaneMask;
    public GlobalPhysicsSettingsSO Settings;
    [SerializeField] private Transform _ikTarget;
    [SerializeField] private Transform _headJoint;

    [Header("Aim Rig")]
    [SerializeField] private Rig aimRig;
    [SerializeField] private Transform _tongueTip;
    [SerializeField] private float blendInSpeed = 10f;
    [SerializeField] private float blendOutSpeed = 12f;

    [Header("Limits")]
    public TongueLimitSO TongueLimit;

    private LineRenderer _tongue;
    private GameObject _player;
#if !ENABLE_AIM
    private Transform _turnXForm;   // the transform that will be rotated to turn the player's facing direction
#endif
    [SerializeField] private FlavorManager _flavorManager;
    private Coroutine _activeRoutine;

    public bool IsAiming = false;

    //private bool _isExtending = false;
    private bool _isExtending => _mode == TongueMode.Extending || _mode == TongueMode.AttachedSwinging || _mode == TongueMode.AttachedHolding;
    private const float RigOffEps = 0.001f;
    public TongueMode _mode = TongueMode.Aim;

    public float TongueLength = 2f;
    public Vector3 EndPoint;
    public Vector3 Direction;
    public Vector3 Velocity;
    public Vector3 IdleDirection = Vector3.left;

    public bool IsAimingActive => aimRig ? (IsAiming && aimRig.weight > RigOffEps) : IsAiming;
    public bool IsExtending => _mode == TongueMode.Extending;
    public bool IsAttached => _mode == TongueMode.AttachedSwinging;
    public bool IsHold=> _mode == TongueMode.AttachedHolding;

    public event Action<Vector3, float> OnTongueExtension;
    public event Action<float> OnTongueRetraction;
    public event Action<Vector3, float> OnTongueExtensionAndRetraction;
    public event Action<Vector3, bool> OnTongueAttach;
    public event Action OnTongueExtendToLength;

    public float CurrentLength { get; private set; }

#if ENABLE_AIM
    private bool _usingGamepad; // Action binding adaptation
    private bool _usingPointer; // Action binding adaptation
    private Vector2 _lookInput; // Action binding adaptation
    private float _mouseSensitivity = 1; // Action binding adaptation
#endif

#if ENABLE_AIM
    private void PullInput()
    {
        if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f)
        {
            _lookInput = Gamepad.current.rightStick.ReadValue();
            _usingGamepad = true;
            _usingPointer = false;
        }
        else if (Mouse.current != null)
        {
            _lookInput = Mouse.current.delta.ReadValue();
            _usingGamepad = false;
            _usingPointer = true;
        }
    }
#endif

    public enum TongueMode
    {
        Idle,
        Aim,
        Extending,
        AttachedHolding,
        AttachedSwinging
    }

    private void Awake()
    {
        _tongue = GetComponent<LineRenderer>();
        _player = GameObject.FindWithTag("Player");

        if (_tongue == null) Debug.LogError("Missing LineRenderer.");
        if (_player == null) Debug.LogError("Player not found. Tag it as 'Player'.");

        if (_player)
        {
            _flavorManager = _player.GetComponent<FlavorManager>();
#if !ENABLE_AIM
            // get the transform that will be rotated to turn the player's facing direction (this is the direction we'll aim)
            TurnEventRelay turnRelay = _player.GetComponentInChildren<TurnEventRelay>();
            if (turnRelay)
            {
                    _turnXForm = turnRelay.transform;
            }
#endif
        }
    }

    private void Start()
    {
        _tongue.startWidth = 0.5f;
        _tongue.endWidth = 0.0f;
        _mode = TongueMode.Idle;
        if (aimRig) aimRig.weight = 1f;
        EndPoint = GetTongueAimPos();
        IsAiming = false;
    }

    private void Update()
    {
        UpdateFlavorAndRig();
        UpdateModeLogic();
        //UpdateTongue();
        //Debug.Log($"mode: {_mode}");
    }

    private void LateUpdate()
    {
        UpdateTongue();
    }

    #region --- Main Update Logic ---

    public void UpdateFlavorAndRig()
    {
       
       // IsAiming = _flavorManager ? _flavorManager.GetActiveActiveFlavor() is SweetFlavorSO : false;
        float target = (IsAiming||_isExtending)? 1f:0f;
        float speed = IsAiming ? blendInSpeed : blendOutSpeed;

        if (aimRig) {
            //Debug.Log($"Updating flavor and rig. FlavorManager: {_flavorManager}, ActiveFlavor: {_flavorManager?.GetActiveActiveFlavor()}");
            aimRig.weight = Mathf.MoveTowards(aimRig.weight, target, speed * Time.deltaTime);
        }
            
    }

    private void UpdateModeLogic()
    {
        bool aimingActive = IsAimingActive;

        switch (_mode)
        {
            case TongueMode.Idle:
                if (aimingActive)
                    SetMode(TongueMode.Aim);
                else
                    MaintainIdle();
                break;

            case TongueMode.Aim:
                if (!aimingActive)
                {
                    SetMode(TongueMode.Idle);
                }
                else
                    UpdateAim();
                break;

            case TongueMode.Extending:
                if (_ikTarget)
                    _ikTarget.position = EndPoint;
                break;

            case TongueMode.AttachedSwinging:
                if (_ikTarget)
                    _ikTarget.position = EndPoint;
                break;

            case TongueMode.AttachedHolding:
                if (_ikTarget)
                    _ikTarget.position = EndPoint;
                break;
        }

        if (!aimingActive && _tongueTip)
            EndPoint = _tongueTip.position;
    }

    private void MaintainIdle()
    {
        Vector3 origin = _player.transform.position;
#if !ENABLE_AIM
        float ang = _turnXForm.transform.localEulerAngles.y;
        if (ang < 135.0f)
            IdleDirection = Vector3.right;
        else
            IdleDirection = Vector3.left;
#endif
        Direction = IdleDirection.normalized;
        EndPoint = origin + Direction * TongueLength;

        if (_ikTarget)
            _ikTarget.position = EndPoint;
    }

    private void UpdateAim()
    {
#if ENABLE_AIM
        PullInput();

        if (_usingGamepad)
        {
            AimByStick();
        }
        else if (_usingPointer)
        {
            AimByMouse();
        }
#else
        MaintainIdle();
#endif
    }

#if ENABLE_AIM
    private void AimByMouse()
    {
        Vector3 origin = _player.transform.position;

        float horizontal_velocity = _lookInput.x;
        float angularVelocity = horizontal_velocity * _mouseSensitivity;

        float Direction_angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
        Direction_angle += (-1) * angularVelocity;
        float Direction_angle_rad = Direction_angle * Mathf.Deg2Rad;

        Direction = new Vector3(Mathf.Cos(Direction_angle_rad), Mathf.Sin(Direction_angle_rad), 0f);

        float delta = Vector2.SignedAngle(Vector2.right, (Vector2)Direction);
        if (delta < -90f) delta = 360.0f - delta * -1f;

        float clamped = Mathf.Clamp(delta, TongueLimit.MinAngle, TongueLimit.MaxAngle);
        if (delta < TongueLimit.MinAngle)
            clamped = (Direction.x < 0f) ? TongueLimit.MaxAngle : TongueLimit.MinAngle;

        Direction = Quaternion.AngleAxis(clamped, Vector3.forward) * Vector3.right;
        Direction.Normalize();

        EndPoint = origin + Direction * TongueLength;

        if (_ikTarget)
            _ikTarget.position = EndPoint;
    }

    private void AimByStick()
    {
        Vector3 origin = _player.transform.position;
        Direction = new Vector3(_lookInput.normalized.x, _lookInput.normalized.y, 0f);

        float delta = Vector2.SignedAngle(Vector2.right, (Vector2)Direction);
        if (delta < -90f) delta = 360.0f - delta * -1f;

        float clamped = Mathf.Clamp(delta, TongueLimit.MinAngle, TongueLimit.MaxAngle);
        if (delta < TongueLimit.MinAngle)
            clamped = (Direction.x < 0f) ? TongueLimit.MaxAngle : TongueLimit.MinAngle;

        Direction = Quaternion.AngleAxis(clamped, Vector3.forward) * Vector3.right;
        Direction.Normalize();

        EndPoint = origin + Direction * TongueLength;

        if (_ikTarget)
            _ikTarget.position = EndPoint;
    }
#endif

#endregion

    #region --- Public Control Methods ---

    public void ExtendTongue(Vector3 target, float duration)
    {
        if (_mode == TongueMode.AttachedSwinging)
        {
            Debug.LogWarning("Cannot extend tongue while attached.");
            return;
        }

        OnTongueExtension?.Invoke(target, duration);
        RestartCoroutine(ExtendTongueRoutine(target, duration));
    }

    public void RetractTongue(float duration)
    {
        OnTongueRetraction?.Invoke(duration);
        RestartCoroutine(RetractTongueRoutine(duration));
    }

    public void ExtendTongueToLength(float length, float duration)
    {
        if (_mode == TongueMode.AttachedSwinging)
        {
            Debug.LogWarning("Cannot extend tongue while attached.");
            return;
        }
        AimTongue();
        OnTongueExtendToLength?.Invoke();
        RestartCoroutine(ExtendTongueToLengthRoutine(length, duration));
    }

    public void ExtendAndRetractTongue(Vector3 target, float duration)
    {
        if (_mode == TongueMode.AttachedSwinging)
        {
            Debug.LogWarning("Cannot extend or retract tongue while attached.");
            return;
        }
        OnTongueExtensionAndRetraction?.Invoke(target, duration);
        var start = GetTongueAimPos();
        RestartCoroutine(ExtendAndRetractRoutine(start, target, duration));
    }

    public void AttachTongue(Vector3 target, bool swinging = false)
    {
        OnTongueAttach?.Invoke(target, swinging);
        RestartCoroutine(null);
        _mode = swinging ? TongueMode.AttachedSwinging : TongueMode.AttachedHolding;
        EndPoint = target;
        

    }

    public void AimTongue()
    {
        OnTongueRetraction?.Invoke(0f);
        RestartCoroutine(null);
        SetMode(TongueMode.Aim);
        UpdateAim();
        EndPoint = GetTongueAimPos();
        


    }

    public void ResetToIdle()
    {
        RestartCoroutine(null);
        SetMode(TongueMode.Idle);
        MaintainIdle();
    }

    #endregion

    #region --- Helper Utilities ---

    private void SetMode(TongueMode mode)
    {
        _mode = mode;
    }

    public void StopAllCoroutines()
    {
        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);
    }

    public void UpdateEndPoint(Vector3 target)
    {
        EndPoint = target;
    }

    public void SetTongueLength(float length)
    {
        CurrentLength = length;
    }

    private void RestartCoroutine(IEnumerator routine)
    {
        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);

        if (routine != null)
        {
            Debug.Log("Starting Tongue Coroutine: " + (routine != null ? routine.ToString() : "null"));
            _activeRoutine = StartCoroutine(routine);
        }  
        else
            _activeRoutine = null;
    }

    private IEnumerator ExtendTongueRoutine(Vector3 target, float duration)
    {
        
        aimRig.weight= 1f;
        SetMode(TongueMode.Extending);
        Vector3 start = EndPoint;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = easeOutExpo(elapsed / duration);
            EndPoint = Vector3.Lerp(start, target, t);
            CurrentLength = Vector3.Distance(_player.transform.position, EndPoint);
            yield return null;
        }

        EndPoint = target;
        _activeRoutine = null;
    }

    private IEnumerator RetractTongueRoutine(float duration)
    {
        SetMode(TongueMode.Extending);
        float startLength = CurrentLength;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = easeOutExpo(elapsed / duration);

            CurrentLength = Mathf.Lerp(startLength, TongueLength, t);
            EndPoint = _player.transform.position + Direction * CurrentLength;
            yield return null;
        }

        _activeRoutine = null;
        AimTongue();
    }

    private IEnumerator ExtendTongueToLengthRoutine(float targetLength, float duration)
    {

        SetMode(TongueMode.Extending);
        Vector3 target = _player.transform.position + Direction.normalized * targetLength;
        float startLength = CurrentLength;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = easeOutExpo(elapsed / duration);
            CurrentLength = Mathf.Lerp(startLength, targetLength, t);
            EndPoint = _player.transform.position + Direction * CurrentLength;
            yield return null;
        }
        CurrentLength = targetLength;
        _activeRoutine = null;
    }

    private IEnumerator ExtendAndRetractRoutine(Vector3 start, Vector3 end, float duration)
    {
        ExtendTongue(end, duration);
        while (_activeRoutine != null)
        {
            yield return null;
        }
        RetractTongue(duration);
        while (_activeRoutine != null)
        {
            yield return null;
        }
    }

#if ENABLE_AIM
    private Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        return Physics.Raycast(ray, out RaycastHit hit, 100f, _aimPlaneMask)
            ? hit.point
            : _player.transform.position;
    }
#endif

    public Vector3 GetTongueAimPos()
    {
        return _player.transform.position + Direction.normalized * TongueLength;
    }

    private void UpdateTongue()
    {
        if (!_tongue || !_player) return;
        _tongue.SetPosition(0, _player.transform.position);
        _tongue.SetPosition(1, EndPoint);
    }

    private float easeOutExpo(float x)
    {
        return Mathf.Abs(x - 1f) < Settings.FloatPrecisionThreshold
            ? 1f
            : 1f - Mathf.Pow(2, -10 * x);
    }

#endregion
}