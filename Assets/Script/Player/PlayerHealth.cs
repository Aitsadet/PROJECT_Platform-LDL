using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; // ใช้สำหรับ New Input System

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Invincibility (I-Frames)")]
    public float iFramesDuration = 1.5f;
    private bool isInvincible = false;

    [Header("Debug Test")]
    public bool enableDebugKeys = true; // เปิดไว้ทดสอบกดปุ่ม K เพื่อทดลองลดเลือด

    [Header("Events")]
    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;

    private SpriteRenderer sr;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // ระบบ New Input System สำหรับกดทดสอบดาเมจด้วยปุ่ม K
        if (enableDebugKeys && Keyboard.current != null)
        {
            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                TakeDamage(1);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damageAmount;
        onTakeDamage?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        onDeath?.Invoke();

        // หยุดความเร็วของ Rigidbody2D ใน Unity 6
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // ปิดการควบคุมตัวละคร
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Debug.Log("Player Died!");
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float flashDelay = iFramesDuration / 10f;

        for (int i = 0; i < 5; i++)
        {
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(flashDelay);
            if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(flashDelay);
        }

        if (sr != null) sr.color = Color.white;
        isInvincible = false;
    }

    public void SetInvincible(bool state)
    {
        if (!isDead) isInvincible = state;
    }
}