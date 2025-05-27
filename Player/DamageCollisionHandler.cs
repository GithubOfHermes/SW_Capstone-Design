using UnityEngine;
using System;

public class DamageCollisionHandler : MonoBehaviour
{
    private Action<Collider2D> onTriggerEnterCallback;

    public void Initialize(Action<Collider2D> callback)
    {
        onTriggerEnterCallback = callback;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        onTriggerEnterCallback?.Invoke(other);
    }

    private void OnEnable()
    {
        // 활성화 직후, 이미 겹쳐있는 콜라이더들을 수동으로 검사
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = true
        };

        Collider2D[] results = new Collider2D[16];
        int count = col.Overlap(filter, results);
        for (int i = 0; i < count; i++)
        {
            onTriggerEnterCallback?.Invoke(results[i]);
        }
    }
}
