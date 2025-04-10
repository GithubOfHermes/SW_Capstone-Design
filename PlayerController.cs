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
    private bool lastWasDashAttack = false;
    private List<Collider2D> ignoredEnemies = new List<Collider2D>();
    private List<Collider2D> ignoredPlatforms = new List<Collider2D>();
    private bool isInvincible = false;
    private bool isDead = false;
    [SerializeField] private GameObject Damage;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private float lastDamageTime = -999f;
    [SerializeField] private float repeatDamageCooldown = 0.7f;

    [Header("Dash Settings")]
    private int currentDashCount = 2;
    private int maxDashCount = 2;
    private float dashCooldown = 2.5f;
    private float dashDuration = 0.5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;

    [Header("Skills")]
    private float skill1Cooldown = 5f;
    private float skill2Cooldown = 5f;
    private float lastSkill1Time = -999f;
    private float lastSkill2Time = -999f;
    [SerializeField] private GameObject skill1Prefab; // Skill_1 프리팹 

    [Header("Animation")]
    private readonly string IDLE = "Idle";
    private readonly string RUN = "Run";
    private readonly string JUMP = "jump";
    private readonly string JUMP_TO_FALL = "JumptoFall";
    private readonly string ATTACK = "Attack";
    private readonly string DASH = "Dash";
    private readonly string DASHATTACK = "DashAttack";
    private readonly string HURT = "Hurt";
    private readonly string DEATH = "Death";

    private bool isCroushing = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        StartCoroutine(DashRechargeRoutine());
    }

    public bool IsInvincible() => isInvincible;

    public void TakeDamage(float enemyDamage)
    {
        if (isDead || isInvincible || Time.time - lastDamageTime < repeatDamageCooldown) return;

        lastDamageTime = Time.time;
        currentHealth -= enemyDamage;
        Debug.Log($"Player가 {enemyDamage}를 받았다! 현재재 HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtRoutine());
        }
    }

    private void Die()
    {
        isDead = true;
        animator.Play(DEATH);
        rb.linearVelocity = Vector2.zero;
        enabled = false;
        StartCoroutine(WaitForDeathAnimation());
    }

    private IEnumerator WaitForDeathAnimation()
    {
        float deathAnimLength = 0f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == DEATH)
            {
                deathAnimLength = clip.length - 0.1f;
                break;
            }
        }

        yield return new WaitForSeconds(deathAnimLength);
        Time.timeScale = 0f;
    }

    private IEnumerator HurtRoutine()
    {
        isInvincible = true;
        animator.Play(HURT, 0, 0f);
        yield return new WaitForSeconds(0.4f); // 피격 애니메이션 시간
        isInvincible = false;

        if (!isAttacking)
        {
            if (isGrounded)
            {
                float moveInput = Input.GetAxisRaw("Horizontal");
                if (moveInput == 0)
                    PlayAnimationIfNotPlaying(IDLE);
                else
                    PlayAnimationIfNotPlaying(RUN);
            }
            else
            {
                PlayAnimationIfNotPlaying(JUMP);
            }
        }
    }

    private void Update()
    {
        if (isDashing || isDead) return;

        float moveInput = Input.GetAxisRaw("Horizontal");

        // Movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Flip
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput) * 2, 2, 2);
        }

        // Skills
        if (Input.GetKeyDown(KeyCode.Q) && Time.time - lastSkill1Time >= skill1Cooldown)
        {
            UseSkill1();
        }
        if (Input.GetKeyDown(KeyCode.E) && Time.time - lastSkill2Time >= skill2Cooldown)
        {
            UseSkill2();
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

        // DownJump
        if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            StartCoroutine(CroushRoutine());
        }
        // Regular Jump
        else if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            if (!isAttacking)
            {
                PlayAnimationIfNotPlaying(JUMP);
            }
        }

        // Falling
        if (!isGrounded && rb.linearVelocity.y < 0 && !isAttacking && !isCroushing)
        {
            PlayAnimationIfNotPlaying(JUMP);
            PlayAnimationIfNotPlaying(JUMP_TO_FALL);
        }

        // Dash
        if (Input.GetMouseButtonDown(1) && currentDashCount > 0 && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }

        // Movement Animations
        if (!isAttacking && isGrounded && !isInvincible && !isCroushing)
        {
            if (moveInput == 0)
                PlayAnimationIfNotPlaying(IDLE);
            else
                PlayAnimationIfNotPlaying(RUN);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
            isGrounded = false;
    }

    private IEnumerator DashRoutine()
    {
        currentDashCount--;
        isDashing = true;
        PlayAnimationIfNotPlaying(DASH);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] enemyDamages = GameObject.FindGameObjectsWithTag("EnemyDamage");
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        Collider2D playerCol = GetComponent<Collider2D>();

        // Ignore enemy collisions
        foreach (var enemy in enemies)
        {
            foreach (var col in enemy.GetComponentsInChildren<Collider2D>())
            {
                if (col != null && playerCol != null)
                {
                    Physics2D.IgnoreCollision(playerCol, col, true);
                    ignoredEnemies.Add(col);
                }
            }
        }

        foreach (var enemyDamage in enemyDamages)
        {
            foreach (var col in enemyDamage.GetComponentsInChildren<Collider2D>())
            {
                if (col != null && playerCol != null)
                {
                    Physics2D.IgnoreCollision(playerCol, col, true);
                    ignoredEnemies.Add(col);
                }
            }
        }

        // Ignore platform collisions
        foreach (var platform in platforms)
        {
            foreach (var col in platform.GetComponentsInChildren<Collider2D>())
            {
                if (col != null && playerCol != null)
                {
                    Physics2D.IgnoreCollision(playerCol, col, true);
                    ignoredPlatforms.Add(col);
                }
            }
        }

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

        rb.linearVelocity = dashDirection.normalized * moveSpeed * 3.0f;

        yield return new WaitForSeconds(dashDuration);

        // Restore enemy collisions
        foreach (var col in ignoredEnemies)
        {
            if (col != null && playerCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, col, false);
            }
        }
        ignoredEnemies.Clear();

        // Restore platform collisions
        foreach (var col in ignoredPlatforms)
        {
            if (col != null && playerCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, col, false);
            }
        }
        ignoredPlatforms.Clear();

        isDashing = false;
    }

    private IEnumerator PlatformIgnoringJump()
    {
        Collider2D playerCol = GetComponent<Collider2D>();
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");

        // Ignore platform collisions
        foreach (var platform in platforms)
        {
            foreach (var col in platform.GetComponentsInChildren<Collider2D>())
            {
                if (col != null && playerCol != null)
                {
                    Physics2D.IgnoreCollision(playerCol, col, true);
                    ignoredPlatforms.Add(col);
                }
            }
        }

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        PlayAnimationIfNotPlaying(JUMP);

        yield return new WaitForSeconds(0.5f);

        // Restore platform collisions
        foreach (var col in ignoredPlatforms)
        {
            if (col != null && playerCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, col, false);
            }
        }
        ignoredPlatforms.Clear();
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

        if (Damage != null)
        {
            Damage.SetActive(true);
        }

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

        if (Damage != null)
        {
            Damage.SetActive(false);
        }

        isAttacking = false;
    }

    private void PlayAnimationIfNotPlaying(string animationName)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }

    private void UseSkill1()
    {
        lastSkill1Time = Time.time;
        Debug.Log("Skill1 사용");
        
        // DashAttack 애니메이션 재생
        StartCoroutine(PlayAttackAnimation(DASHATTACK));
        
        // Skill_1 생성
        if (skill1Prefab != null)
        {
            float direction = transform.localScale.x > 0 ? 1f : -1f;
            Vector3 spawnPos = transform.position + new Vector3(direction * 2f, 0f, 0f);
            GameObject skill = Instantiate(skill1Prefab, spawnPos, Quaternion.identity);
            skill.transform.localScale = new Vector3(direction, 1f, 1f); // 방향 설정
        }
    }

    private void UseSkill2()
    {
        lastSkill2Time = Time.time;
        Debug.Log("Skill2 사용");
        // 여기에 Skill2의 구현 가능
    }

    private IEnumerator CroushRoutine()
    {
        isCroushing = true;
        
        Collider2D playerCol = GetComponent<Collider2D>();
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        List<Collider2D> ignoredPlatforms = new List<Collider2D>();

        // Ignore platform collisions
        foreach (var platform in platforms)
        {
            foreach (var col in platform.GetComponentsInChildren<Collider2D>())
            {
                if (col != null && playerCol != null)
                {
                    Physics2D.IgnoreCollision(playerCol, col, true);
                    ignoredPlatforms.Add(col);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Restore platform collisions
        foreach (var col in ignoredPlatforms)
        {
            if (col != null && playerCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, col, false);
            }
        }

        isCroushing = false;
    }
}