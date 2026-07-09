using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private bool isSpawning = false;
    [SerializeField]
    private GameObject enemyToSpawn;
    [SerializeField]
    private Vector3[] enemySpawnLocations;
    [SerializeField]
    private float spawnCooldown;
    private float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        if (isSpawning)
        {
            timer += Time.deltaTime;
            if (timer >= spawnCooldown)
            {
                SpawnEnemies();
                timer = 0f;
            }
        }
    }

    private void SpawnEnemies()
    {
        foreach(Vector3 currEnemyLocation in enemySpawnLocations)
        {
            Instantiate(enemyToSpawn, currEnemyLocation, Quaternion.identity);
        }
    }

    public void StartSpawning()
    {
        Debug.Log("spawning");
        isSpawning = true;
    }

    public void StopSpawning()
    {
        Debug.Log("stop spawning");
        isSpawning = false;
    }
}
