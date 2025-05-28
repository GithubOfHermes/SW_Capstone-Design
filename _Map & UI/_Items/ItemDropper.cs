using UnityEngine;

public static class ItemDropper
{
    public static void Drop(GameObject prefab, Vector3 spawnPoint)
    {
        Vector3 pos = spawnPoint + new Vector3(Random.Range(-0.2f, 0.2f), 0f, -1f);
        GameObject item = Object.Instantiate(prefab, pos, Quaternion.identity);
        item.layer = LayerMask.NameToLayer("Item");

        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float upwardForce = 6f;
            float sideVariance = Random.Range(-0.5f, 0.5f);
            rb.AddForce(new Vector2(sideVariance, 1f) * upwardForce, ForceMode2D.Impulse);
        }

        // 본체 Collider만 추출 (Trigger 제외)
        Collider2D itemCol = null;
        foreach (var col in item.GetComponents<Collider2D>())
        {
            if (!col.isTrigger)
            {
                itemCol = col;
                break;
            }
        }

        // 충돌 영구 무시
        Collider2D playerCol = GameObject.FindWithTag("Player")?.GetComponent<Collider2D>();
        if (playerCol != null && itemCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, itemCol, true);
        }

        item.AddComponent<ItemBounceBehavior>();
    }
}
