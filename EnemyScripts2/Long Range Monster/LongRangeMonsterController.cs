using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LongRangeMonsterController : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Hit, Dead }
    public State currentState = State.Idle;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float chaseRange = 5f;
    public float attackRange = 4f;
    public LayerMask groundLayer;

    public float maxHealth = 30;
    private float currentHealth;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isFacingRight = true;

    public Transform wallCheck;
    public float wallCheckDistance = 2f;
    public float groundCheckDistance = 2f;

    private float flipCooldown = 0f;
    private float lastFlipTime = -1f;

    private LongMonsterAttack longMonsterAttack;
    private bool isInvincible = false;
    public float damageCooldown = 0.3f;

    private bool isAttacking = false;

    public System.Action<MonoBehaviour> OnMonsterDie;

    [Header("Drop Item")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;
    public float coinDropForce = 3f;

    [Header("Health Bar")]
    public Image healthFillImage;

    [Header("Sound")]
    public AudioClip attackSound;
    public AudioClip dieSound;

    private AudioSource audioSource;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        longMonsterAttack = GetComponent<LongMonsterAttack>();
        currentState = State.Patrol;
        currentHealth = maxHealth;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 1f;
            healthFillImage.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("HealthFillImage가 연결되지 않았습니다!");
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
        if (currentState == State.Dead) return;

        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Hit:
                break;
        }

        CheckTransition();
    }

    void Idle()
    {
        rb.linearVelocity = Vector2.zero;
        PlayAnimation("Idle");
    }

    void Patrol()
    {
        PlayAnimation("Walk");

        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        rb.linearVelocity = new Vector2(direction.x * patrolSpeed, rb.linearVelocity.y);

        bool wallHit = Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, groundLayer);
        bool groundAhead = IsGroundAhead();

        if ((wallHit || !groundAhead) && Time.time - lastFlipTime > flipCooldown)
        {
            Flip();
            lastFlipTime = Time.time;
        }
    }

    void Chase()
    {
        float xDist = Mathf.Abs(transform.position.x - player.position.x);
        float yDist = Mathf.Abs(transform.position.y - player.position.y);

        if (xDist <= attackRange && yDist <= 5.0f)
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnimation("Idle");
            return;
        }

        PlayAnimation("Run");

        Vector2 direction = (player.position - transform.position).normalized;

        bool wallHit = Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, groundLayer);
        bool groundAhead = IsGroundAhead();

        if (wallHit || !groundAhead)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = State.Idle;
            Invoke(nameof(SwitchToPatrol), 1.0f);
            return;
        }

        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
        Flip(direction.x);
    }

    bool IsGroundAhead()
    {
        Vector2 origin = wallCheck.position + Vector3.down * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    void SwitchToPatrol()
    {
        if (currentState == State.Idle)
            currentState = State.Patrol;
    }

    void Attack()
    {
        rb.linearVelocity = Vector2.zero;

        float dirX = player.position.x - transform.position.x;
        Flip(dirX);

        bool playerInFront = (dirX > 0 && isFacingRight) || (dirX < 0 && !isFacingRight);

        if (!isAttacking && playerInFront)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        PlayAnimation("Attack");

        if (attackSound != null && audioSource != null)
            audioSource.PlayOneShot(attackSound, 0.2f);

        yield return new WaitForSeconds(0.5f);
        longMonsterAttack?.ShootArrow(player, isFacingRight); // 플레이어를 향해 발사

        yield return new WaitForSeconds(0.3f);
        PlayAnimation("Idle");

        yield return new WaitForSeconds(4f);
        isAttacking = false;
        currentState = State.Idle;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;

        Vector3 wcPos = wallCheck.localPosition;
        wcPos.x *= -1;
        wallCheck.localPosition = wcPos;
    }

    void Flip(float dirX)
    {
        if ((dirX > 0 && !isFacingRight) || (dirX < 0 && isFacingRight))
        {
            Flip();
        }
    }

    void CheckTransition()
    {
        if (currentState == State.Dead || currentState == State.Hit || isAttacking) return;

        float xDist = Mathf.Abs(transform.position.x - player.position.x);
        float yDist = Mathf.Abs(transform.position.y - player.position.y);

        if (xDist <= attackRange && yDist <= 3.0f)
        {
            if (currentState != State.Attack)
            {
                currentState = State.Attack;
                Attack();
            }
        }
        else if (xDist <= chaseRange && yDist <= 3.5f)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == State.Dead || isInvincible) return;

        isInvincible = true;
        currentHealth -= damage;
        Debug.Log($"Archer가 {damage}의 데미지를 입음! 현재 HP: {currentHealth}");

        if (healthFillImage != null)
        {
            if (!healthFillImage.transform.parent.gameObject.activeSelf)
                healthFillImage.transform.parent.gameObject.SetActive(true);

            float fill = Mathf.Clamp01(currentHealth / maxHealth);
            healthFillImage.fillAmount = fill;
        }

        if (currentHealth > 0)
        {
            OnHit();
        }
        else
        {
            OnDie();
        }

        Invoke(nameof(ResetInvincibility), damageCooldown);
    }

    void ResetInvincibility()
    {
        isInvincible = false;
    }

    public void OnHit()
    {
        if (currentState == State.Dead) return;

        currentState = State.Hit;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        isAttacking = false;
        PlayAnimation("Hit");
        Invoke(nameof(RecoverFromHit), 1f);
    }

    void RecoverFromHit()
    {
        if (currentState != State.Dead)
            currentState = State.Idle;
    }

    public void OnDie()
    {
        if (currentState == State.Dead) return;

        currentState = State.Dead;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        isAttacking = false;

        if (dieSound != null && audioSource != null)
            audioSource.PlayOneShot(dieSound, 0.2f);

        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.Play("Die");
        }

        Invoke(nameof(DestroySelf), 2f);
    }

    void DestroySelf()
    {
        DropItem();

        if (healthFillImage != null)
            healthFillImage.transform.parent.gameObject.SetActive(false);

        Destroy(gameObject);
        OnMonsterDie?.Invoke(this);
    }

    void PlayAnimation(string animName)
    {
        if (animator != null && currentState != State.Dead)
        {
            animator.Play(animName);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
        {
            Debug.Log("데미지 입음!!");
            TakeDamage(10);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(wallCheck.position, dir * wallCheckDistance);

            Vector2 downPos = wallCheck.position + Vector3.down * 0.1f;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(downPos, Vector2.down * groundCheckDistance);
        }
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
}
