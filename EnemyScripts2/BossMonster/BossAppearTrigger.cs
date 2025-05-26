using UnityEngine;

public class BossAppearTrigger : MonoBehaviour
{
    public GameObject bossObject; 

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (!collision.CompareTag("Player")) return;

        triggered = true;

        if (bossObject != null)
        {
            bossObject.SetActive(true);
            BossController boss = bossObject.GetComponent<BossController>();
            if (boss != null)
            {
                boss.StartAppearance();
            }
        }

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.TriggerBossBGM();
        }

        Destroy(gameObject);
    }
}
