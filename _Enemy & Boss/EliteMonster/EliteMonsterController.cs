using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // ✅ SceneManagement 네임스페이스 추가

public class EliteMonsterController : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Hit, Dead }
    public State currentState = State.Idle;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float chaseRange = 5f;
    public float attackRange = 1.5f;
    public LayerMask groundLayer;

    public float maxHealth = 30;
    private float currentHealth;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isFacingRight = true;

    public Transform wallCheck;
    public float wallCheckDistance = 0.6f;
    public float groundCheckDistance = 0.7f;

    private float flipCooldown = 3f;
    private float lastFlipTime = -1f;

    private CloseMonsterAttack closeMonsterAttack;
    private bool isAttacking = false;
    private bool isInvincible = false;
    public float damageCooldown = 0.3f;

    [Header("Health Bar")]
    public Image healthFillImage;

    [Header("Special Attacks")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    [Header("Drop Item")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;
    public float itemDropForce = 7f;

    [Header("Audio Clips")]
    public AudioClip attackSound;
    public AudioClip dieSound;
    public AudioClip fireballSound;
    public AudioClip chargeSound;

    private AudioSource audioSource;

    // ✅ 포털 관련 변수 추가
    public GameObject portalToNextScene;
    public string targetSceneName = "1-3 Elite";
    public System.Action<MonoBehaviour> OnMonsterDie;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        closeMonsterAttack = GetComponent<CloseMonsterAttack>();
        audioSource = GetComponent<AudioSource>();

        currentState = State.Patrol;
        currentHealth = maxHealth;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 1f;
        }
        else
        {
            Debug.LogError("HealthFillImage가 연결되지 않았습니다!");
        }

        if (portalToNextScene != null)
        {
            portalToNextScene.SetActive(false);
        }

    }

    private void Update()
    {
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
            case State.Dead:
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
        float distance = Vector2.Distance(transform.position, player.position);
        float dirX = player.position.x - transform.position.x;
        Flip(dirX);

        if (distance <= 4f)
        {
            if (!isAttacking)
                StartCoroutine(AttackRoutine());
        }
        else if (distance >= 7f && distance <= 9f && !isAttacking)
        {
            int choice = Random.Range(0, 2);
            if (choice == 0)
                StartCoroutine(RangedAttackRoutine());
            else
                StartCoroutine(ChargeAttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        PlayAnimation("Attack");

        if (attackSound != null)
            audioSource.PlayOneShot(attackSound, 0.3f);

        yield return new WaitForSeconds(0.5f);
        closeMonsterAttack?.DoAttack(isFacingRight);

        yield return new WaitForSeconds(1f);
        PlayAnimation("Idle");
        yield return new WaitForSeconds(2f);
        isAttacking = false;
    }

    IEnumerator RangedAttackRoutine()
    {
        isAttacking = true;
        PlayAnimation("Attack2");
        yield return new WaitForSeconds(0.5f);

        Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
        GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        fireball.GetComponent<Fireball>().Initialize(isFacingRight ? Vector2.right : Vector2.left);

        if (fireballSound != null)
            audioSource.PlayOneShot(fireballSound, 0.3f);

        PlayAnimation("Idle");
        yield return new WaitForSeconds(2f);
        isAttacking = false;
    }

    IEnumerator ChargeAttackRoutine()
    {
        isAttacking = true;
        PlayAnimation("Attack5");
        yield return new WaitForSeconds(0.3f);

        if (chargeSound != null)
            audioSource.PlayOneShot(chargeSound, 0.3f);

        float chargeSpeed = 12f;
        float duration = 0.8f;
        float elapsed = 0f;
        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;

        while (elapsed < duration)
        {
            rb.linearVelocity = new Vector2(dir.x * chargeSpeed, rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.3f);
        PlayAnimation("Idle");
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;

        Vector3 wcPos = wallCheck.localPosition;
        wcPos.x *= -1;
        wallCheck.localPosition = wcPos;

        if (fireballSpawnPoint != null)
        {
            Vector3 fbPos = fireballSpawnPoint.localPosition;
            fbPos.x *= -1;
            fireballSpawnPoint.localPosition = fbPos;
        }
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

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < attackRange || (distance >= 7f && distance <= 9f))
            currentState = State.Attack;
        else if (distance < chaseRange)
            currentState = State.Chase;
        else
            currentState = State.Patrol;
    }

    public void TakeDamage(float damage)
    {
        if (currentState == State.Dead || isInvincible) return;

        isInvincible = true;
        currentHealth -= damage;

        if (healthFillImage != null)
        {
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
        Invoke(nameof(RecoverFromHit), 0.5f);
    }

    void RecoverFromHit()
    {
        currentState = State.Idle;
    }

    public void OnDie()
    {
        if (currentState == State.Dead) return;

        currentState = State.Dead;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        isAttacking = false;

        if (dieSound != null)
            audioSource.PlayOneShot(dieSound, 0.3f);

        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.Play("Die");
        }

        if (SceneManager.GetActiveScene().name == targetSceneName && portalToNextScene != null)
        {
            portalToNextScene.SetActive(true);
        }

        Invoke(nameof(DestroySelf), 1f);
    }

    void DestroySelf()
    {
        DropMultipleItems(coinPrefab, 5);
        DropMultipleItems(heartPrefab, 3);

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

    void DropMultipleItems(GameObject itemPrefab, int count)
    {
        if (itemPrefab == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);
            GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
            if (itemRb != null)
            {
                Vector2 randomDir = Random.insideUnitCircle + Vector2.up * 1.5f;
                Vector2 force = randomDir.normalized * itemDropForce;
                itemRb.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }
}