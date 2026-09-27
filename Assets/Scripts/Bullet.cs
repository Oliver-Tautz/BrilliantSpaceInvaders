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
        // Keep traveling when this collider has an ignored tag.
        for (int i = 0; i < ignoreTags.Length; i++)
        {
            if (other.gameObject.tag == ignoreTags[i])
                return;
        }

        // Any other collision consumes the bullet.
        Destroy(gameObject);
    }

    private void OnDestroy()
    {

        OnBulletDestroyed?.Invoke(this);
    }
}
