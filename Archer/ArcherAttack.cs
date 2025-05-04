using UnityEngine;

public class ArcherAttack : MonoBehaviour
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float attackCooldown = 3f;

    private bool canAttack = true;

    public void ShootArrow(bool isFacingRight)
    {
        if (!canAttack || arrowPrefab == null || firePoint == null)
            return;

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.Init(isFacingRight);
        }

        canAttack = false;
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack()
    {
        canAttack = true;
    }
}
