using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlyingCloseRangeMonsterController : MonoBehaviour
{
    public enum State { Wander, Idle, Chase, Attack, Hit, Dead }
    public State currentState = State.Wander;

    public float wanderRadius = 3f;
    public float wanderDelay = 1.5f;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public float chaseRange = 6f;
    public float attackRange = 1.5f;
    public float maxHealth = 30f;
    public float damageCooldown = 0.3f;

    public LayerMask obstacleLayer;
    public FlyingMonsterAttackTrigger attackHandler;
    public Image healthFillImage;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 wanderTarget;
    private Vector2 currentVelocity;
    private float currentHealth;
    private bool isAttacking = false;
    private bool isInvincible = false;
    private bool isDead = false;
    private bool isFacingRight = true;

    [Header("Drop Item")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;
    public float coinDropForce = 3f;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sound")]
    public AudioClip attackSound;
    public AudioClip dieSound;

    private AudioSource audioSource;
    public System.Action<MonoBehaviour> OnMonsterDie;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
        rb.gravityScale = 0;
        currentHealth = maxHealth;

        IgnorePlatformCollisions();
        PickRandomTarget();
        InvokeRepeating(nameof(PickRandomTarget), wanderDelay, wanderDelay);

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 1f;
            healthFillImage.transform.parent.gameObject.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (currentState == State.Dead || isDead) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Wander:
                MoveTo(wanderTarget, moveSpeed);
                animator.Play("Idle");
                if (distToPlayer <= chaseRange)
                    currentState = State.Chase;
                break;

            case State.Chase:
                MoveTo(player.position, chaseSpeed);
                animator.Play("Idle");
                if (distToPlayer <= attackRange)
                    currentState = State.Attack;
                break;

            case State.Attack:
                rb.linearVelocity = Vector2.zero;
                float dirX = player.position.x - transform.position.x;
                FlipByDirection(new Vector2(dirX, 0));

                if (!isAttacking)
                {
                    StartCoroutine(AttackRoutine());
                }
                break;

            case State.Hit:
                rb.linearVelocity = Vector2.zero;
                break;

            case State.Idle:
                animator.Play("Idle");
                break;
        }
    }

    void IgnorePlatformCollisions()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        foreach (GameObject platform in platforms)
        {
            Collider2D platformCol = platform.GetComponent<Collider2D>();
            if (platformCol != null)
            {
                Physics2D.IgnoreCollision(myCollider, platformCol);
            }
        }
    }

    void MoveTo(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        Vector2 targetVelocity = dir * speed;
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, 0.2f);

        FlipByDirection(dir);
        CheckWallCollision(dir);
    }

    void PickRandomTarget()
    {
        if (currentState != State.Wander) return;
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = (Vector2)transform.position + offset;
    }

    void CheckWallCollision(Vector2 dir)
    {
        float checkDistance = 0.3f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, checkDistance, obstacleLayer);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Trap"))
            {
                PickRandomTarget();
            }
        }
    }

    void FlipByDirection(Vector2 dir)
    {
        if (dir.x > 0.01f)
        {
            spriteRenderer.flipX = false;
            isFacingRight = true;
        }
        else if (dir.x < -0.01f)
        {
            spriteRenderer.flipX = true;
            isFacingRight = false;
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.Play("Attack2");

        if (attackSound != null && audioSource != null)
            audioSource.PlayOneShot(attackSound, 0.2f);

        yield return new WaitForSeconds(0.5f); // 공격 타이밍
        attackHandler?.TriggerAttack(isFacingRight);

        yield return new WaitForSeconds(0.2f); // 후딜
        animator.Play("Idle");

        yield return new WaitForSeconds(0.2f); // 다음 상태 결정 전 짧은 대기
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange + 0.2f)
            currentState = State.Attack;
        else if (distToPlayer <= chaseRange)
            currentState = State.Chase;
        else
            currentState = State.Wander;

        isAttacking = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        isInvincible = true;

        if (healthFillImage != null)
        {
            healthFillImage.transform.parent.gameObject.SetActive(true);
            healthFillImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitReaction());
        }

        Invoke(nameof(ResetInvincibility), damageCooldown);
    }

    IEnumerator HitReaction()
    {
        currentState = State.Hit;
        yield return new WaitForSeconds(0.5f);
        currentState = State.Wander;
    }

    void ResetInvincibility()
    {
        isInvincible = false;
    }

    void Die()
    {
        currentState = State.Dead;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        isAttacking = false;

        PlaySoundDetached(dieSound, 0.2f);

        animator.Play("Die");
        DropItem();
        OnMonsterDie?.Invoke(this);
        Destroy(gameObject, 0.4f);
    }

    void DropItem()
    {
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < 30f)
        {
            Debug.Log("아이템 드랍 없음");
            return;
        }
        else if (randomValue < 90f)
        {
            Drop(coinPrefab);
        }
        else
        {
            Drop(heartPrefab);
        }
    }

    void Drop(GameObject itemPrefab)
    {
        if (itemPrefab == null) return;

        GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
        if (itemRb != null)
        {
            Vector2 force = new Vector2(Random.Range(-1f, 1f), 1f).normalized * coinDropForce;
            itemRb.AddForce(force, ForceMode2D.Impulse);
        }

        FindObjectOfType<MonsterCollisionManager>()?.ApplyIgnoreCollisionWithEnemies(item);
    }

    void PlaySoundDetached(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        GameObject soundObj = new GameObject("TempSound");
        AudioSource tempAudio = soundObj.AddComponent<AudioSource>();
        tempAudio.clip = clip;
        tempAudio.volume = volume;
        tempAudio.Play();

        Destroy(soundObj, clip.length);
    }
}
