using UnityEngine;
using UnityEngine.InputSystem;
using System;
using BRILLIANTSPACEINVADERS.Utils;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifetime = 5f;
    [SerializeField] private float moveSpeed = 6f;
    private float lastFireTime;
    private Controls controls;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool bulletActive = false;
    private Bullet myActiveBullet;

    private Action<InputAction.CallbackContext> _onAttackPerformed;
    private Action<InputAction.CallbackContext> _onMovePerformed;
    private Action<InputAction.CallbackContext> _onMoveCanceled; private void Awake()


    {
        controls = new Controls();
        rb = GetComponent<Rigidbody2D>();

        _onAttackPerformed = OnAttack;
        _onMovePerformed = ctx => moveInput = ctx.ReadValue<Vector2>();
        _onMoveCanceled = ctx => moveInput = Vector2.zero;
    }


    private void OnEnable()
    {

        // Enable the whole Player action map
        controls.Player.Enable();

        // Subscribe to the Attack action
        controls.Player.Attack.performed += _onAttackPerformed;
        controls.Player.Move.performed += _onMovePerformed;
        controls.Player.Move.canceled += _onMoveCanceled;
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled (important!)
        controls.Player.Attack.performed -= _onAttackPerformed;
        controls.Player.Move.performed -= _onMovePerformed;
        controls.Player.Move.canceled -= _onMoveCanceled;

        controls.Player.Disable();
    }

    private void FixedUpdate()
    {
        // Only move left/right, ignore y
        Vector2 velocity = new Vector2(moveInput.x * moveSpeed, 0f);
        rb.linearVelocity = velocity;
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        Debug.Log("Attack action triggered! Phase: " + ctx.phase);
        if (!ctx.performed) return; // only on press
        if (bulletActive) return; // only one bullet at a time

        Fire();
    }



    private void Fire()
    {

        myActiveBullet = BulletFactory.FireBullet(bulletPrefab, firePoint, bulletLifetime, Vector2.up * bulletSpeed, HandleBulletDestroyed);
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
