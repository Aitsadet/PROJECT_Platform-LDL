using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("ระยะทางที่ให้ขยับจากจุดที่วาง (ซ้าย-ขวา)")]
    [SerializeField] private float distance = 3f;

    [Header("ความเร็ว")]
    [SerializeField] private float speed = 2.5f;

    private Rigidbody2D rb;
    private float targetX;
    private float minX;
    private float maxX;
    private Vector2 previousPos;
    private Rigidbody2D playerRb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // คำนวณพิกัดอัตโนมัติจากจุดที่มันวางอยู่ตอนเริ่มเกม
        minX = transform.position.x - distance;
        maxX = transform.position.x + distance;

        targetX = maxX;
        previousPos = rb.position;
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = new Vector2(targetX, rb.position.y);
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);

        Vector2 deltaMovement = newPosition - previousPos;
        rb.MovePosition(newPosition);
        previousPos = newPosition;

        if (playerRb != null)
        {
            playerRb.position += deltaMovement;
        }

        if (Mathf.Abs(rb.position.x - targetX) < 0.05f)
        {
            targetX = (targetX == minX) ? maxX : minX;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            if (collision.transform.position.y > transform.position.y)
            {
                playerRb = collision.transform.root.GetComponent<Rigidbody2D>();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            playerRb = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        float startX = Application.isPlaying ? (minX + distance) : transform.position.x;
        float y = transform.position.y;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector2(startX - distance, y), new Vector2(startX + distance, y));
        Gizmos.DrawWireSphere(new Vector2(startX - distance, y), 0.2f);
        Gizmos.DrawWireSphere(new Vector2(startX + distance, y), 0.2f);
    }
}