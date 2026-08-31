using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
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

    [Header("Wall Jump / Slide Settings")]
    public float wallCheckDistance = 0.5f;   // ระยะห่างจากตัวละครไปด้านข้าง (ตามทิศที่หัน) ที่จะตรวจกำแพง
    public float wallCheckYOffset = 0f;      // ปรับสูง-ต่ำของจุดตรวจ ถ้าอยากให้สูง/ต่ำกว่ากึ่งกลางตัวละคร
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;
    public float wallJumpForceX = 8f;
    public float wallJumpForceY = 10f;
    public float wallJumpLockTime = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float horizontalInput;
    private bool isGrounded;
    private bool facingRight = true;

    private int jumpCount;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumping;

    private bool isDashing;
    private bool canDash = true;
    private float dashTimeCounter;
    private float dashCooldownCounter;
    private float originalGravityScale;

    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallJumpLockCounter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        originalGravityScale = rb.gravityScale;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

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

        // --- นับเวลา wall jump lock ---
        if (wallJumpLockCounter > 0f)
        {
            wallJumpLockCounter -= Time.deltaTime;
        }

        // --- รับ Input การเดิน ---
        horizontalInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontalInput = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontalInput = 1f;

        // อย่าเพิ่งกลับด้าน sprite ถ้ากำลังล็อกจาก wall jump
        if (wallJumpLockCounter <= 0f)
        {
            if (horizontalInput > 0) { sr.flipX = false; facingRight = true; }
            else if (horizontalInput < 0) { sr.flipX = true; facingRight = false; }
        }

        // --- เช็คพื้น ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- เช็คกำแพง (ตำแหน่งคำนวณสดๆ ตามทิศที่หัน ไม่ต้องมี GameObject ลูก) ---
        Vector2 wallCheckPos = GetWallCheckPosition();
        isTouchingWall = Physics2D.OverlapCircle(wallCheckPos, wallCheckRadius, wallLayer);

        bool pushingIntoWall = (facingRight && horizontalInput > 0f) || (!facingRight && horizontalInput < 0f);
        isWallSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y < 0f && pushingIntoWall;

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

        if (jumpBufferCounter > 0f && isWallSliding)
        {
            DoWallJump();
        }
        else if (jumpBufferCounter > 0f && (canCoyoteJump || canAirJump))
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
        if (isDashing)
        {
            float dashDirection = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            return;
        }

        if (wallJumpLockCounter > 0f)
        {
            // ปล่อยตามแรงดีดจาก wall jump ไม่ให้ input แทรก
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
        else
        {
            if (rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0f && !IsSpaceHeld())
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }
    }

    // คำนวณตำแหน่งจุดตรวจกำแพง สลับข้างอัตโนมัติตามทิศที่ตัวละครหันอยู่
    private Vector2 GetWallCheckPosition()
    {
        float direction = facingRight ? 1f : -1f;
        return (Vector2)transform.position + new Vector2(wallCheckDistance * direction, wallCheckYOffset);
    }

    private void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpCount--;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        isJumping = true;
    }

    private void DoWallJump()
    {
        float jumpDirection = facingRight ? -1f : 1f;

        rb.linearVelocity = new Vector2(jumpDirection * wallJumpForceX, wallJumpForceY);

        facingRight = jumpDirection > 0f;
        sr.flipX = !facingRight;

        jumpCount = maxJumpCount - 1;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        isJumping = true;
        wallJumpLockCounter = wallJumpLockTime;
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

        // วาดจุดตรวจกำแพงตามตำแหน่งที่คำนวณสดๆ (ทำงานได้แม้ตอนไม่ Play เพราะใช้ facingRight ที่เซฟไว้ล่าสุด)
        Gizmos.color = Color.blue;
        Vector2 wallPos = Application.isPlaying
            ? GetWallCheckPosition()
            : (Vector2)transform.position + new Vector2(wallCheckDistance, wallCheckYOffset);
        Gizmos.DrawWireSphere(wallPos, wallCheckRadius);
    }
}