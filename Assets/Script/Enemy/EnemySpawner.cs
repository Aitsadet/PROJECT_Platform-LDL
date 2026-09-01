using UnityEngine;
using System.Collections.Generic; // ต้องใช้ตัวนี้เพื่อใช้ List

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int spawnCount = 3;

    [Header("Spawn Area (Red Zone)")]
    public Vector2 areaSize = new Vector2(5f, 3f);

    [Header("Spacing Settings")]
    public float minDistance = 1.5f; // กำหนดระยะห่างขั้นต่ำของแต่ละตัว ปรับเพิ่มลดได้ใน Unity
    public int maxAttempts = 15;     // จำนวนครั้งที่พยายามสุ่มหาที่ว่าง (ป้องกันเกมค้างถ้ากล่องเล็กไป)

    // ตัวแปรสำหรับจำตำแหน่งที่ศัตรูเกิดไปแล้ว
    private List<Vector2> spawnedPositions = new List<Vector2>();

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPos = Vector2.zero;
        bool positionFound = false;

        // สุ่มหาตำแหน่งใหม่ (ทำซ้ำสูงสุดไม่เกิน maxAttempts เพื่อกัน Infinite Loop)
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomX = Random.Range(-areaSize.x / 2, areaSize.x / 2);
            float randomY = Random.Range(-areaSize.y / 2, areaSize.y / 2);
            spawnPos = (Vector2)transform.position + new Vector2(randomX, randomY);

            bool isTooClose = false;

            // เช็คระยะห่างกับศัตรูตัวอื่นๆ ที่เกิดไปก่อนหน้านี้
            foreach (Vector2 pos in spawnedPositions)
            {
                if (Vector2.Distance(spawnPos, pos) < minDistance)
                {
                    isTooClose = true; // ถ้าเกิดใกล้กันเกินไปให้สุ่มใหม่
                    break;
                }
            }

            // ถ้าตำแหน่งนี้เว้นระยะห่างพอดี ถือว่าใช้ได้ ให้หยุดสุ่ม
            if (!isTooClose)
            {
                positionFound = true;
                break;
            }
        }

        // ถ้าสุ่มได้ตำแหน่งที่ดี ให้สร้างศัตรูและบันทึกตำแหน่งไว้
        if (positionFound)
        {
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            spawnedPositions.Add(spawnPos);
        }
        else
        {
            Debug.LogWarning("พยายามสุ่มที่เกิดแล้วแต่พื้นที่เล็กเกินไป หรือจำนวนศัตรูเยอะไปจนไม่มีที่ให้เว้นระยะห่าง");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(areaSize.x, areaSize.y, 1f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 1f));
    }
}