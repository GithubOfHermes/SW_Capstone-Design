using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float damageAmount = 10f;

    private void OnEnable()
    {
        // 활성화 직후, 이미 플레이어와 겹쳐 있는지 검사
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = true
        };
        Collider2D[] results = new Collider2D[8];
        int count = col.Overlap(filter, results);
        for (int i = 0; i < count; i++)
        {
            if (results[i].CompareTag("Player"))
                ApplyDamage(results[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ApplyDamage(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null && !player.IsInvincible())
            {
                player.TakeDamage(damageAmount);
            }
        }
    }

    private void ApplyDamage(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
            player.TakeDamage(damageAmount);
    }

    public void SetDamage(float amount)
    {
        damageAmount = amount;
    }
}
