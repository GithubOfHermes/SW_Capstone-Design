using UnityEngine;

public class Skill_1 : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float destroyTime = 1.5f;
    [SerializeField] private int baseDamage = 20;
    [SerializeField] private int skillDamage;

    private bool piercing = false;
    private bool isSkill2Active = false;
    private Vector2 moveDirection = Vector2.right; // 기본 방향

    public void SetPiercing(bool value)
    {
        piercing = value;
    }

    public void SetSkill2Active(bool value)
    {
        isSkill2Active = value;

        if (isSkill2Active)
        {
            skillDamage = (int)((baseDamage + PlayerUpgradeData.bonusSkill1Damage) * 1.5f);
        }
        else
        {
            skillDamage = baseDamage + PlayerUpgradeData.bonusSkill1Damage; // 강화된 데미지 기준으로 복원
        }
    }

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;

        // 방향에 따라 오브젝트 회전 적용
        if (moveDirection == Vector2.up)
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        else if (moveDirection == Vector2.down)
            transform.rotation = Quaternion.Euler(0f, 0f, -90f);
        else if (moveDirection == Vector2.left)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        else
            transform.rotation = Quaternion.identity;
    }

    private void Start()
    {
        skillDamage = baseDamage + PlayerUpgradeData.bonusSkill1Damage;
        if (isSkill2Active)
            skillDamage = (int)(skillDamage * 1.5f);

        // Coin, Heart와의 충돌 무시 설정
        IgnoreSpecificTagCollisions("Coin");
        IgnoreSpecificTagCollisions("Heart");

        Destroy(gameObject, destroyTime);
    }

    private void IgnoreSpecificTagCollisions(string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        Collider2D myCol = GetComponent<Collider2D>();

        foreach (GameObject obj in objs)
        {
            foreach (Collider2D col in obj.GetComponentsInChildren<Collider2D>())
            {
                if (myCol != null && col != null)
                {
                    Physics2D.IgnoreCollision(myCol, col, true);
                }
            }
        }
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            LongRangeMonsterController longEnemy = collision.GetComponent<LongRangeMonsterController>();
            CloseRangeMonsterController closeEnemy = collision.GetComponent<CloseRangeMonsterController>();
            EliteMonsterController eliteEnemy = collision.GetComponent<EliteMonsterController>();
            if (longEnemy != null) longEnemy.TakeDamage(skillDamage);
            if (closeEnemy != null) closeEnemy.TakeDamage(skillDamage);
            if (eliteEnemy != null) eliteEnemy.TakeDamage(skillDamage);
            // 관통 중이면 여기서 끝냄 (충돌해도 Destroy X)
            if (piercing) return;
        }

        // 관통 모드가 아니고, 다른 오브젝트와 충돌했을 때만 파괴
        if (!piercing && !collision.CompareTag("Player") && !collision.CompareTag("PlayerDamage"))
        {
            Destroy(gameObject);
        }
    }
}
