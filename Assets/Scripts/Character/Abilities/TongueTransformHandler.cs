using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TongueTransformHandler : AbilityHandlerBase
{

    private TongueTransformSO _activeAbility;
    private bool _isTransformed = false;
    public bool IsTransformed => _isTransformed;

    [SerializeField] private GameObject _base;
    [SerializeField] private GameObject _transformed;
    [SerializeField] private Transform _tami;
    private PushOffOverhang _pushOffOverhang;
    [SerializeField] private LayerMask _groundLayer;
    private Vector3 _point1;
    private Vector3 _point2;
    private float _capsuleHeight;
    private float _capsuleRadius;
    private float _sphereRadius;
    private Vector3 sphereCenter;
    public TongueTransformSO ActiveAbility => _activeAbility;

    public event Action<TongueTransformEventArgs> OnTransformStateChanged;

    public override void Initialize(AbilityUser user, PlayerController controller, PlayerInputHandler inputHandler,
        GameObject tongue, TongueController tongueController, List<AbilitySO> abilities)
    {
        base.Initialize(user, controller, inputHandler, tongue, tongueController);
        _pushOffOverhang = GetComponent<PushOffOverhang>();
    }

    private void Start()
    {
        (float, float) capsuleSize = MeshSize.GetSize(_base);
        if (_tami == null)
        {
            _tami = _base.transform.Find("tami_referenced_rig");
        }
        float radius = capsuleSize.Item1 * 0.5f;
        _capsuleHeight = Mathf.Max(0f, capsuleSize.Item2 - 2f * radius);
        Vector3 up = Vector3.up * (_capsuleHeight * 0.5f);
        _point1 = -up;
        _point2 = up;
        _capsuleRadius = radius;
        (float, float) sphereSize = MeshSize.GetSize(_transformed);
        _sphereRadius = sphereSize.Item2 / 2;
    }

    public override AbilityStartResult? Toggle(AbilitySO ability)
    {
        if (ability is not TongueTransformSO)
        {
            Debug.LogError("TongueTransformHandler: Toggle must have TongueTransformSO passed in");
            return null;
        }

        base.Toggle(ability);
        _activeAbility = (TongueTransformSO) ability;

        AbilityCooldownManager.UpdateCooldown(ability, _activeAbility.Cooldown);

        if (_isTransformed)
        {
            if (CheckCapsuleHasSpace())
            {
                Release();
            }
        }
        else
        {
            Debug.Log("TongueTransformHandler :: Checking Sphere");
            Debug.Log(CheckSphereHasSpace());
            if (CheckSphereHasSpace()) {
                ExecuteTransform();
                return new AbilityStartResult(true);
            }
        }

        return null;
    }

    public override void Release()
    {
        int sign = _playerController.LastBallMoveSign;

        //Debug.Log("Releasing Tongue Transform");
        _playerController.ResistsWind = false;
        _transformed.SetActive(false);
        _base.SetActive(true);

        Transform model = _tami.transform;
        Vector3 rot = model.localEulerAngles;
        rot.y = sign > 0 ? 90f : 180f;
        model.localEulerAngles = rot;

        _pushOffOverhang.enabled = true;
        _isTransformed = false;
        OnTransformStateChanged?.Invoke(new TongueTransformEventArgs(false, _activeAbility.moveAction, _activeAbility.jumpAction));
        base.Release();
        _playerController.RaiseEndBallAbility(); // SFX
    }

    public void ExecuteTransform()
    {
        OnTransformStateChanged?.Invoke(new TongueTransformEventArgs(true, _activeAbility.moveAction, _activeAbility.jumpAction));
        StartCoroutine(CallTurningBallHandler());
    }

    private IEnumerator CallTurningBallHandler()
    {
        yield return new WaitForSeconds(0.67f);
        TurningBallHandler();
    }

    public void TurningBallHandler()
    {
        if (_activeAbility == null)
        {
            Debug.LogError("TongueTransformHandler :: TurningBallHandler called but _activeAbility is null");
            return;
        }

        _playerController.ResistsWind = true;
        _base.SetActive(false);
        _transformed.SetActive(true);
        _pushOffOverhang.enabled = false;
        _isTransformed = true;
        _playerController.RaiseStartBallAbility();
    }


    //public void UntoggleTransform()
    //{
    //    _transformed.SetActive(false);
    //    _base.SetActive(true);
    //    //_isTransformingOnCooldown = true;
    //    _pushOffOverhang.enabled = true;

    //    _isTransformed = false;
    //    //_transformTimer = 0f;

    //    OnTransformStateChanged?.Invoke(new TongueTransformEventArgs(false, _activeAbility.moveAction, _activeAbility.jumpAction));
    //}

    private bool CheckCapsuleHasSpace()
    {
        // top
        _point1 = transform.position + Vector3.up * _capsuleHeight;
        // bottom
        _point2 = transform.position;
        return !Physics.CheckCapsule(_point1, _point2, _capsuleRadius, _groundLayer);
    }

    private bool CheckSphereHasSpace()
    {
        sphereCenter = transform.position + Vector3.down * (_capsuleHeight / 2 + _capsuleRadius) + Vector3.up * _sphereRadius;
        _point1 = transform.position - Vector3.down * _sphereRadius + Vector3.up * (_capsuleHeight - _capsuleRadius);
        _point2 = transform.position - Vector3.down * _sphereRadius + Vector3.up * _capsuleRadius;
        //Debug.Log("point 1: " + _point1 + "\npoint 2: " + _point2);
        return !Physics.CheckSphere(sphereCenter, _sphereRadius - 0.001f, _groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (_isTransformed)
        {
            Gizmos.DrawSphere(sphereCenter, _sphereRadius);
        }
        else
        {
            Gizmos.DrawSphere(_point1, _capsuleRadius);
            Gizmos.DrawSphere(_point2, _capsuleRadius);
        }
    }
}
