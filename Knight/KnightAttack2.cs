using UnityEngine;

public class KnightAttack2 : MonoBehaviour
{
    public GameObject attackTrigger; // 공격 판정 오브젝트
    public Vector2 offsetRight = new Vector2(0.2f, 0f);
    public Vector2 offsetLeft = new Vector2(-0.2f, 0f);

    public void DoAttack(bool isFacingRight)
    {
        if (attackTrigger != null)
        {
            // 방향에 따라 localPosition 설정
            attackTrigger.transform.localPosition = isFacingRight ? offsetRight : offsetLeft;

            // 활성화 후 일정 시간 뒤 비활성화
            attackTrigger.SetActive(true);
            Invoke(nameof(DisableAttack), 0.1f);
        }
    }

    void DisableAttack()
    {
        if (attackTrigger != null)
        {
            attackTrigger.SetActive(false);
        }
    }
}
