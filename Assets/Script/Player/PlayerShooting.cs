using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Aiming")]
    public Transform firePoint;
    public Transform weaponPivot;
    public Camera mainCamera;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 15f;
    public float fireRate = 0.25f;

    // --- ไม่จำเป็นต้องใช้ Flip Body With Aim จากสคริปต์นี้แล้ว เพราะเราจะพลิก Scale ทางสคริปต์เดินแทน ---
    // public SpriteRenderer bodySprite;    
    // public bool flipBodyWithAim = true;

    private float fireCooldown;
    private Vector2 aimDirection = Vector2.right;

    // --- เพิ่มการอ้างอิงไปที่สคริปต์ PlayerMovement ---
    private PlayerMovement playerMovement;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // ค้นหาสคริปต์เดินในตัวละคร
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Mouse.current == null) return;

        UpdateAimDirection();
        RotateWeaponPivot();

        // นับเวลา cooldown การยิง
        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        }

        // กดครั้งเดียวยิงครั้งเดียว
        if (Mouse.current.leftButton.wasPressedThisFrame && fireCooldown <= 0f)
        {
            Shoot();

            // --- บังคับหันหน้าไปทางเมาส์เมื่อกดยิง ---
            if (playerMovement != null)
            {
                playerMovement.ForceFaceDirection(aimDirection.x);
            }
        }
    }

    private void UpdateAimDirection()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCamera.transform.position.z)
        );

        Vector2 originPos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        aimDirection = ((Vector2)mouseWorldPos - originPos).normalized;
    }

    private void RotateWeaponPivot()
    {
        if (weaponPivot == null) return;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // --- แก้บั๊กเรื่องปืนตีลังกาเวลาหันตัว ---
        // ถ้าระบบหลักพลิก Scale ของตัวละคร แกนหมุนจะต้องปรับทิศทางเล็กน้อยไม่ให้หัวปืนกลับด้าน
        if (transform.localScale.x < 0)
        {
            // ถ้าตัวละครหันซ้าย แกนหมุนจะต้องถูก Flip กลับ (เพราะมันถูกดึง Scale ติดลบมา)
            weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle + 180f);
        }
        else
        {
            weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        fireCooldown = fireRate;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        bulletObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(aimDirection, bulletSpeed);
        }
    }
}