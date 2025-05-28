using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;
    public float damage = 10f;

    public AudioClip hitSound;
    private AudioSource audioSource;

    private Vector2 direction;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifeTime);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        sr.flipX = (dir.x < 0);
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

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            PlaySoundOnly(hitSound, 0.4f);

            Debug.Log("플레이어에게 파이어볼 데미지!");
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
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