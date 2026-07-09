using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AbilityHandlerBase;

public class SwingHandler : AbilityHandlerBase
{
    private SwingAbilitySO _activeAbility;
    public LayerMask WhatIsSwingable;
    private Coroutine _swingRoutine;

    private Collider _swingTarget;

    public override void Initialize(
        AbilityUser user,
        PlayerController controller,
        PlayerInputHandler inputHandler,
        GameObject tongue,
        TongueController tongueController,
        List<AbilitySO> abilities)
    {
        base.Initialize(user, controller, inputHandler, tongue, tongueController);
        _playerController.WhatIsSwingable = WhatIsSwingable;
    }

    public PreCheckResult PreCheck(AbilitySO ability)
    {
        if (ability is SwingAbilitySO swing)
        {
            SwingTargetResolver resolver = _playerController.GetComponent<SwingTargetResolver>();

            Collider target = resolver.CurrentTarget;

            if (target == null)
            {
                return new PreCheckResult(false);
            }

            const float epsilon = 0.1f;

            Vector3 targetPos = target.transform.position;
            Vector3 playerPos = _playerController.transform.position;

            Vector3 toSwingPoint = targetPos - playerPos;
            float distance = toSwingPoint.magnitude;
            if (distance <= epsilon) return new PreCheckResult(false);

            Vector3 dir = toSwingPoint / distance;

            Debug.DrawLine(playerPos, playerPos + dir * (swing.MaxRappleDistance + epsilon), Color.cyan, 1.0f);

            if (Physics.Raycast(
                    playerPos,
                    dir,
                    out RaycastHit hit,
                    swing.MaxRappleDistance + epsilon,
                    LayerMask.GetMask("Terrain"),
                    QueryTriggerInteraction.Ignore))
            {
                if (hit.distance < distance)
                {
                    return new PreCheckResult(false);
                }
               
            }

            _swingTarget = target;

            return new PreCheckResult(true, valid: target);
        }
        else
        {
            Debug.LogError("SwingHandler: PreCheck must have SwingAbilitySO passed in");
            return new PreCheckResult(false);
        }
    }

    public override AbilityStartResult? Toggle(AbilitySO ability)
    {
        // Toggle off
        if (_playerController.IsInState<SwingingState>())
        {
            Release();
            return new AbilityStartResult(false);
        }

        if (ability is not SwingAbilitySO swingAbility)
        {
            Debug.LogError("SwingHandler: Toggle must have SwingAbilitySO passed in");
            return null;
        }

        _activeAbility = swingAbility;

        var preCheckResult = PreCheck(_activeAbility);
        if (!preCheckResult.CanActivate)
            return null;

        base.Toggle(ability);

        AbilityCooldownManager.UpdateCooldown(_activeAbility, _activeAbility.Cooldown);

        Vector3 swingPoint = _swingTarget.transform.position;

        _inputHandler.FreezeInput(_activeAbility.DelayTime);

        if (_swingRoutine != null) StopCoroutine(_swingRoutine);
        _swingRoutine = StartCoroutine(DelayedExecuteSwing(_activeAbility.DelayTime));
        _tongueController.ExtendTongue(swingPoint, _activeAbility.DelayTime);

        return new AbilityStartResult(true);
    }

    private IEnumerator DelayedExecuteSwing(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ExecuteSwing();
    }

    public override void Release()
    {
        if (_swingRoutine != null)
        {
            StopCoroutine(_swingRoutine);
            _swingRoutine = null;
        }

        _tongueController.AimTongue();
        _inputHandler.FreezeInput(_activeAbility.DelayTime);
        Invoke(nameof(UntoggleSwing), _activeAbility.DelayTime);

        base.Release();
    }

    private void ExecuteSwing()
    {
        _abilityUser.RequestMovementStateChange(
            new(MovementHandlerType.SWINGING, ability: _activeAbility, owner: _abilityUser.GetActiveAbility(), data: _swingTarget)
        );
    }

    public void UntoggleSwing()
    {
        //Debug.Log("SwingHandler :: Unsuccessful swing.");
    }
}
