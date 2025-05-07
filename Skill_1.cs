using UnityEngine;

public class Skill_1 : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float destroyTime = 3f;
    [SerializeField] private int skillDamage = 20;

    private bool piercing = false;

    public void SetPiercing(bool value)
    {
        piercing = value;
    }

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        float moveDir = transform.localScale.x > 0 ? 1f : -1f;
        transform.Translate(Vector2.right * moveDir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(skillDamage);
            }

            // 관통 중이면 여기서 끝냄 (Destroy 안 함)
            if (piercing) return;
        }

        // 관통 모드가 아니고, 적 외의 다른 오브젝트와 충돌했을 때만 파괴
        if (!piercing && !collision.CompareTag("Player") && !collision.CompareTag("PlayerDamage"))
        {
            Destroy(gameObject);
        }
    }
}
