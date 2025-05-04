using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 3f;
    public float lifeTime = 3f;
    public int damage = 10;

    private Vector2 direction = Vector2.right;

    public void Init(bool isFacingRight)
    {
        direction = isFacingRight ? Vector2.right : Vector2.left;

        // Sprite 좌우 반전
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1 : -1);
        transform.localScale = scale;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
