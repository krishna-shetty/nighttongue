using System;
using System.Collections.Generic;
using UnityEngine;

public class DragHandler : AbilityHandlerBase
{
    private Rigidbody _attachedObject;
    private DraggableObject _draggableComponent;
    private IForceSource _attachedSource;
    private Vector3 _attachedLocalPoint;
    private DragAbilitySO _activeAbility;

    [Header("Refs")]
    private float _tongueLength;
    public LayerMask suctionObstructionMask;

    [Header("Pickup")]
    private float _maxRange = 8f;
    private float _extendTime;
    private float _retractTime;
    private float _pickupRadius = 0.35f;
    public LayerMask pickupMask;

    private bool _isExtending;
    private bool _isAttached;
    private bool _autoRetracting;
    private Vector3 _extendTarget;
    private bool _isRetracting = false;

    private SpringJoint _tongueJoint;
    private RigidbodyConstraints _originalConstraints; 

    public override bool NeedsTick =>
    _isRetracting || _isExtending || _autoRetracting || _isAttached;


    private Vector3 GetHomePos()
    {
        Vector3 p = transform.position + _tongueController.Direction * _tongueController.TongueLength;
        return p;
    }

    public bool IsHoldingObject => _attachedObject != null;

    public override void Initialize(AbilityUser user, PlayerController controller, PlayerInputHandler inputHandler,
        GameObject tongue, TongueController tongueController, List<AbilitySO> abilities)
    {
        base.Initialize(user, controller, inputHandler, tongue, tongueController);
    }

    public override AbilityStartResult? Hold(AbilitySO ability)
    {
        if (ability is not DragAbilitySO)
        {
            Debug.LogError("DragHandler: Hold must have DragAbilitySO passed in");
            return null;
        }

        base.Hold(ability);
        _activeAbility = (DragAbilitySO) ability;

        _maxRange = _activeAbility.DragRange;
        _extendTime = _activeAbility.extendTime;
        _retractTime = _activeAbility.retractTime;

        OnTonguePressed(_activeAbility);

        return new AbilityStartResult(true);
    }

    public override void Release()
    {
        // If extending, retract immediately
        if (_isExtending)
        {
            RetractNow();
            return;
        }
        else if (_attachedObject != null)
        {
            DropAttachedObject();

            RetractNow();
        }
        base.Release();
    }

    public override void Tick()
    {
        if (_playerController.GetCurrentState() is not GroundedState state)
        {
            Release();
        }

        if(_isAttached && _attachedObject.gameObject.activeInHierarchy == false)
        {
            Release();
            return;
        }

        if (_isAttached && _attachedObject != null)
        {
            Vector3 worldAnchor = _attachedObject.transform.TransformPoint(_attachedLocalPoint);
            _tongueController.UpdateEndPoint(worldAnchor);
        }

        // While extending, try to catch object near the tip
        if (_isExtending)
        {
            TryCatchWhileExtending();

            if (IsNear(_tongueController.CurrentLength, _maxRange))
            {
                RetractNow();
            }
        }

        // When auto-retracting and reached player, clear flag
        if (_autoRetracting && IsTipNear(GetHomePos()))
        {
            _autoRetracting = false;
        }

        // when retracting and tongue tip is back at suction point, reset tongue back to Aim mode
        if (_isRetracting && IsTipNear(GetHomePos()))
        {
            _isRetracting = false;
            if (!_attachedObject) Release();
        }
    }


    public void OnTonguePressed(DragAbilitySO ability)
    {
        // Compute extend ray target (blocked by walls)
        Vector3 start = transform.position;
        Vector3 dir = _tongueController.Direction.normalized;

        float range = (_activeAbility != null && _activeAbility.DragRange > 0f)
            ? _activeAbility.DragRange
            : _maxRange;

        _extendTarget = start + dir * range;
        if (Physics.Raycast(start, dir, out RaycastHit hit, range, suctionObstructionMask, QueryTriggerInteraction.Ignore))
            _extendTarget = hit.point;

        float dist = Vector3.Distance(start, _extendTarget);

        _tongueController.ExtendTongueToLength(dist, _extendTime);
        _isExtending = true;
        _autoRetracting = false;
    }

    private void AttachTongue(Rigidbody target, Vector3 hitPoint)
    {
        if (_attachedObject != null) return;
        _tongueController._mode= TongueController.TongueMode.AttachedHolding;

        _attachedObject = target;
        _tongueLength = Vector3.Distance(transform.position, _attachedObject.transform.position);

        _originalConstraints = _attachedObject.constraints;
        target.constraints = RigidbodyConstraints.FreezeRotation;

        // Use SpringJoint instead
        _tongueJoint = _attachedObject.gameObject.AddComponent<SpringJoint>();
        _tongueJoint.connectedBody = GetComponent<Rigidbody>();
        _tongueJoint.autoConfigureConnectedAnchor = false;
        _attachedLocalPoint = _attachedObject.transform.InverseTransformPoint(hitPoint);
        _tongueJoint.anchor = _attachedLocalPoint;
        _tongueJoint.connectedAnchor = Vector3.zero;

        // Set both min and max distance

        Vector3 delta = hitPoint - _tongueController.transform.position;
        float idealDist = delta.magnitude;
        idealDist = Mathf.Max(idealDist, _activeAbility.MinConnectDistance);
        _tongueJoint.minDistance = idealDist;
        _tongueJoint.maxDistance = idealDist + _activeAbility.DragStretchDistance;
        float spring = Mathf.Lerp(_activeAbility.SpringConstantMin, _activeAbility.SpringConstantMax, target.mass);
        _tongueJoint.spring = spring;  
        _tongueJoint.damper = _activeAbility.Damper;    

        _attachedSource = _attachedObject.GetComponent<IForceSource>();
        _playerController.RegisterForceSource(_attachedSource);
        _playerController.RaiseDragAttach();
        _draggableComponent.OnLifted += Release;
        _draggableComponent.PlayAttachedSound();
    }

    private void TryCatchWhileExtending()
    {
        Vector3 tip = _tongueController.EndPoint;

        Vector3 start = _tongueController.transform.position;
        Vector3 fwd = _tongueController.Direction;

        if (Physics.SphereCast(start, _pickupRadius, fwd, out RaycastHit hitInfo, _tongueController.CurrentLength + _pickupRadius, ~0, QueryTriggerInteraction.Ignore))
        {
            var rb = hitInfo.collider.attachedRigidbody;
            if (rb == null) return;
            // Only catch if it has a DraggableObject component
            DraggableObject draggable = rb.GetComponent<DraggableObject>();
            if (draggable == null) return;

            _draggableComponent = draggable;

            // Attach to the object
            AttachTongue(rb, hitInfo.point);

            _isAttached = true;
            _isExtending = false;
            _autoRetracting = false;
            _isRetracting = false;

            _tongueController.StopAllCoroutines();
            _tongueController.SetTongueLength(_tongueLength);
        }
    }

    private void RetractNow()
    {
        _isRetracting = true;
        _isExtending = false;
        _autoRetracting = false;

        _tongueController.RetractTongue(_retractTime);
    }

    private bool IsTipNear(Vector3 worldPos, float eps = 0.03f)
    {
        Vector3 a = _tongueController.EndPoint; 
        Vector3 b = worldPos; 
        return (a - b).sqrMagnitude <= eps * eps;
    }

    private bool IsNear(float a, float b, float eps = 0.03f)
    {
        return Mathf.Abs(a - b) <= eps;
    }

    private void DropAttachedObject()
    {

        if (_attachedObject == null) return;
        _tongueController._mode = TongueController.TongueMode.Aim;
        if (_draggableComponent != null)
        {
            _draggableComponent.OnLifted -= Release;
        }

        _attachedObject.constraints = _originalConstraints;

        _playerController.UnregisterForceSource(_attachedSource);
        _playerController.RaiseDragDetach();
        _attachedSource = null;


        // Destroy the joint
        if (_tongueJoint != null)
        {
            Destroy(_tongueJoint);
            _tongueJoint = null;
        }

        _attachedObject.linearVelocity = Vector3.zero;

        _attachedObject = null;
        _draggableComponent.PlayDetachedSound();
        _draggableComponent = null;
        _isAttached = false;
    }

    // ===== Throw logic =====

    private void OnDrawGizmosSelected()
    {
        if (_tongueController != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_tongueController.EndPoint, _pickupRadius);
        }
    }
}