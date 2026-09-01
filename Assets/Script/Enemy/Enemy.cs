using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Health & Damage")]
    public int maxHealth = 3;
    private int currentHealth;
    public int damageToPlayer = 1;

    [Header("Vision & Movement")]
    public float moveSpeed = 3f;
    public float visionRange = 7f;
    public LayerMask obstacleLayer; // อย่าลืมติ๊ก Layer Ground และ Wall
    private Transform player;
    private bool isChasing = false;

    [Header("Edge & Wall Check")]
    public Transform checkPoint;
    public float checkDistance = 1f;

    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= visionRange)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distance, obstacleLayer);

            // ถ้าไม่ติดกำแพงแปลว่ามองเห็น
            isChasing = (hit.collider == null);
        }
        else
        {
            isChasing = false;
        }
    }

    void FixedUpdate()
    {
        if (isChasing && player != null)
        {
            ChasePlayer();
        }
        else
        {
            // หยุดเดินเมื่อไม่เห็นผู้เล่น
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void ChasePlayer()
    {
        // หาว่าผู้เล่นอยู่ซ้าย (-1) หรือขวา (1)
        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);

        // เนื่องจากโมเดลตั้งต้นหน้าสีขาวหันไปทางซ้าย
        // เดินซ้าย (direction = -1) -> Scale X ต้องเป็น 1
        // เดินขวา (direction = 1) -> Scale X ต้องเป็น -1 เพื่อพลิกหน้ากลับ
        transform.localScale = new Vector3(-directionToPlayer * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // เช็คพื้นและกำแพงด้านหน้า (CheckPoint จะย้ายฝั่งตาม Scale ให้อัตโนมัติ)
        bool isGroundAhead = Physics2D.Raycast(checkPoint.position, Vector2.down, checkDistance, obstacleLayer);
        bool isWallAhead = Physics2D.Raycast(checkPoint.position, new Vector2(directionToPlayer, 0), checkDistance, obstacleLayer);

        if (isGroundAhead && !isWallAhead)
        {
            rb.linearVelocity = new Vector2(directionToPlayer * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ทำดาเมจใส่ผู้เล่นเมื่อเดินชน
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth pHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (pHealth != null) pHealth.TakeDamage(damageToPlayer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        if (checkPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(checkPoint.position, Vector2.down * checkDistance);

            // วาดเส้นเช็คกำแพง (ทิศอิงตามการหันหน้าปัจจุบัน)
            float currentFacing = transform.localScale.x > 0 ? -1f : 1f;
            Gizmos.DrawRay(checkPoint.position, new Vector2(currentFacing, 0) * checkDistance);
        }
    }
}