using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Aiming")]
    public Transform firePoint;          // จุดที่กระสุนจะเกิด (ลูกของ Player วางไว้ปลายปืน/มือ)
    public Transform weaponPivot;        // (ทางเลือก) ตัวหมุนตามเมาส์ เช่น sprite ปืน ถ้าไม่มีปล่อยว่างได้
    public Camera mainCamera;            // กล้องหลัก ถ้าไม่ใส่จะใช้ Camera.main อัตโนมัติ

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 15f;
    public float fireRate = 0.25f;       // วินาทีต่อการยิง 1 นัด (ยิ่งน้อยยิ่งยิงถี่)

    [Header("Flip Body With Aim (ทางเลือก)")]
    public SpriteRenderer bodySprite;    // ถ้าอยากให้ตัวละครหันตามทิศเมาส์ด้วย ใส่ตรงนี้
    public bool flipBodyWithAim = true;

    private float fireCooldown;
    private Vector2 aimDirection = Vector2.right;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        UpdateAimDirection();
        RotateWeaponPivot();

        if (flipBodyWithAim && bodySprite != null)
        {
            bodySprite.flipX = aimDirection.x < 0f;
        }

        // นับเวลา cooldown การยิง
        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        }

        // กดครั้งเดียวยิงครั้งเดียว
        if (Mouse.current.leftButton.wasPressedThisFrame && fireCooldown <= 0f)
        {
            Shoot();
        }
    }

    // หาทิศทางจากตัวละครไปยังตำแหน่งเมาส์ในโลกเกม
    private void UpdateAimDirection()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCamera.transform.position.z)
        );

        Vector2 originPos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        aimDirection = ((Vector2)mouseWorldPos - originPos).normalized;
    }

    // หมุน weaponPivot ให้ชี้ไปทางเมาส์ (ถ้ามีปืน/แขนที่ต้องหมุนตาม)
    private void RotateWeaponPivot()
    {
        if (weaponPivot == null) return;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        fireCooldown = fireRate;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // หมุนกระสุนให้หันตามทิศยิง (เผื่อ sprite กระสุนมีหัวท้าย)
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        bulletObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(aimDirection, bulletSpeed);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + (Vector3)aimDirection * 1f);
        }
    }
}