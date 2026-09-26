using UnityEngine;
using System;

public static class BulletFactory
{

    public static Bullet FireBullet(GameObject bulletPrefab, Transform firePoint, float lifetime, Vector2 velocity, Action<Bullet> onDestroyed)
    {



        GameObject go = UnityEngine.Object.Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = go.GetComponent<Bullet>();
        bullet.setLifetime(lifetime);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = velocity;


        bullet.OnBulletDestroyed += onDestroyed;

        return bullet;



    }
}