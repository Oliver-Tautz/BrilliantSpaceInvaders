using UnityEngine;
using System;
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; // seconds before auto-destroy
    [SerializeField] private string[] ignoreTags = new string[] { "Player" }; // Tag of objects that cannot be hit



    public event Action<Bullet> OnBulletDestroyed;
    public void setLifetime(float time)
    {
        lifetime = time;
    }

    public float getLifetime()
    {
        return lifetime;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Example: destroy on hitting something
        if (other.CompareTag("Invader") || other.CompareTag("Boundary") || other.CompareTag("Bunker"))
        {

            Destroy(gameObject);

        }
    }

    private void OnDestroy()
    {

        OnBulletDestroyed?.Invoke(this);
    }
}
