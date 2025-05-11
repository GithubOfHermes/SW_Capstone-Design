using UnityEngine;

public class Skill_1 : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float destroyTime = 3f;
    [SerializeField] private int skillDamage = 20;

    private bool piercing = false;
    private bool isSkill2Active = false;

    public void SetPiercing(bool value)
    {
        piercing = value;
    }

    public void SetSkill2Active(bool value)
    {
        isSkill2Active = value;
        if (isSkill2Active)
        {
            skillDamage = (int)(skillDamage * 1.5f);
        }
        else
        {
            skillDamage = 20; // 원래 데미지로 초기화화
        }
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
            LongRangeMonsterController longEnemy = collision.GetComponent<LongRangeMonsterController>();
            CloseRangeMonsterController closeEnemy = collision.GetComponent<CloseRangeMonsterController>();
            EliteMonsterController eliteEnemy = collision.GetComponent<EliteMonsterController>();
            if (longEnemy != null)
            {
                longEnemy.TakeDamage(skillDamage);
            }
            if (closeEnemy != null)
            {
                closeEnemy.TakeDamage(skillDamage);
            }
            if (eliteEnemy != null)
            {
                eliteEnemy.TakeDamage(skillDamage);
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
