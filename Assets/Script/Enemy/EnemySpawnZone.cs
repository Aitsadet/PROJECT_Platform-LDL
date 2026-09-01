using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnZone : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;       // ใส่ได้หลายแบบ จะสุ่มเลือกตอน spawn แต่ละตัว

    [Header("Spawn Count")]
    public int minCount = 2;
    public int maxCount = 5;                // สุ่มระหว่าง min-max (รวม max ด้วย)

    [Header("Spawn Timing")]
    public bool spawnOnStart = true;
    public float spawnDelay = 0f;           // หน่วงเวลาก่อน spawn (วินาที) ถ้าอยากให้เกิดหลังเริ่มเกมสักพัก

    [Header("Placement Safety")]
    public LayerMask obstacleLayer;         // Layer ของ Ground/Wall กันไม่ให้ spawn ทับ
    public float obstacleCheckRadius = 0.3f;
    public int maxPlacementAttempts = 10;   // พยายามหาตำแหน่งว่างกี่ครั้งก่อนจะข้ามตัวนั้นไป

    [Header("Zone Visualization")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.25f);

    private BoxCollider2D zoneCollider;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true; // โซนไม่ต้องชนอะไร แค่ใช้กำหนดขอบเขต
        }
    }

    void Start()
    {
        if (spawnOnStart)
        {
            if (spawnDelay > 0f)
                Invoke(nameof(SpawnEnemies), spawnDelay);
            else
                SpawnEnemies();
        }
    }

    public void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"[{name}] ยังไม่ได้ใส่ Enemy Prefabs", this);
            return;
        }

        int spawnCount = Random.Range(minCount, maxCount + 1); // +1 เพราะ Random.Range(int,int) exclusive ปลายบน

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 spawnPos;
            if (TryGetValidSpawnPosition(out spawnPos))
            {
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
                spawnedEnemies.Add(enemy);
            }
        }
    }

    // สุ่มตำแหน่งในโซน แล้วเช็คว่าไม่ทับกำแพง/พื้น ถ้าทับลองใหม่จนครบจำนวนครั้งที่กำหนด
    private bool TryGetValidSpawnPosition(out Vector2 result)
    {
        Bounds bounds = zoneCollider != null
            ? zoneCollider.bounds
            : new Bounds(transform.position, Vector3.one * 2f); // fallback ถ้าลืมใส่ BoxCollider2D

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 candidate = new Vector2(x, y);

            bool blocked = Physics2D.OverlapCircle(candidate, obstacleCheckRadius, obstacleLayer);
            if (!blocked)
            {
                result = candidate;
                return true;
            }
        }

        result = Vector2.zero;
        return false; // หาตำแหน่งว่างไม่เจอ ข้ามการ spawn ตัวนี้ไป
    }

    // เผื่ออยากเรียก spawn ใหม่ทีหลัง (เช่น ล้างศัตรูเก่าแล้ว spawn รอบใหม่)
    public void ClearSpawnedEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        spawnedEnemies.Clear();
    }

    void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.offset, box.size);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireCube(box.offset, box.size);
    }
}