using UnityEngine;
using System.Collections;

public static class CollisionTemporizer
{
    public static void IgnoreTemporarily(Collider2D a, Collider2D b, float seconds)
    {
        if (a == null || b == null || CoroutineRunner.Instance == null)
        {
            Debug.LogWarning("[CollisionTemporizer] 충돌 무시 실패: 유효하지 않은 인자 또는 CoroutineRunner 누락");
            return;
        }

        // 기존처럼 GameObject 만들지 않고 전역 CoroutineRunner에 위임
        CoroutineRunner.Instance.StartCoroutine(ReenableAfter(a, b, seconds));
    }

    private static IEnumerator ReenableAfter(Collider2D a, Collider2D b, float duration)
    {
        Physics2D.IgnoreCollision(a, b, true);
        yield return new WaitForSeconds(duration);

        if (a != null && b != null)
            Physics2D.IgnoreCollision(a, b, false);
    }
}
