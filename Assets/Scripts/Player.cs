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
    [Header("Audio")]
    [SerializeField] private AudioClip shotSound;
    [SerializeField, Range(0f, 1f)] private float shotVolume = 1f;
    [SerializeField] private AudioClip hitSound;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 1f;
    [Header("Lives")]
    [SerializeField, Min(1)] private int startingLives = 3;
    [SerializeField, Min(0f)] private float respawnInvulnerability = 1f;

    private float lastFireTime;
    private Controls controls;
    private Vector2 moveInput;
    private Vector2 pointerScreenPosition;
    private bool mouseMovementActive;
    private bool touchMovementActive;
    private Rigidbody2D rb;
    private Camera gameCamera;
    private BoxCollider2D shooterCollider;
    private bool bulletActive = false;
    private Bullet myActiveBullet;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector2 startingPosition;
    [SerializeField, Min(0)] private int livesRemaining;
    private float invulnerabilityTimer;
    private bool gameOver;

    public event Action<int> OnLivesChanged;
    public int LivesRemaining => livesRemaining;

    private Action<InputAction.CallbackContext> _onAttackPerformed;
    private Action<InputAction.CallbackContext> _onMovePerformed;
    private Action<InputAction.CallbackContext> _onMoveCanceled;
    private Action<InputAction.CallbackContext> _onPointPerformed;

    private void Awake()
    {
        InitializeControls();
        rb = GetComponent<Rigidbody2D>();
        gameCamera = Camera.main;
        shooterCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
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
        _onMovePerformed = OnMovePerformed;
        _onMoveCanceled = ctx => moveInput = Vector2.zero;
        _onPointPerformed = OnPointPerformed;
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
        controls.UI.Point.performed += _onPointPerformed;
    }

    public void SetGameActive(bool active)
    {
        if (controls == null || gameOver)
            return;

        if (active)
        {
            controls.Player.Enable();
            controls.UI.Point.Enable();
        }
        else
        {
            moveInput = Vector2.zero;
            mouseMovementActive = false;
            touchMovementActive = false;
            rb.linearVelocity = Vector2.zero;
            controls.Player.Disable();
            controls.UI.Point.Disable();
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
        controls.UI.Point.performed -= _onPointPerformed;

        controls.Player.Disable();
        controls.UI.Point.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        if (moveInput.x != 0f)
            mouseMovementActive = false;
    }

    private void OnPointPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Touchscreen || (Application.isMobilePlatform && Touchscreen.current != null))
            return;

        Vector2 position = ctx.ReadValue<Vector2>();
        if (position.x < 0f || position.x > Screen.width || position.y < 0f || position.y > Screen.height)
            return;

        pointerScreenPosition = position;
        mouseMovementActive = true;
    }

    private void FixedUpdate()
    {
        if (gameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if ((mouseMovementActive || touchMovementActive) && gameCamera != null)
        {
            rb.linearVelocity = Vector2.zero;

            float depth = transform.position.z - gameCamera.transform.position.z;
            float targetX = gameCamera.ScreenToWorldPoint(new Vector3(pointerScreenPosition.x, pointerScreenPosition.y, depth)).x;
            float leftEdge = gameCamera.ViewportToWorldPoint(new Vector3(0f, 0f, depth)).x + shooterCollider.bounds.extents.x;
            float rightEdge = gameCamera.ViewportToWorldPoint(new Vector3(1f, 0f, depth)).x - shooterCollider.bounds.extents.x;
            targetX = Mathf.Clamp(targetX, leftEdge, rightEdge);

            float nextX = Mathf.MoveTowards(rb.position.x, targetX, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(new Vector2(nextX, rb.position.y));
            return;
        }

        // Keyboard movement stays on the horizontal axis.
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
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound, hitVolume);
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
        if (shotSound != null)
            audioSource.PlayOneShot(shotSound, shotVolume);

    }

    public void SetTouchTarget(Vector2 screenPosition)
    {
        if (gameOver || !controls.Player.enabled)
            return;

        pointerScreenPosition = screenPosition;
        touchMovementActive = true;
    }

    public void ClearTouchTarget()
    {
        touchMovementActive = false;
    }

    public void FireFromTouch()
    {
        if (gameOver || !controls.Player.enabled || bulletActive)
            return;

        Fire();
    }

    private void HandleBulletDestroyed(Bullet destroyed)
    {
        if (myActiveBullet != null)
            myActiveBullet.OnBulletDestroyed -= HandleBulletDestroyed;

        bulletActive = false;
        myActiveBullet = null;
    }
}
