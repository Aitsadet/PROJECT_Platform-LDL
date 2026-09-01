using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int spawnCount = 3;

    [Header("Spawn Area (Red Zone)")]
    public Vector2 areaSize = new Vector2(5f, 3f);

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        float randomX = Random.Range(-areaSize.x / 2, areaSize.x / 2);
        float randomY = Random.Range(-areaSize.y / 2, areaSize.y / 2);
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(randomX, randomY);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(areaSize.x, areaSize.y, 1f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 1f));
    }
}