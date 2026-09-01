using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float lifeTime = 3f;
    public int damage = 1;

    [Header("Collision")]
    public LayerMask hittableLayers;     // รวม Layer ของ Ground + Wall (และ Enemy ถ้าต้องการ) ไว้ในนี้

    private Rigidbody2D rb;
    private Vector2 direction;
    private float speed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 dir, float bulletSpeed)
    {
        direction = dir;
        speed = bulletSpeed;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    void Update()
    {
        if (rb == null)
        {
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // เช็คว่าชนกับ Layer ที่กำหนดไว้ไหม (Ground, Wall ฯลฯ)
        bool isHittableLayer = ((1 << other.gameObject.layer) & hittableLayers) != 0;

        if (isHittableLayer)
        {
            Destroy(gameObject);
            return;
        }

        // เช็คศัตรูแยกด้วย Tag เหมือนเดิม (เผื่ออยากส่ง damage)
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}