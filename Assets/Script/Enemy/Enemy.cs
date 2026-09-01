using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Chase Player")]
    public float moveSpeed = 2f;
    public float detectionRange = 6f;    // ระยะที่เริ่มไล่ตาม Player
    public float stopDistance = 0.5f;    // ระยะห่างที่หยุดไล่ (กันเดินทับ Player)

    [Header("Contact Damage (ทางเลือก)")]
    public bool dealContactDamage = true;
    public int contactDamage = 1;
    public float contactCooldown = 1f;   // กันโดนดาเมจรัวทุกเฟรมตอนชนติดกัน

    [Header("References")]
    public LayerMask obstacleLayer;      // (ทางเลือก) ใช้เผื่ออนาคตถ้าจะเช็คแนวขวางระหว่างไล่

    private Rigidbody2D rb;
    private Transform player;
    private float contactCooldownCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        // หา Player ในฉากด้วย Tag (ต้องตั้ง Tag "Player" ที่ตัวละครผู้เล่นไว้ก่อน)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"[{name}] หา GameObject ที่มี Tag 'Player' ไม่เจอ ตรวจสอบว่าตั้ง Tag ให้ Player แล้วหรือยัง", this);
        }
    }

    void Update()
    {
        if (contactCooldownCounter > 0f)
        {
            contactCooldownCounter -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // อยู่นอกระยะตรวจจับ หรือใกล้ Player เกินไปแล้ว -> หยุดนิ่ง
        if (distance > detectionRange || distance <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    // เรียกจากภายนอก เช่น Bullet.cs ตอนกระสุนชน
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // จุดนี้ใส่ effect/animation/เสียงตายเพิ่มทีหลังได้ เช่น Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!dealContactDamage) return;
        if (contactCooldownCounter > 0f) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // ตัวอย่างเรียก TakeDamage ฝั่ง Player ถ้ามีสคริปต์ PlayerHealth
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage);
            contactCooldownCounter = contactCooldown;
        }
    }

    void OnDrawGizmosSelected()
    {
        // แสดงระยะตรวจจับใน Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}