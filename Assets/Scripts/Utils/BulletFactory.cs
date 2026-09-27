using UnityEngine;
using System;

public static class BulletFactory
{
    public static Bullet FireBullet(GameObject bulletPrefab, Collider2D shooterCollider, float lifetime, Vector2 velocity, Action<Bullet> onDestroyed)
    {
        // Validate inputs before creating the bullet.
        if (bulletPrefab == null)
            throw new ArgumentNullException(nameof(bulletPrefab));
        if (shooterCollider == null)
            throw new ArgumentNullException(nameof(shooterCollider));

        Vector2 direction = velocity.normalized;
        if (direction == Vector2.zero)
            throw new ArgumentException("Bullet velocity must have a direction.", nameof(velocity));

        if (!shooterCollider.enabled)
            throw new InvalidOperationException("The firing object's collider must be enabled before firing.");

        // Start at the shooter's center, then calculate a spawn point just beyond both colliders.
        Bounds shooterBounds = shooterCollider.bounds;
        GameObject go = UnityEngine.Object.Instantiate(bulletPrefab, shooterBounds.center, Quaternion.identity);
        Bullet bullet = go.GetComponent<Bullet>();
        bullet.setLifetime(lifetime);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = velocity;

        Collider2D bulletCollider = go.GetComponent<Collider2D>();

        // Find how far along the shot direction the ray exits the shooter's bounds.
        float edgeDistance = Mathf.Min(
            Mathf.Abs(direction.x) > Mathf.Epsilon ? shooterBounds.extents.x / Mathf.Abs(direction.x) : float.PositiveInfinity,
            Mathf.Abs(direction.y) > Mathf.Epsilon ? shooterBounds.extents.y / Mathf.Abs(direction.y) : float.PositiveInfinity);
        // Include the bullet's own size so it starts clear of the shooter.
        float bulletExtentAlongDirection = 0f;
        if (bulletCollider != null)
        {
            Vector2 bulletExtents = bulletCollider.bounds.extents;
            bulletExtentAlongDirection = Mathf.Abs(direction.x) * bulletExtents.x + Mathf.Abs(direction.y) * bulletExtents.y;
        }

        Vector2 shooterCenter = new Vector2(shooterBounds.center.x, shooterBounds.center.y);
        Vector2 spawnPosition = shooterCenter + direction * (edgeDistance + bulletExtentAlongDirection + 0.01f);
        go.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, shooterBounds.center.z);


        // Notify the caller when this bullet is destroyed.
        bullet.OnBulletDestroyed += onDestroyed;

        return bullet;
    }
}
