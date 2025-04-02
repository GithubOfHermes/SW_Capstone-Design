using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isDashing = false;
    private bool isAttacking = false;
    private bool lastWasDashAttack = false;  // 마지막 공격이 DashAttack이었는지 추적
    private List<Collider2D> ignoredEnemies = new List<Collider2D>();

    [Header("Dash Settings")]
    private int currentDashCount = 2;   // 현재 대쉬 횟수
    private int maxDashCount = 2;       // 최대 대쉬 횟수
    private float dashCooldown = 2.5f;  // 대쉬 쿨타임
    private float dashDuration = 0.3f;  // 대쉬 무적 시간

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;

    // Animation parameter names
    private readonly string IDLE = "Idle";
    private readonly string RUN = "Run";
    private readonly string JUMP = "jump";
    private readonly string JUMP_TO_FALL = "JumptoFall";
    private readonly string ATTACK = "Attack";
    private readonly string DASH = "Dash";
    private readonly string DASHATTACK = "DashAttack";

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        StartCoroutine(DashRechargeRoutine()); // 대시 회복 시작
    }

    private void Update()
    {
        if (isDashing) return;

        float moveInput = Input.GetAxisRaw("Horizontal");

        // Movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Flip
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput) * 2, 2, 2);
        }

        // Attack
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            if (lastWasDashAttack)
            {
                StartCoroutine(PlayAttackAnimation(ATTACK));
                lastWasDashAttack = false;
            }
            else
            {
                StartCoroutine(PlayAttackAnimation(DASHATTACK));
                lastWasDashAttack = true;
            }
        }

        // Jump (애니메이션은 공격 중일 때 재생 X)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            if (!isAttacking)
            {
                PlayAnimationIfNotPlaying(JUMP);
            }
        }

        // Falling
        if (!isGrounded && rb.linearVelocity.y < 0 && !isAttacking)
        {
            PlayAnimationIfNotPlaying(JUMP);
            PlayAnimationIfNotPlaying(JUMP_TO_FALL);
        }

        // Dash (연속 2회 가능)
        if (Input.GetMouseButtonDown(1) && currentDashCount > 0 && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }

        // Movement animations
        if (!isAttacking && isGrounded)
        {
            if (moveInput == 0)
                PlayAnimationIfNotPlaying(IDLE);
            else
                PlayAnimationIfNotPlaying(RUN);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    private IEnumerator DashRoutine()
    {
        currentDashCount--;
        isDashing = true;
        PlayAnimationIfNotPlaying(DASH);

        // 무적 처리: Enemy 태그 오브젝트와 충돌 무시
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Collider2D playerCol = GetComponent<Collider2D>();

        foreach (var enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (enemyCol != null && playerCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, enemyCol, true);
                ignoredEnemies.Add(enemyCol);
            }
        }

        // 방향 계산 (대각선 포함)
        Vector2 dashDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) dashDirection += Vector2.up;
        if (Input.GetKey(KeyCode.S)) dashDirection += Vector2.down;
        if (Input.GetKey(KeyCode.A)) dashDirection += Vector2.left;
        if (Input.GetKey(KeyCode.D)) dashDirection += Vector2.right;

        if (dashDirection == Vector2.zero)
        {
            float xDir = transform.localScale.x > 0 ? 1f : -1f;
            dashDirection = new Vector2(xDir, 0f);
        }

        rb.linearVelocity = dashDirection.normalized * moveSpeed * 3f;

        yield return new WaitForSeconds(dashDuration);

        // 무적 해제
        foreach (var enemyCol in ignoredEnemies)
        {
            if (enemyCol != null && playerCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, enemyCol, false);
            }
        }
        ignoredEnemies.Clear();

        isDashing = false;
    }

    private IEnumerator DashRechargeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(dashCooldown);

            if (currentDashCount < maxDashCount)
            {
                currentDashCount++;
            }
        }
    }

    private IEnumerator PlayAttackAnimation(string animationName)
    {
        isAttacking = true;
        animator.Play(animationName);

        float animLength = 0.4f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
            {
                animLength = clip.length * 0.95f;
                break;
            }
        }

        yield return new WaitForSeconds(animLength);
        isAttacking = false;
    }

    private void PlayAnimationIfNotPlaying(string animationName)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }
}
