using UnityEngine;

public class Skill_1 : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float destroyTime = 3f;
    [SerializeField] private int skillDamage = 20;

    private void Start()
    {
        Destroy(gameObject, destroyTime); // 자동 파괴
    }

    private void Update()
    {
        float moveDir = transform.localScale.x > 0 ? 1f : -1f;
        transform.Translate(Vector2.right * moveDir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy에게만 데미지를 주되, 충돌한 모든 경우에 파괴는 해야 함
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(skillDamage);
            }
        }

        // 단, Player 자신은 통과
        if (!collision.CompareTag("Player") && !collision.CompareTag("PlayerDamage"))
        {
            Destroy(gameObject);
        }
    }
}
