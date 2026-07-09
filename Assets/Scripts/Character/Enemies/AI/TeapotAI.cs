using UnityEngine;
using System.Collections.Generic;

public class TeapotAI : StateMachine
{
    private TeapotEnemyEnvironmentalContext _teapotExternalCtx;
    private TeapotEnemyInternalState _teapotInternalCtx;
    [SerializeField] public GameObject _minionPrefab;
    [SerializeField] public float _minionsSpawnInterval;
    [SerializeField] public int _minionsMaxAtOnce;
    private List<ISensor> _sensorList = new List<ISensor>();
    private DirectionSensor _directionSensor;
    private List<IActuator> _actuatorList = new List<IActuator>();
    [SerializeField] private float _viewDistance = 5f;
    [SerializeField] private Transform _target;
    private FacingActuator _facingActuator;
    private SpawnActuator _spawnActuator;
    private ObjectPool<VaporAI> _vaporPool;
    [SerializeField] private int _minVaporBunch, _maxVaporBunch;
    [SerializeField] private float _timeBetweenVaporInBunch;

    private void Awake()
    {
        CurrentState = new TeapotSpawnState(this);
        UpdateType = UpdateType.Game;
        _teapotExternalCtx = new TeapotEnemyEnvironmentalContext();
        _teapotInternalCtx = new TeapotEnemyInternalState(_minionPrefab, _minionsSpawnInterval, _minionsMaxAtOnce);
        _vaporPool = new ObjectPool<VaporAI>(_minionPrefab.GetComponent<VaporAI>(), _minionsMaxAtOnce);
        _directionSensor = new DirectionSensor(_viewDistance, _target);
        _sensorList.Add(_directionSensor);
        _facingActuator = new FacingActuator(_teapotExternalCtx);
        _actuatorList.Add(_facingActuator);
        _spawnActuator = new SpawnActuator(_vaporPool, _maxVaporBunch, _minVaporBunch, _timeBetweenVaporInBunch);
        _actuatorList.Add(_spawnActuator);
    }

    public void UpdateExternalState()
    {
        foreach (var sensor in _sensorList)
        {
            sensor.Sense(_teapotExternalCtx);
        }
    }

    public void UpdateInternalState()
    {
        foreach (var actuator in _actuatorList)
        {
            actuator.Act(_teapotInternalCtx);
        }
    }

    public TeapotEnemyInternalState GetInternalInfo()
    {
        return _teapotInternalCtx;
    }

    public TeapotEnemyEnvironmentalContext GetExternalInfo()
    {
        return _teapotExternalCtx;
    }

    public void SetPosition()
    {
        _teapotExternalCtx.TransformPosition = this.transform;
        _teapotInternalCtx.TransformPosition = this.transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _viewDistance);
    }
}
