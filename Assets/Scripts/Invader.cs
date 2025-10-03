using UnityEngine;

public class Invader : MonoBehaviour
{


    private bool isDead = false;


    private float shot_timer;
    public static event Action<Invader> OnInvaderKilled;

    // ===== Tunables (editable in Inspector) =====
    [Header("Config")]
    [Tooltip("Hit points for this invader.")]
    [SerializeField] private int health = 1;

    public int Health => health;

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

        // Hide or pool; SetActive(false) is good for pooling
        gameObject.SetActive(false);
    }

}
