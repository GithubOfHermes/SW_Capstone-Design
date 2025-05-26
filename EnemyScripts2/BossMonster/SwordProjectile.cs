using UnityEngine;

public class SwordProjectile : MonoBehaviour
{
    public float damage = 20f;
    public float lifeTime = 5f;
    private Rigidbody2D rb;

    [Header("히트 관련련")]
    public GameObject hitEffect;
    public AudioClip hitSound;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("[SwordProjectile] Rigidbody2D가 없습니다!");
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (rb.linearVelocity != Vector2.zero)
        {
            transform.right = rb.linearVelocity.normalized;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);

            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f); 
            }
            PlayHitSound();
        }

        if (collision.CompareTag("Wall") || collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f); 
            }
            PlayHitSound();
        }
    }

    private void PlayHitSound()
    {
        if (hitSound == null) return;

        GameObject tempAudio = new GameObject("TempHitSound");
        AudioSource audio = tempAudio.AddComponent<AudioSource>();

        audio.PlayOneShot(hitSound, 0.2f);

        Destroy(tempAudio, hitSound.length);
    }
}
