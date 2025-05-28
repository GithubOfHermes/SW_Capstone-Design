using UnityEngine;

/// <summary>
/// 씬에 배치해서 몬스터, 플레이어, 아이템 충돌 무시 관리 (씬 시작 시 자동 처리 & 수동 호출 가능)
/// </summary>
public class MonsterCollisionManager : MonoBehaviour
{
    [Header("시작 시 자동 적용")]
    public bool applyOnStart = true;
    public bool ignorePlayerOnStart = true;
    public bool ignoreItemOnStart = true;

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyIgnoreCollisionToAllEnemies();
        }

        if (ignorePlayerOnStart)
        {
            ApplyIgnoreCollisionWithPlayer();
        }

        if (ignoreItemOnStart)
        {
            ApplyIgnoreCollisionWithAllEnemiesAndItems();
        }
    }

    /// <summary>
    /// 씬 내 Enemy 태그 몬스터끼리 충돌 무시
    /// </summary>
    public void ApplyIgnoreCollisionToAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            Collider2D colA = enemies[i].GetComponent<Collider2D>();
            if (colA == null) continue;

            for (int j = i + 1; j < enemies.Length; j++)
            {
                Collider2D colB = enemies[j].GetComponent<Collider2D>();
                if (colB == null) continue;

                Physics2D.IgnoreCollision(colA, colB);
            }
        }
    }

    /// <summary>
    /// 스폰된 몬스터과 기존 몬스터, 플레이어, 아이템 충돌 무시
    /// </summary>
    public void ApplyIgnoreCollision(GameObject newMonster)
    {
        Collider2D myCol = newMonster.GetComponent<Collider2D>();
        if (myCol == null) return;

        GameObject[] others = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject other in others)
        {
            if (other == newMonster) continue;

            Collider2D otherCol = other.GetComponent<Collider2D>();
            if (otherCol != null)
            {
                Physics2D.IgnoreCollision(myCol, otherCol);
            }
        }

        ApplyIgnoreCollisionWithPlayer(newMonster);
        ApplyIgnoreCollisionWithAllItems(newMonster);
    }

    /// <summary>
    /// 씬 내 모든 몬스터와 플레이어 충돌 무시
    /// </summary>
    public void ApplyIgnoreCollisionWithPlayer()
    {
        Collider2D playerCol = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Collider2D>();
        if (playerCol == null) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (enemyCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, enemyCol);
            }
        }
    }

    /// <summary>
    /// 특정 몬스터와 플레이어 충돌 무시
    /// </summary>
    public void ApplyIgnoreCollisionWithPlayer(GameObject monster)
    {
        Collider2D monsterCol = monster.GetComponent<Collider2D>();
        Collider2D playerCol = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Collider2D>();

        if (monsterCol != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(monsterCol, playerCol);
        }
    }

    /// <summary>
    /// 씬의 모든 몬스터가 씬 내 'Item' 태그 오브젝트와 충돌 무시 (한 번만 호출해도 계속 유지)
    /// </summary>
    public void ApplyIgnoreCollisionWithAllEnemiesAndItems()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (enemyCol == null) continue;

            foreach (GameObject item in items)
            {
                Collider2D itemCol = item.GetComponent<Collider2D>();
                if (itemCol == null) continue;

                Physics2D.IgnoreCollision(enemyCol, itemCol);
            }
        }
    }

    /// <summary>
    /// 특정 몬스터와 씬에 존재하는 모든 'Item' 태그 오브젝트 충돌 무시
    /// </summary>
    public void ApplyIgnoreCollisionWithAllItems(GameObject monster)
    {
        Collider2D monsterCol = monster.GetComponent<Collider2D>();
        if (monsterCol == null) return;

        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

        foreach (GameObject item in items)
        {
            Collider2D itemCol = item.GetComponent<Collider2D>();
            if (itemCol != null)
            {
                Physics2D.IgnoreCollision(monsterCol, itemCol);
            }
        }
    }

    /// <summary>
    /// 드랍된 아이템과 씬 내 모든 몬스터 충돌 무시 (아이템 생성 직후 반드시 호출)
    /// </summary>
    public void ApplyIgnoreCollisionWithEnemies(GameObject item)
    {
        Collider2D itemCol = item.GetComponent<Collider2D>();
        if (itemCol == null) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (enemyCol != null)
            {
                Physics2D.IgnoreCollision(itemCol, enemyCol);
            }
        }
    }
}
