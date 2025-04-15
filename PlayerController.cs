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
    private bool isOnTrap = false;
    [SerializeField] private GameObject Damage;

    [Header("Health")]
    private int currentHP;
    private int maxHP = 100;
    private float lastDamageTime = -999f;
    [SerializeField] private float repeatDamageCooldown = 0.3f;

    [Header("Dash Settings")]
    private int currentDashCount = 3;
    private int maxDashCount = 3;
    private float dashCooldown = 2.5f;
    private float dashDuration = 0.25f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 15f;

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

    [Header("Attack")]
    [SerializeField] private int attackDamage = 5;

    private bool isCroushing = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHP = maxHP;
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.SetInitialHP(maxHP);
        }
        StartCoroutine(DashRechargeRoutine());
    }

    public bool IsInvincible() => isInvincible;

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible || Time.time - lastDamageTime < repeatDamageCooldown) return;

        lastDamageTime = Time.time;
        currentHP -= (int)damage;
        Debug.Log($"Player가 {damage}의 데미지를 받았다! 현재 HP: {currentHP}");

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.SetHP(currentHP);
        }

        if (currentHP <= 0)
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
        animator.Play(HURT);
        yield return new WaitForSeconds(0.2f);
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

        // Trap 데미지 처리
        if (isOnTrap)
        {
            TakeDamage(5f);
        }

        float moveInput = Input.GetAxisRaw("Horizontal");

        // Movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Flip
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput) * 1.5f, 1.5f, 1.5f);
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

    private int groundContactCount = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            groundContactCount++;
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            groundContactCount = Mathf.Max(0, groundContactCount - 1);
            if (groundContactCount == 0)
                isGrounded = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            isOnTrap = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            isOnTrap = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            isOnTrap = true;
        }
    }

    private IEnumerator DashRoutine()
    {
        currentDashCount--;
        isDashing = true;
        PlayAnimationIfNotPlaying(DASH);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] enemyAttacks = GameObject.FindGameObjectsWithTag("EnemyAttack");
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

        foreach (var enemyAttack in enemyAttacks)
        {
            foreach (var col in enemyAttack.GetComponentsInChildren<Collider2D>())
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

        float dashTimer = 0f;
        Vector2 initialVerticalDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) initialVerticalDirection = Vector2.up;
        if (Input.GetKey(KeyCode.S)) initialVerticalDirection = Vector2.down;

        while (dashTimer < dashDuration)
        {
            Vector2 dashDirection = Vector2.zero;
            
            // 수직 방향은 초기 입력값 유지
            dashDirection += initialVerticalDirection;
            
            // 수평 방향은 실시간으로 업데이트
            if (Input.GetKey(KeyCode.A)) dashDirection += Vector2.left;
            if (Input.GetKey(KeyCode.D)) dashDirection += Vector2.right;

            if (dashDirection == Vector2.zero)
            {
                float xDir = transform.localScale.x > 0 ? 1f : -1f;
                dashDirection = new Vector2(xDir, 0f);
            }

            rb.linearVelocity = dashDirection.normalized * moveSpeed * 3.0f;
            
            dashTimer += Time.deltaTime;
            yield return null;
        }

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
        currentAnim = animationName; // 현재 애니메이션 이름 고정
        animator.Play(animationName);

        if (Damage != null)
        {
            Damage.SetActive(true);
            BoxCollider2D damageCollider = Damage.GetComponent<BoxCollider2D>();
            if (damageCollider == null)
            {
                damageCollider = Damage.AddComponent<BoxCollider2D>();
                damageCollider.isTrigger = true;
            }
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

        currentAnim = ""; // 공격 종료 후 currentAnim 초기화
    }


    private string currentAnim = "";

    private void PlayAnimationIfNotPlaying(string animationName)
    {
        if (currentAnim == animationName) return;

        animator.Play(animationName);
        currentAnim = animationName;
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

    private void OnDamageCollision(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    private void OnEnable()
    {
        if (Damage != null)
        {
            BoxCollider2D damageCollider = Damage.GetComponent<BoxCollider2D>();
            if (damageCollider == null)
            {
                damageCollider = Damage.AddComponent<BoxCollider2D>();
                damageCollider.isTrigger = true;
            }
            
            DamageCollisionHandler handler = Damage.GetComponent<DamageCollisionHandler>();
            if (handler == null)
            {
                handler = Damage.AddComponent<DamageCollisionHandler>();
                handler.Initialize(OnDamageCollision);
            }
        }
    }
}