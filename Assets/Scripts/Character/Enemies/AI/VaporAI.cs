using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class VaporAI : StateMachine
{
    private VaporEnemyEnvironmentalContext _vaporEnvironmentalCtx;
    private VaporEnemyInternalState _vaporInternalCtx;
    private List<ISensor> _sensorList = new List<ISensor>();
    [SerializeField] private float _wallCheckDistance = 0.51f;
    [SerializeField] private LayerMask _wallLayer;
    private WallSensor _wallSensor;

    [SerializeField] private float _groundCheckDistance = 0.51f;
    [SerializeField] private LayerMask _groundLayer;
    private GroundSensor _groundSensor;

    private List<IActuator> _actuatorList = new List<IActuator>();
    private MoveActuator _moveActuactor;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector3 _moveDirection;
    private event UnityAction _onDeath;
    private WallDeathActuator _wallDeathActuator;

    private ISpawnInfo _owner;
    private ObjectPool<VaporAI> _pool;

    private void Awake()
    {
        CurrentState = new VaporMoveState(this);
        UpdateType = UpdateType.Game;
        _vaporEnvironmentalCtx = new VaporEnemyEnvironmentalContext();
        _vaporInternalCtx = new VaporEnemyInternalState(_moveSpeed, _moveDirection);
        _groundSensor = new GroundSensor(_groundCheckDistance, _groundLayer);
        _sensorList.Add(_groundSensor);
        _moveActuactor = new MoveActuator();
        _actuatorList.Add(_moveActuactor);
        _wallSensor = new WallSensor(_wallCheckDistance, _wallLayer);
        _sensorList.Add(_wallSensor);
        _wallDeathActuator = new WallDeathActuator(_vaporEnvironmentalCtx, GetComponent<Health>(), gameObject);
        _actuatorList.Add(_wallDeathActuator);
    }

    public void UpdateExternalState()
    {
        foreach (var sensor in _sensorList)
        {
            sensor.Sense(_vaporEnvironmentalCtx);
        }
    }

    public void UpdateInternalState()
    {
        foreach (var actuator in _actuatorList)
        {
            actuator.Act(_vaporInternalCtx);
        }
    }

    public VaporEnemyInternalState GetInternalInfo()
    {
        return _vaporInternalCtx;
    }

    public VaporEnemyEnvironmentalContext GetExternalInfo()
    {
        return _vaporEnvironmentalCtx;
    }

    public void SetPosition()
    {
        _vaporEnvironmentalCtx.TransformPosition = this.transform;
        _vaporInternalCtx.TransformPosition = this.transform;
    }

    public void Initialize(ISpawnInfo teapotOwner, ObjectPool<VaporAI> poolRef, bool isPooled)
    {
        _owner = teapotOwner;
        _pool = poolRef;
        if (isPooled)
        {
            gameObject.GetComponent<Health>().OnDeath += VaporDeath;
        }
    }

    private void VaporDeath(GameObject deadVapor)
    {
        _owner.minionsSpawned--;
        GetComponent<Health>().ResetHealth();
        GetComponent<Health>().OnDeath -= VaporDeath;
        _pool.Return(this);
    }
}
