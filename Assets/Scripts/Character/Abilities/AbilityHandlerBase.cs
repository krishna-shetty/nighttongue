using System;
using System.Collections.Generic;
using UnityEngine;

// dumping these structs here but they should be moved later
public struct MovementStateIntent
{
    public MovementHandlerType TargetState;
    public AbilitySO ActiveAbility;
    public AbilitySO OwnerAbility;
    public object Data; // extra data to switch states i.e. grapple point
    public MovementStateIntent(MovementHandlerType targetState, AbilitySO ability, AbilitySO owner = null, object data = null)
    {
        TargetState = targetState;
        ActiveAbility = ability;
        OwnerAbility = owner ?? ability;
        Data = data;
    }
}

// example of how modifiers can be passed
public struct MovementModifiers
{
    public float GroundSpeedScale;
    public float GroundAccelScale;
    public float JumpMultiplier;
    public MovementModifiers(float groundSpeedScale = 1, float groundAccelScale = 1, float jumpMultiplier = 1)
    {
        GroundSpeedScale = groundSpeedScale;
        GroundAccelScale = groundAccelScale;
        JumpMultiplier = jumpMultiplier;
    }
}

public struct AbilityStartResult
{
    public bool ActivationSuccessful;
    public MovementModifiers? MoveModifiers;
    public MovementStateIntent? MoveState;
    public AbilityStartResult(bool successful, MovementModifiers? modifiers = null, MovementStateIntent? moveState = null)
    {
        ActivationSuccessful = successful;
        MoveModifiers = modifiers;
        MoveState = moveState;
    }
}

public abstract class AbilityHandlerBase : MonoBehaviour
{
    protected AbilityUser _abilityUser;
    protected AbilitySO _uncastActiveAbility;
    protected PlayerController _playerController;
    protected PlayerInputHandler _inputHandler;
    protected GameObject _tongue;
    protected TongueController _tongueController;
    public virtual bool NeedsTick => false;

    public virtual void Initialize(AbilityUser user, PlayerController controller, PlayerInputHandler inputHandler,
        GameObject tongue, TongueController tongueController, List<AbilitySO> abilities = null)
    {
        _abilityUser = user;
        _playerController = controller;
        _inputHandler = inputHandler;
        _tongue = tongue;
        _tongueController = tongueController;
    }

    public virtual AbilityStartResult? Toggle(AbilitySO ability)
    {
        _uncastActiveAbility = ability;
        return null;
    }

    public virtual AbilityStartResult? Hold(AbilitySO ability)
    {
        _uncastActiveAbility = ability;
        return null;
    }

    /// <summary>
    /// Every handler implementation should call this at some point (usually at the end)
    /// </summary>
    public virtual void Release()
    {
        _abilityUser.InvokeOnAbilityCancel(_uncastActiveAbility);
    }

    public virtual void Tick() { }

    public struct PreCheckResult
    {
        public bool CanActivate;
        public Collider TargetPoint;
        public PreCheckResult(bool canActivate, Collider valid = null)
        {
            CanActivate = canActivate;
            TargetPoint = valid;
        }
    }
    public abstract class TargetAbilityHandlerBase : AbilityHandlerBase
    {
        public abstract PreCheckResult PreCheck(AbilitySO ability);
        protected PreCheckResult DoTargetPreCheck(float maxDistance, float maxAngle, LayerMask mask)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxDistance, mask);

            Collider bestCollider = null;
            Vector3 bestPoint = default;
            float bestDistance = float.PositiveInfinity;

            foreach (var collider in hitColliders)
            {
                Vector3 point = collider.ClosestPoint(transform.position);
                Vector3 toPoint = point - transform.position;
                float distance = toPoint.magnitude;
                if (distance <= 0.001f) continue;

                Vector3 direction = toPoint / distance;
                float angle = Vector3.Angle(transform.up, direction);
                if (angle > maxAngle) continue;

                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
                {
                    if (hit.collider != collider) continue;

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCollider = collider;
                        bestPoint = hit.point;
                    }
                }
            }

            return new PreCheckResult(bestCollider != null, bestCollider);
        }
    }
}
