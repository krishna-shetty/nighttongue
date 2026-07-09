using UnityEngine;
using System.Collections.Generic;

public class YellowGoombaAI : StateMachine
{
    private YGoombaEnemyEnvironmentalContext _goombaExternalCtx;
    private YGoombaEnemyInternalState _goombaInternalCtx;
    private List<ISensor> _sensorList = new List<ISensor>();
    private List<IActuator> _idleActuatorList = new List<IActuator>();
    private List<IActuator> _chaseActuatorList = new List<IActuator>();
    [SerializeField] private LayerMask _groundLayer;
    public bool DontPatrolOnIdle;
    private MoveActuator _moveActuactor;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector3 _moveDirection;
    private EdgeSensor _edgeSensor;
    [SerializeField] private float _wallCheckDistance;
    private WallSensor _wallSensor;
    private TurnActuator _turnActuator;
    private DirectionSensor _directionSensor;
    [SerializeField] private float _viewDistance = 5f;
    [SerializeField] private Transform _target;
    private ChaseActuator _chaseActuator;
    private StopActuator _stopActuator;
    private DangerActuator _dangerActuator;
    [SerializeField] private float _dangerDuration;
    [SerializeField] private float _vulnerabilityDuration;
    [SerializeField] private float transformDelay;
    [SerializeField] private BoolRef _dangerForm;


    private void Awake()
    {
        CurrentState = new YGoombaIdleState(this);
        UpdateType = UpdateType.Game;
        _goombaExternalCtx = new YGoombaEnemyEnvironmentalContext(gameObject);
        _goombaInternalCtx = new YGoombaEnemyInternalState(_moveSpeed, _moveDirection);
        _edgeSensor = new EdgeSensor(_groundLayer);
        _sensorList.Add(_edgeSensor);
        _wallSensor = new WallSensor(_wallCheckDistance, _groundLayer);
        _sensorList.Add(_wallSensor);
        _turnActuator = new TurnActuator(_goombaExternalCtx, _goombaExternalCtx);
        _idleActuatorList.Add(_turnActuator);
        _chaseActuator = new ChaseActuator(_goombaExternalCtx);
        _idleActuatorList.Add(_chaseActuator);
        _chaseActuatorList.Add(_chaseActuator);
        _stopActuator = new StopActuator(_goombaExternalCtx, _goombaExternalCtx, _moveSpeed);
        _chaseActuatorList.Add(_stopActuator);
        _moveActuactor = new MoveActuator();
        _idleActuatorList.Add(_moveActuactor);
        _chaseActuatorList.Add(_moveActuactor);
        _dangerActuator = new DangerActuator(GetComponent<DamageReceiver>(), _dangerDuration, _vulnerabilityDuration, GetComponent<Renderer>(), transformDelay, _dangerForm, _moveSpeed);
        _idleActuatorList.Add(_dangerActuator);
        _chaseActuatorList.Add(_dangerActuator);
        _directionSensor = new DirectionSensor(_viewDistance, _target);
        _sensorList.Add(_directionSensor);
        if (DontPatrolOnIdle)
        {
            _goombaInternalCtx.MoveSpeed = 0;
        }
        GetComponent<Renderer>().material.color = Color.yellow;
    }


    public void UpdateExternalState()
    {
        foreach (var sensor in _sensorList)
        {
            sensor.Sense(_goombaExternalCtx);
        }
    }

    public void UpdateIdleInternalState()
    {
        foreach (var actuator in _idleActuatorList)
        {
            actuator.Act(_goombaInternalCtx);
        }
    }

    public void UpdateChaseInternalState()
    {
        foreach (var actuator in _chaseActuatorList)
        {
            actuator.Act(_goombaInternalCtx);
        }
    }

    public YGoombaEnemyInternalState GetInternalInfo()
    {
        return _goombaInternalCtx;
    }

    public YGoombaEnemyEnvironmentalContext GetExternalInfo()
    {
        return _goombaExternalCtx;
    }

    public void SetPosition()
    {
        _goombaExternalCtx.TransformPosition = this.transform;
        _goombaInternalCtx.TransformPosition = this.transform;
    }
}
[System.Serializable]
public class BoolRef
{
    public bool value;
}
