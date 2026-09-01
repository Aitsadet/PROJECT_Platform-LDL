using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // ... (ส่วนตัวแปร Header ต่างๆ เหมือนเดิม) ...
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public int maxJumpCount = 2;

    [Header("Jump Feel")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public bool allowAirDash = true;
    public bool resetJumpOnDash = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float horizontalInput;
    private bool isGrounded;
    private bool facingRight = true;

    // --- เพิ่มตัวแปรหน่วงเวลาไม่ให้หันหน้าตามการเดินทันทีหลังยิง ---
    private float aimLockTime = 0.5f;
    private float aimLockCounter = 0f;

    private int jumpCount;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumping;

    private bool isDashing;
    private bool canDash = true;
    private float dashTimeCounter;
    private float dashCooldownCounter;
    private float originalGravityScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        originalGravityScale = rb.gravityScale;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // นับเวลาล็อกการหันหน้า
        if (aimLockCounter > 0)
        {
            aimLockCounter -= Time.deltaTime;
        }

        // --- นับเวลา cooldown ของ dash ---
        if (!canDash)
        {
            dashCooldownCounter -= Time.deltaTime;
            if (dashCooldownCounter <= 0f)
                canDash = true;
        }

        // --- ระหว่าง dash ไม่รับ input การเดิน/กระโดดอื่นๆ ---
        if (isDashing)
        {
            dashTimeCounter -= Time.deltaTime;
            if (dashTimeCounter <= 0f)
            {
                EndDash();
            }
            return;
        }

        // --- รับ Input การเดิน ---
        horizontalInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontalInput = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontalInput = 1f;

        // --- หันหน้าด้วย Scale (จะทำงานก็ต่อเมื่อไม่ได้ถูกล็อคหน้าจากการยิง) ---
        if (aimLockCounter <= 0f)
        {
            if (horizontalInput > 0)
            {
                FaceDirection(1f);
            }
            else if (horizontalInput < 0)
            {
                FaceDirection(-1f);
            }
        }

        // --- เช็คพื้น ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            jumpCount = maxJumpCount;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // --- Jump Buffer ---
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        bool canCoyoteJump = coyoteTimeCounter > 0f;
        bool canAirJump = !canCoyoteJump && jumpCount > 0;

        if (jumpBufferCounter > 0f && (canCoyoteJump || canAirJump))
        {
            DoJump();
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0f && isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        if (isGrounded && rb.linearVelocity.y <= 0f)
        {
            isJumping = false;
        }

        // --- Dash Input ---
        bool shiftPressed = Keyboard.current.leftShiftKey.wasPressedThisFrame;
        if (shiftPressed && canDash && (isGrounded || allowAirDash))
        {
            StartDash();
        }
    }

    void FixedUpdate()
    {
        // ... (ส่วนนี้เหมือนเดิม) ...
        if (isDashing)
        {
            float dashDirection = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !IsSpaceHeld())
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    // ... (ส่วน Jump และ Dash เหมือนเดิม) ...
    private void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpCount--;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        isJumping = true;
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimeCounter = dashDuration;
        dashCooldownCounter = dashCooldown;
        rb.gravityScale = 0f;

        if (resetJumpOnDash)
            jumpCount = maxJumpCount;
    }

    private void EndDash()
    {
        isDashing = false;
        rb.gravityScale = originalGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);
    }

    private bool IsSpaceHeld()
    {
        return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // ================= ฟังก์ชันสำหรับจัดการการหันหน้า =================
    private void FaceDirection(float directionX)
    {
        if (directionX > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            facingRight = true;
        }
        else if (directionX < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            facingRight = false;
        }
    }

    // สคริปต์ยิงปืนจะเรียกใช้คำสั่งนี้เมื่อกดยิง เพื่อบังคับให้หันหน้าตามทิศเมาส์
    public void ForceFaceDirection(float directionX)
    {
        FaceDirection(directionX);
        aimLockCounter = aimLockTime; // ล็อคไม่ให้หันหน้าตามทิศการเดินเป็นเวลาสั้นๆ (0.5วิ) เพื่อให้ดูสมูท
    }
}