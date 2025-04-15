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
}