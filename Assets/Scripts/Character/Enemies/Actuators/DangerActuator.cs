using UnityEngine;

public class DangerActuator : IActuator
{
    private DamageReceiver _damageReceiver;
    private float _dangerDuration;
    private float _vulnerabilityDuration;
    private float _dangerTimer;
    private Renderer _renderer;
    private float _transformDelay;
    private BoolRef _dangerForm;
    private float _originalSpeed;
    private float _bufferTimer = 0f;
    public DangerActuator(DamageReceiver damageReceiver, float dangerDuration, float vulnerabilityDuration, Renderer renderer, float transformDelay, BoolRef dangerForm, float originalSpeed)
    {
        _damageReceiver = damageReceiver;
        _dangerDuration = dangerDuration;
        _vulnerabilityDuration = vulnerabilityDuration;
        _dangerTimer = 0f;
        _renderer = renderer;
        _transformDelay = transformDelay;
        _dangerForm = dangerForm;
        _originalSpeed = originalSpeed;
    }
    public void Act(IContext context)
    {
        if (context is IDangerInfo dangerInfo)
        {
            _dangerTimer += Time.deltaTime;
            if (dangerInfo.isDanger)
            {
                if (_dangerTimer >= _dangerDuration)
                {
                    _damageReceiver.CanBeStomped = true;
                    _renderer.material.color = Color.yellow;
                    _dangerForm.value = false;
                    _bufferTimer += Time.deltaTime;
                    if(_bufferTimer >= _transformDelay)
                    {
                        dangerInfo.isDanger = false;
                        _dangerTimer = 0f;
                        _bufferTimer = 0f;
                        if (context is IMoveInfo moveInfo)
                        {
                            moveInfo.MoveSpeed = _originalSpeed;
                        }
                    }
                }
            }
            else
            {
                if (_dangerTimer >= _vulnerabilityDuration)
                {
                    _bufferTimer += Time.deltaTime;
                    if (context is IMoveInfo moveInfo)
                    {
                        moveInfo.MoveSpeed = 0f;
                    }
                    if (_bufferTimer >= _transformDelay)
                    {
                        _bufferTimer = 0f;
                        dangerInfo.isDanger = true;
                        _dangerTimer = 0f;
                        _damageReceiver.CanBeStomped = false;
                        _renderer.material.color = Color.red;
                        _dangerForm.value = true;
                    }
                }
            }
        }
    }
}
