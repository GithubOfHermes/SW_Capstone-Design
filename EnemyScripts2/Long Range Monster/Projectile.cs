using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 3f;
    public float lifeTime = 3f;
    public int damage = 10;

     public AudioClip hitSound;
    private AudioSource audioSource;

    private Vector2 direction = Vector2.right;

    public void Init(Vector2 fireDirection)
    {
        direction = fireDirection.normalized;

        // Sprite 방향 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifeTime);
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
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
            PlaySoundOnly(hitSound, 0.2f);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    void PlaySoundOnly(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        GameObject soundObj = new GameObject("FireballSound");
        AudioSource tempAudio = soundObj.AddComponent<AudioSource>();
        tempAudio.clip = clip;
        tempAudio.volume = volume;
        tempAudio.Play();

        Destroy(soundObj, clip.length);  // 사운드 재생 시간 후 삭제
    }
}
