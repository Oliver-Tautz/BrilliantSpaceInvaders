using UnityEngine;
using System;

using BRILLIANTSPACEINVADERS.Utils;
using System.Collections.Generic;
public class Invader : MonoBehaviour
{


    private bool isDead = false;

    private float shot_timer;
    public event Action<Invader> OnInvaderKilled;
    private int CoordinateX;
    private int CoordinateY;

    private List<int> killedSameCol = new List<int>(); // Y Coordinates of invaders killed in the same row. 
    private int numberOfInvadersPerCol; // Total number of invaders per column at the start of the game.
    private int allowedFire; // Is this invader allowed to fire?
    private int allowedSum;
    private bool _initialized;

    // ===== Tunables (editable in Inspector) =====
    [Header("Config")]
    [Tooltip("Hit points for this invader.")]
    [SerializeField] private int health = 1;

    public int GetCoordinateY => CoordinateY;

    public int GetCoordinateX => CoordinateX;

    public int Health => health;


    /// <summary>
    /// Call this right after Instantiate to configure the invader.
    /// </summary>
    public void Initialize(int col, int row, int invadersPerColumn)
    {
        CoordinateX = col;
        CoordinateY = row;
        numberOfInvadersPerCol = invadersPerColumn;

        // Any precomputed values that must exist before Start():
        allowedSum = MathUtils.SumRange(CoordinateY, numberOfInvadersPerCol - 1);

        _initialized = true;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Use layers/tags to keep this tight
        if (other.CompareTag("PlayerBullet"))
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

}
