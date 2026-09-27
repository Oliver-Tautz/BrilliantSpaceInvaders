using UnityEngine;
using System;

using BRILLIANTSPACEINVADERS.Utils;
using System.Collections.Generic;
public class Invader : MonoBehaviour
{

    private bool isDead = false;


    private float shotInterval;// seconds between possible shots
    private float shotTimer; // timer for shots
    public event Action<Invader> OnInvaderKilled;
    private int coordinateCol;
    private int coordinateRow;
    private Bullet bulletPrefab;


    private bool allowedFire = false; // Is this invader allowed to fire?
    private bool _initialized;

    // ===== Tunables (editable in Inspector) =====
    [Header("Config")]
    [Tooltip("Hit points for this invader.")]
    [SerializeField] private int health = 1;

    [Header("Firing")]
    [Tooltip("Shortest delay between this invader's shots, in seconds.")]
    [SerializeField, Min(0.1f)] private float minShotInterval = 1f;
    [Tooltip("Longest delay between this invader's shots, in seconds.")]
    [SerializeField, Min(0.1f)] private float maxShotInterval = 3f;
    [Tooltip("Downward bullet speed in units per second.")]
    [SerializeField, Min(0f)] private float bulletSpeed = 5f;

    [SerializeField] private Sprite[] frames;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private int frameIndex;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
            boxCollider.enabled = true;
        FitColliderToSprite();
    }

    public void AdvanceFrame()
    {
        if (frames == null || frames.Length == 0) return;

        frameIndex = (frameIndex + 1) % frames.Length;
        spriteRenderer.sprite = frames[frameIndex];
        FitColliderToSprite();
    }

    private void FitColliderToSprite()
    {
        if (boxCollider == null || spriteRenderer.sprite == null) return;

        boxCollider.size = spriteRenderer.sprite.bounds.size;
        boxCollider.offset = spriteRenderer.sprite.bounds.center;
    }

    public int GetCoordinateRow => coordinateRow;

    public int GetCoordinateCol => coordinateCol;

    public int Health => health;





    void Update()
    {
        shotTimer += Time.deltaTime;

        if (shotTimer >= shotInterval)
        {
            shotTimer = 0f; // reset timer

            if (allowedFire)
            {
                // Fire bullet
                FireBullet();
            }
        }



    }
    public void SetAllowedFire(bool allowed)
    {
        allowedFire = allowed;
    }


    /// <summary>
    /// Call this right after Instantiate to configure the invader.
    /// </summary>
    public void Initialize(int col, int row, Bullet bulletPrefab)
    {
        coordinateCol = col;
        coordinateRow = row;

        float shortestInterval = Mathf.Min(minShotInterval, maxShotInterval);
        float longestInterval = Mathf.Max(minShotInterval, maxShotInterval);
        shotInterval = UnityEngine.Random.Range(shortestInterval, longestInterval);
        shotTimer = UnityEngine.Random.Range(0f, shotInterval);

        this.bulletPrefab = bulletPrefab;

        _initialized = true;

    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Use layers/tags to keep this tight
        if (other.CompareTag("Bullet_Player"))
        {
            // If you use pooled bullets, disable instead of Destroy
            Destroy(other.gameObject);
            TakeDamage(1);
        }
    }
    private void TakeDamage(int amount)
    {
        if (isDead) return;

        health -= amount;
        if (health <= 0)
        {
            isDead = true;
            Kill();
        }
    }

    private void Kill()
    {
        // Notify observers
        OnInvaderKilled?.Invoke(this);

        // Destroy this invader
        Destroy(gameObject);
    }
    private void FireBullet()
    {
        // Implement bullet firing logic here
        Debug.Log($"Invader at ({coordinateCol}, {coordinateRow}) fired a bullet!");


        BulletFactory.FireBullet(bulletPrefab.gameObject, boxCollider, bulletPrefab.getLifetime(), Vector2.down * bulletSpeed, null);
    }


}
