using UnityEngine;

public class FlyingMonsterAttackTrigger : MonoBehaviour
{
    [SerializeField] private GameObject attackTrigger;
    [SerializeField] private Vector2 offsetRight = new Vector2(0.24f, -0.04f);
    [SerializeField] private Vector2 offsetLeft = new Vector2(-0.24f, -0.04f);
    [SerializeField] private float activeTime = 0.5f;

    private Coroutine attackCoroutine;

    /// <summary>
    /// 몬스터의 방향에 맞춰 공격 트리거 위치 조정 및 실행
    /// </summary>
    public void TriggerAttack(bool isFacingRight)
    {
        if (attackTrigger == null) return;

        Vector2 offset = isFacingRight ? offsetRight : offsetLeft;
        attackTrigger.transform.localPosition = offset;
        attackTrigger.transform.localScale = Vector3.one;

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        attackTrigger.SetActive(true);
        yield return new WaitForSeconds(activeTime);
        attackTrigger.SetActive(false);
    }

    private void OnDisable()
    {
        if (attackTrigger != null)
            attackTrigger.SetActive(false);
    }
}
