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

        // --- รับ Input การเดิน ---
        horizontalInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontalInput = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontalInput = 1f;

        // --- หันหน้าด้วย Scale (แบบ 2) ---
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            facingRight = true;
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            facingRight = false;
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
        if (isDashing)
        {
            float dashDirection = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            return;
        }

        // --- เดินซ้าย-ขวา ---
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // --- ปรับ Gravity ---
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !IsSpaceHeld())
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

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
}