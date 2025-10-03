using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float fireCooldown = 0.5f;

    private float lastFireTime;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastFireTime + fireCooldown)
        {
            Fire();
        }
    }

    private void Fire()
    {
        lastFireTime = Time.time;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.up * bulletSpeed;
    }
}
