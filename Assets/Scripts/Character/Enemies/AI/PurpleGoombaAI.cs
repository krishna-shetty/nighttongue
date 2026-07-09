using UnityEngine;
using System.Collections.Generic;

public class PurpleGoombaAI : StateMachine
{
    private PGoombaEnemyEnvironmentalContext _goombaExternalCtx;
    private PGoombaEnemyInternalState _goombaInternalCtx;
    private List<ISensor> _sensorList = new List<ISensor>();
    private List<IActuator> _idleActuatorList = new List<IActuator>();
    private List<IActuator> _chaseActuatorList = new List<IActuator>();
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private bool DontPatrolOnIdle;
    private MoveActuator _moveActuactor;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector3 _moveDirection;
    private EdgeSensor _edgeSensor;
    [SerializeField] private float wallCheckDistance;
    private WallSensor _wallSensor;
    private TurnActuator _turnActuator;
    private DirectionSensor _directionSensor;
    [SerializeField] private float _viewDistance = 5f;
    [SerializeField] private Transform _target;
    private ChaseActuator _chaseActuator;
    private StopActuator _stopActuator;



    private void Awake()
    {
        CurrentState = new PGoombaIdleState(this);
        UpdateType = UpdateType.Game;
        _goombaExternalCtx = new PGoombaEnemyEnvironmentalContext(gameObject);
        _goombaInternalCtx = new PGoombaEnemyInternalState(_moveSpeed, _moveDirection);
        _edgeSensor = new EdgeSensor(_groundLayer);
        _sensorList.Add(_edgeSensor);
        _wallSensor = new WallSensor(wallCheckDistance, _groundLayer);
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
        _directionSensor = new DirectionSensor(_viewDistance, _target);
        _sensorList.Add(_directionSensor);
        if (DontPatrolOnIdle)
        {
            _goombaInternalCtx.MoveSpeed = 0;
        }
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

    public PGoombaEnemyInternalState GetInternalInfo()
    {
        return _goombaInternalCtx;
    }

    public PGoombaEnemyEnvironmentalContext GetExternalInfo()
    {
        return _goombaExternalCtx;
    }

    public void SetPosition()
    {
        _goombaExternalCtx.TransformPosition = this.transform;
        _goombaInternalCtx.TransformPosition = this.transform;
    }
}
