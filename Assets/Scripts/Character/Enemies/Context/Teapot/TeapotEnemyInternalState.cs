using UnityEngine;

public class TeapotEnemyInternalState : ISpawnInfo
{
    public Transform TransformPosition { get; set; }
    public int minionsSpawned { get; set; }
    public GameObject minion { get; set; }
    public float minionsSpawnInterval { get; set; }
    public int minionsMaxAtOnce { get; set; }
    public float timeSinceLastSpawn { get; set; }

    public TeapotEnemyInternalState(GameObject minionPrefab, float spawnInterval, int maxMinions)
    {
        minion = minionPrefab;
        minionsSpawnInterval = spawnInterval;
        minionsMaxAtOnce = maxMinions;
        minionsSpawned = 0;
        timeSinceLastSpawn = 0;
    }
}
