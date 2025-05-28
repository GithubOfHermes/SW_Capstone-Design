using UnityEngine;

public class BossAttackTrigger : MonoBehaviour
{
    [SerializeField] private float damageAmount = 15f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ApplyDamage(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        ApplyDamage(collision);
    }

    private void ApplyDamage(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && !player.IsInvincible())
        {
            player.TakeDamage(damageAmount);
        }
    }
    public void SetDamage(float newDamage)
    {
        damageAmount = newDamage;
    }
}
