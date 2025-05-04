using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KnightController : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Hit, Dead }
    public State currentState = State.Idle;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float chaseRange = 5f;
    public float attackRange = 1.5f;
    public LayerMask groundLayer;

    public int maxHealth = 30;
    private int currentHealth;

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

    private KnightAttack2 knightAttack;
    private bool isAttacking = false;
    private bool isInvincible = false;
    public float damageCooldown = 0.3f;

    [Header("Health Bar")]
    public Image healthFillImage;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        knightAttack = GetComponent<KnightAttack2>();
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
        PlayAnimation("Knight_IDLE");
    }

    void Patrol()
    {
        PlayAnimation("Knight_Walk");

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
        PlayAnimation("Knight_Run");

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

        if (!isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        PlayAnimation("Knight_Attack2");
        knightAttack?.DoAttack(isFacingRight);

        yield return new WaitForSeconds(1f);
        PlayAnimation("Knight_IDLE");
        yield return new WaitForSeconds(2f);

        isAttacking = false;
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
        if (currentState == State.Dead || currentState == State.Hit) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < attackRange)
            currentState = State.Attack;
        else if (distance < chaseRange)
            currentState = State.Chase;
        else
            currentState = State.Patrol;
    }

    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead || isInvincible) return;

        isInvincible = true;
        currentHealth -= damage;
        Debug.Log($"Knight가 {damage}의 데미지를 입음! 현재 HP: {currentHealth}");

        if (healthFillImage != null)
        {
            float fill = Mathf.Clamp01((float)currentHealth / maxHealth);
            healthFillImage.fillAmount = fill;
            Debug.Log($"체력바 업데이트: {fill}");
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
        PlayAnimation("Knight_Hit");
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

        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.Play("Knight_Die");
        }

        Invoke(nameof(DestroySelf), 0.5f);
    }

    void DestroySelf()
    {
        if (healthFillImage != null)
            healthFillImage.transform.parent.gameObject.SetActive(false);

        Destroy(gameObject);
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
        if (collision.CompareTag("PlayerDamage") || collision.CompareTag("Trap"))
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
}
