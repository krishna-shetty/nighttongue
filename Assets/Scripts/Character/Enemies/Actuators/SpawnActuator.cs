using UnityEngine;

public class SpawnActuator : IActuator
{
    private ObjectPool<VaporAI> _vaporPool;
    private bool _debugLeftDeath;
    private float _currTimeBetweenSpawns = 0f;
    private int _maxBunch, _minBunch;
    private float _bunchTime;
    private bool _isSpawning = false;
    private int _batchSpawned = 0;
    private int _batchSpawnNumber = 0;
    public SpawnActuator(ObjectPool<VaporAI> vaporPool, int maxBunch, int minBunch, float bunchTime)
    {
        this._vaporPool = vaporPool;
        _minBunch = minBunch;
        _maxBunch = maxBunch;  
        _bunchTime = bunchTime;
    }
    public void Act(IContext context)
    {
        if (context is ISpawnInfo spawnInfo)
        {
            if (_isSpawning)
            {
                _currTimeBetweenSpawns += Time.deltaTime;
                if (_batchSpawned < _batchSpawnNumber && _currTimeBetweenSpawns >= _bunchTime)
                {
                    VaporAI vapor = _vaporPool.Get(spawnInfo.TransformPosition.position, Quaternion.identity);
                    vapor.transform.position = spawnInfo.TransformPosition.position;
                    vapor.Initialize(spawnInfo, _vaporPool, true);
                    spawnInfo.minionsSpawned++;
                    _currTimeBetweenSpawns = 0f;
                    _batchSpawned++;
                }
                else if(_batchSpawned >= _batchSpawnNumber)
                {
                    _isSpawning = false;
                    spawnInfo.timeSinceLastSpawn = 0f;
                    _currTimeBetweenSpawns = 0f;
                }
            }
            else
            {
                spawnInfo.timeSinceLastSpawn += Time.deltaTime;
                if ((spawnInfo.timeSinceLastSpawn >= spawnInfo.minionsSpawnInterval) && (spawnInfo.minionsSpawned < spawnInfo.minionsMaxAtOnce))
                {
                    _isSpawning = true;
                    _batchSpawned = 0;
                    _batchSpawnNumber = Random.Range(_minBunch, _maxBunch);
                }
            }
        }
    }
}
