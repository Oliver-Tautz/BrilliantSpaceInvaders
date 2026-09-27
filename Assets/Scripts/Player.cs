using UnityEngine;
using UnityEngine.InputSystem;
using System;
using BRILLIANTSPACEINVADERS.Utils;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifetime = 5f;
    [SerializeField] private float moveSpeed = 6f;
    [Header("Lives")]
    [SerializeField, Min(1)] private int startingLives = 3;
    [SerializeField, Min(0f)] private float respawnInvulnerability = 1f;

    private float lastFireTime;
    private Controls controls;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private BoxCollider2D shooterCollider;
    private bool bulletActive = false;
    private Bullet myActiveBullet;
    private SpriteRenderer spriteRenderer;
    private Vector2 startingPosition;
    [SerializeField, Min(0)] private int livesRemaining;
    private float invulnerabilityTimer;
    private bool gameOver;

    public event Action<int> OnLivesChanged;
    public int LivesRemaining => livesRemaining;

    private Action<InputAction.CallbackContext> _onAttackPerformed;
    private Action<InputAction.CallbackContext> _onMovePerformed;
    private Action<InputAction.CallbackContext> _onMoveCanceled; private void Awake()


    {
        InitializeControls();
        rb = GetComponent<Rigidbody2D>();
        shooterCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startingPosition = rb.position;
        livesRemaining = startingLives;
        shooterCollider.enabled = true;
        FitColliderToSprite();

    }

    private void InitializeControls()
    {
        if (controls != null)
            return;

        controls = new Controls();
        _onAttackPerformed = OnAttack;
        _onMovePerformed = ctx => moveInput = ctx.ReadValue<Vector2>();
        _onMoveCanceled = ctx => moveInput = Vector2.zero;
    }


    private void FitColliderToSprite()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        shooterCollider.size = spriteRenderer.sprite.bounds.size;
        shooterCollider.offset = spriteRenderer.sprite.bounds.center;
    }

    private void OnEnable()
    {
        InitializeControls();

        // The game loop enables player input only while the game is running.
        controls.Player.Attack.performed += _onAttackPerformed;
        controls.Player.Move.performed += _onMovePerformed;
        controls.Player.Move.canceled += _onMoveCanceled;
    }

    public void SetGameActive(bool active)
    {
        if (controls == null || gameOver)
            return;

        if (active)
        {
            controls.Player.Enable();
        }
        else
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            controls.Player.Disable();
        }
    }

    private void OnDisable()
    {
        if (controls == null)
            return;

        // Unsubscribe when disabled (important!)
        controls.Player.Attack.performed -= _onAttackPerformed;
        controls.Player.Move.performed -= _onMovePerformed;
        controls.Player.Move.canceled -= _onMoveCanceled;

        controls.Player.Disable();
    }

    private void FixedUpdate()
    {
        if (gameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Only move left/right, ignore y
        Vector2 velocity = new Vector2(moveInput.x * moveSpeed, 0f);
        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
            invulnerabilityTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet_Invader"))
            LoseLife();
    }

    private void LoseLife()
    {
        if (gameOver || invulnerabilityTimer > 0f)
            return;

        livesRemaining--;
        Debug.Log($"Player hit. Lives remaining: {livesRemaining}");

        if (livesRemaining <= 0)
        {
            gameOver = true;
            moveInput = Vector2.zero;
            shooterCollider.enabled = false;
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
            enabled = false;
            OnLivesChanged?.Invoke(livesRemaining);
            Debug.Log("Game Over");
            return;
        }

        // Finish the respawn before notifying the game loop to pause time.
        transform.position = new Vector3(startingPosition.x, startingPosition.y, transform.position.z);
        rb.position = startingPosition;
        rb.linearVelocity = Vector2.zero;
        invulnerabilityTimer = respawnInvulnerability;
        Physics2D.SyncTransforms();
        OnLivesChanged?.Invoke(livesRemaining);
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (gameOver) return;
        Debug.Log("Attack action triggered! Phase: " + ctx.phase);
        if (!ctx.performed) return; // only on press
        if (bulletActive) return; // only one bullet at a time

        Fire();
    }



    private void Fire()
    {

        myActiveBullet = BulletFactory.FireBullet(bulletPrefab, shooterCollider, bulletLifetime, Vector2.up * bulletSpeed, HandleBulletDestroyed);
        bulletActive = true;

    }

    private void HandleBulletDestroyed(Bullet destroyed)
    {
        if (myActiveBullet != null)
            myActiveBullet.OnBulletDestroyed -= HandleBulletDestroyed;

        bulletActive = false;
        myActiveBullet = null;
    }
}
