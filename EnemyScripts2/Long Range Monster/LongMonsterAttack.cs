using UnityEngine;

public class LongMonsterAttack : MonoBehaviour
{
    [Header("Arrow Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    private bool canAttack = true;

    public void ShootArrow(Transform player, bool isFacingRight)
    {
        if (!canAttack || projectilePrefab == null || firePoint == null || player == null)
            return;

        // 방향 벡터 계산
        Vector2 directionToPlayer = (player.position - firePoint.position).normalized;

        // 시야각 체크 (몬스터가 바라보는 방향과 플레이어 방향이 같은지)
        float dot = Vector2.Dot(directionToPlayer, isFacingRight ? Vector2.right : Vector2.left);
        if (dot < 0.7f) return; // 0.7 = 대략 45도 이상 벗어나면 무시

        // 발사
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Init(directionToPlayer);
        }

        canAttack = false;
        ResetAttack();
    }

    private void ResetAttack()
    {
        canAttack = true;
    }
}
