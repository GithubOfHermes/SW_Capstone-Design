using UnityEngine;

public class Skill_1 : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float destroyTime = 3f;

    private void Start()
    {
        Destroy(gameObject, destroyTime); // 자동 파괴만 담당
    }

    private void Update()
    {
        // 방향 이동만 담당 (localScale.x에 따라)
        float moveDir = transform.localScale.x > 0 ? 1f : -1f;
        transform.Translate(Vector2.right * moveDir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("PlayerDamage"))
            return;
        else{
            Destroy(gameObject);
        }
    }
}
