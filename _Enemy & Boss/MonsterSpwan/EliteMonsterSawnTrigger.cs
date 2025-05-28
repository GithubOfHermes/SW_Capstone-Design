using UnityEngine;

public class EliteMonsterSawnTrigger : MonoBehaviour
{
    public EliteMonsterSpwan spawner;


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
             Debug.Log("플레이어가 트리거에 닿음");
            spawner.SpawnEliteAndMonsters();
        }
        Destroy(gameObject);
    }
}
