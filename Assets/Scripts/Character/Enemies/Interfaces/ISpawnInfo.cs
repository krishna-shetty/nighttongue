using UnityEngine;

public interface ISpawnInfo : IContext
{
    Transform TransformPosition { get; set; }
    GameObject minion { get; set; }
    int minionsSpawned { get; set; }
    float minionsSpawnInterval { get; set; }
    int minionsMaxAtOnce { get; set; }
    float timeSinceLastSpawn { get; set; }
}
