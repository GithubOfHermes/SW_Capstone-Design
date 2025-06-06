using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCol;
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
    [SerializeField] private float repeatDamageCooldown = 1.2f;

    [Header("Dash Settings")]
    private float dashCooldown = 2.5f;
    private float dashDuration = 0.6f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Skills")]
    private float skill1Cooldown = 3f;
    private float skill2Cooldown = 15f;
    private float lastSkill1Time = -999f;
    private float lastSkill2Time = -999f;
    [SerializeField] private GameObject skill1Prefab; // Skill_1 프리팹 
    [SerializeField] private GameObject skill2Prefab; // Skill_2 프리팹 
    [SerializeField] private GameObject synergy1Prefab; // Synergy1 프리팹 
    private bool isSkill2Active = false;
    private float skill2Duration = 3f;
    private float skill2Multiplier = 2f;

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
    [SerializeField] private float attackDamage = 5f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip skill1_Sound;
    public AudioClip skill2_Sound;
    public AudioClip dashSound;
    public AudioClip jumpSound;
    public AudioClip deathSound;
    public AudioClip hurtSound;
    public AudioClip coinSound;
    public AudioClip heartSound;

    private bool isCroushing = false;
    private bool isHurting = false;
    public static float pendingInvincibilityTime = 0f; // 대기 중인 무적 시간


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCol = GetComponent<Collider2D>();

        maxHP = 100 + PlayerUpgradeData.bonusMaxHP;

        if (PlayerStateService.savedHp > 0)
        {
            currentHP = PlayerStateService.savedHp;
        }
        else
        {
            currentHP = maxHP;
        }

        attackDamage += PlayerUpgradeData.bonusAttackDamage;
        audioSource = GetComponent<AudioSource>();
        if (SoundManager.Instance == null)
        {
            GameObject sm = new GameObject("SoundManager");
            sm.AddComponent<SoundManager>();
        }

        if (pendingInvincibilityTime > 0f)
        {
            StartCoroutine(SceneTransitionInvincibility(pendingInvincibilityTime));
            pendingInvincibilityTime = 0f;
        }

        // ← 이동 중에도 Enemy 충돌 무시 (단 데미지는 받도록)
        InvokeRepeating(nameof(IgnoreEnemyCollisionPermanently), 0f, 1f);
        InvokeRepeating(nameof(IgnoreHeartCollision), 0f, 1f);
    }

    // ← 적과 물리 충돌은 항상 무시 (대시 전후 관계없이), 데미지는 여전히 유효함
    private void IgnoreEnemyCollisionPermanently()
    {
        Collider2D playerCol = GetComponent<Collider2D>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        /*foreach (GameObject enemy in enemies)
        {
            foreach (Collider2D enemyCol in enemy.GetComponentsInChildren<Collider2D>())
            {
                if (playerCol != null && enemyCol != null)
                {

                    if (enemyCol.CompareTag("EnemyAttack") || enemyCol.CompareTag("EnemyDamage"))
                    continue;

                    Physics2D.IgnoreCollision(playerCol, enemyCol, true);
                }
            }
        }*/
        foreach (GameObject enemy in enemies)
        {
            // 본체의 Collider만 무시
            Collider2D bodyCol = enemy.GetComponent<Collider2D>();
            if (playerCol != null && bodyCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, bodyCol, true);

            }
        }

    }

    public bool IsInvincible() => isInvincible;

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible || Time.time - lastDamageTime < repeatDamageCooldown) return;

        lastDamageTime = Time.time;

        // 피해 감소 적용
        float reducedDamage = damage * (1f - Mathf.Clamp01(PlayerUpgradeData.damageReductionPercent));
        int finalDamage = Mathf.RoundToInt(reducedDamage);

        currentHP -= finalDamage;

        Debug.Log($"Player가 {damage} → {finalDamage}의 데미지를 받았다! 현재 HP: {currentHP}");

        // 경감된 데미지로 UI 업데이트
        HUDManager.Instance.ReduceHP(finalDamage);

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            if (!isInvincible) // 애니메이션 중복 방지
            {
                StartCoroutine(HurtRoutine());
            }
        }
    }

    private void Die()
    {
        isDead = true;
        if (deathSound != null)
        {
            SoundManager.Instance.PlaySound(deathSound, 0.02f);
        }
        animator.Play(DEATH);
        rb.linearVelocity = Vector2.zero;
        enabled = false;
        HUDManager.Instance?.ShowDeadScreen();
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
        isHurting = true;
        isInvincible = true;
        currentAnim = HURT;
        animator.Play(HURT);

        if (hurtSound != null)
        {
            SoundManager.Instance.PlaySound(hurtSound, 0.05f);
        }

        float hurtAnimLength = 0.5f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == HURT)
            {
                hurtAnimLength = clip.length * 0.95f;
                break;
            }
        }

        yield return new WaitForSeconds(hurtAnimLength); // 애니메이션 끝

        isHurting = false; // 여기서 풀어줌

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

        float remainingTime = repeatDamageCooldown - hurtAnimLength;
        if (remainingTime > 0)
            yield return new WaitForSeconds(remainingTime);

        isInvincible = false;
        currentAnim = "";
    }

    private void Update()
    {
        if (isDashing || isDead) return;

        // Trap 데미지 처리
        if (isOnTrap)
        {
            TakeDamage(10f);
        }

        float moveInput = Input.GetAxisRaw("Horizontal");

        // Movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Flip
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput) * 1.15f, 1.15f, 1.15f);
        }

        // Skills
        float adjustedSkill1Cooldown = skill1Cooldown * (1f - Mathf.Clamp01(PlayerUpgradeData.skillCooldownReductionPercent));
        if (Input.GetKeyDown(KeyCode.Q) && Time.time - lastSkill1Time >= adjustedSkill1Cooldown)
        {
            UseSkill1();
        }
        float adjustedSkill2Cooldown = skill2Cooldown * (1f - Mathf.Clamp01(PlayerUpgradeData.skillCooldownReductionPercent));
        if (Input.GetKeyDown(KeyCode.E) && Time.time - lastSkill2Time >= adjustedSkill2Cooldown)
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
            if (jumpSound != null)
            {
                SoundManager.Instance.PlaySound(jumpSound, 0.02f);
            }
            if (!isAttacking)
            {
                PlayAnimationIfNotPlaying(JUMP);
            }
        }

        // Falling
        if (!isHurting && !isGrounded && rb.linearVelocity.y < 0 && !isAttacking && !isCroushing)
        {
            PlayAnimationIfNotPlaying(JUMP);
            PlayAnimationIfNotPlaying(JUMP_TO_FALL);
        }

        // Dash
        if (Input.GetMouseButtonDown(1) && !isDashing)
        {
            if (HUDManager.Instance != null && HUDManager.Instance.RequestDash())
            {
                StartCoroutine(DashRoutine());
            }
        }

        // Movement Animations
        if (!isAttacking && isGrounded && !isInvincible && !isCroushing)
        {
            if (moveInput == 0)
                PlayAnimationIfNotPlaying(IDLE);
            else
                PlayAnimationIfNotPlaying(RUN);
        }

        // Z 키를 눌렀을 때 가장 가까운 아이템 줍기
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ItemPickup closest = PickupController.GetClosestPickup(transform.position);
            if (closest != null)
            {
                if (heartSound != null)
                {
                    SoundManager.Instance.PlaySound(heartSound, 0.05f);
                }
                PickupEffectHandler.Apply(closest.pickupType, closest.healAmount, closest.goldAmount);
                Destroy(closest.transform.root.gameObject); // 전체 프리팹 제거
            }
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

        // 추가: Coin과 충돌 시 sound 재생
        ItemPickup pickup = other.GetComponent<ItemPickup>();
        if (pickup != null && pickup.pickupType == ItemPickup.PickupType.Coin)
        {
            if (coinSound != null)
            {
                SoundManager.Instance.PlaySound(coinSound, 0.05f);
            }
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
        isDashing = true;
        isInvincible = true; // 무적 시작
        currentAnim = DASH;
        animator.Play(DASH);

        if (dashSound != null)
        {
            SoundManager.Instance.PlaySound(dashSound, 0.03f);
        }

        GameObject[] enemyAttacks = GameObject.FindGameObjectsWithTag("EnemyAttack");
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");

        // 충돌 무시 설정
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

        bool isOnPlatform = false;
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        Collider2D[] hits = new Collider2D[5];
        int count = playerCol.Overlap(filter, hits);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].CompareTag("Platform"))
            {
                isOnPlatform = true;
                break;
            }
        }

        if (!isOnPlatform)
        {
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
        }

        // 입력 방향 계산
        Vector2 dashDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) dashDirection += Vector2.up;
        if (Input.GetKey(KeyCode.S)) dashDirection += Vector2.down;
        if (Input.GetKey(KeyCode.A)) dashDirection += Vector2.left;
        if (Input.GetKey(KeyCode.D)) dashDirection += Vector2.right;

        if (dashDirection == Vector2.zero)
        {
            dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        }

        // 정규화로 방향 벡터 통일
        dashDirection = dashDirection.normalized;

        float dashSpeed = moveSpeed * 3.2f;

        // 좌우 대시 거리만 보정 
        if (dashDirection.y == 0 && Mathf.Abs(dashDirection.x) > 0)
        {
            dashSpeed *= 0.55f;
        }


        rb.linearVelocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // 대시 종료 후 x속도만 정지
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        foreach (var col in ignoredPlatforms)
        {
            if (col != null && playerCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, col, false);
            }
        }
        ignoredPlatforms.Clear();

        isDashing = false;
        isInvincible = false; // 무적 종료료
        currentAnim = "";
        HUDManager.Instance.EndDash();
    }

    public float GetDashCooldown()
    {
        return Mathf.Max(1.0f, dashCooldown - PlayerUpgradeData.dashCooldownReduction);
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

    private IEnumerator PlayAttackAnimation(string animationName)
    {
        isAttacking = true;
        currentAnim = animationName; // 현재 애니메이션 이름 고정
        animator.Play(animationName);

        if (attackSound != null)
        {
            SoundManager.Instance.PlaySound(attackSound, 0.05f);
        }

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

        // 사운드 재생 추가
        if (skill1_Sound != null)
        {
            SoundManager.Instance.PlaySound(skill1_Sound, 0.01f);
        }

        // DashAttack 애니메이션 재생
        StartCoroutine(PlayAttackAnimation(DASHATTACK));

        // Skill_1 또는 Synergy1 생성
        GameObject prefabToUse = isSkill2Active ? synergy1Prefab : skill1Prefab;
        if (prefabToUse != null)
        {
            // 1. 방향 계산 (상하 우선)
            Vector2 shootDirection = Vector2.zero;
            if (Input.GetKey(KeyCode.W)) shootDirection = Vector2.up;
            else if (Input.GetKey(KeyCode.S)) shootDirection = Vector2.down;
            else shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            // 2. 생성 위치 조정
            Vector3 offset = (Vector3)(shootDirection.normalized * 2f);
            GameObject skill = Instantiate(prefabToUse, transform.position + offset, Quaternion.identity);

            // 3. 방향 설정 및 회전 반영
            Skill_1 skill1Script = skill.GetComponent<Skill_1>();
            if (skill1Script != null)
            {
                skill1Script.SetPiercing(isSkill2Active);
                skill1Script.SetSkill2Active(isSkill2Active);
                skill1Script.SetDirection(shootDirection);
            }
        }
    }

    private void UseSkill2()
    {
        if (isSkill2Active) return;
        lastSkill2Time = Time.time;
        Debug.Log("Skill2 사용");
        if (skill2_Sound != null)
        {
            SoundManager.Instance.PlaySound(skill2_Sound, 0.03f);
        }

        // Skill_2 GameObject 활성화
        if (skill2Prefab != null)
        {
            skill2Prefab.SetActive(true);
        }

        // Skill_1 관통 활성화 및 데미지 증가
        if (skill1Prefab != null)
        {
            Skill_1 skill1Script = skill1Prefab.GetComponent<Skill_1>();
            if (skill1Script != null)
            {
                skill1Script.SetSkill2Active(true);
            }
        }

        StartCoroutine(Skill2Routine());
    }

    private IEnumerator Skill2Routine()
    {
        isSkill2Active = true;
        float originalDamage = attackDamage;
        attackDamage *= skill2Multiplier;
        yield return new WaitForSeconds(skill2Duration);
        attackDamage = originalDamage;
        isSkill2Active = false;

        // Skill_2 GameObject 비활성화
        if (skill2Prefab != null)
        {
            skill2Prefab.SetActive(false);
        }

        // Skill_1 데미지 초기화
        if (skill1Prefab != null)
        {
            Skill_1 skill1Script = skill1Prefab.GetComponent<Skill_1>();
            if (skill1Script != null)
            {
                skill1Script.SetSkill2Active(false);
            }
        }
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
            LongRangeMonsterController longEnemy = collision.GetComponent<LongRangeMonsterController>();
            CloseRangeMonsterController closeEnemy = collision.GetComponent<CloseRangeMonsterController>();
            EliteMonsterController eliteEnemy = collision.GetComponent<EliteMonsterController>();
            FlyingCloseRangeMonsterController flyEnemy = collision.GetComponent<FlyingCloseRangeMonsterController>();
            BossHealthSystem bossEnemy = collision.GetComponent<BossHealthSystem>();
            if (longEnemy != null)
            {
                longEnemy.TakeDamage(attackDamage);
            }
            if (closeEnemy != null)
            {
                closeEnemy.TakeDamage(attackDamage);
            }
            if (eliteEnemy != null)
            {
                eliteEnemy.TakeDamage(attackDamage);
            }
            if (flyEnemy != null)
            {
                flyEnemy.TakeDamage(attackDamage);
            }
            if (bossEnemy != null)
            {
                bossEnemy.TakeDamage(attackDamage);
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

    public void ForceIdleAndStopDash()
    {
        isDashing = false;
        currentAnim = "";
        PlayAnimationIfNotPlaying(IDLE);
    }

    public void IncreaseAttack(float amount)
    {
        attackDamage += amount;
        Debug.Log($"[플레이어] 공격력 증가됨: 현재 공격력 = {attackDamage}");
    }

    public IEnumerator SceneTransitionInvincibility(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    private void IgnoreHeartCollision()
    {
        GameObject[] hearts = GameObject.FindGameObjectsWithTag("Heart");
        foreach (var heart in hearts)
        {
            foreach (var col in heart.GetComponentsInChildren<Collider2D>())
            {
                if (col != null && playerCol != null)
                {
                    // Trigger는 무시하지 않음
                    if (!col.isTrigger && !playerCol.isTrigger)
                    {
                        Physics2D.IgnoreCollision(playerCol, col, true);
                    }
                }
            }
        }
    }

    public float GetAttackDamage()
    {
        return attackDamage;
    }

    public int GetSkill1CurrentDamage()
    {
        int baseDamage = 20; // Skill_1 기본값
        int bonus = PlayerUpgradeData.bonusSkill1Damage;
        bool skill2Active = isSkill2Active;

        int damage = baseDamage + bonus;
        if (skill2Active)
            damage = (int)(damage * 1.5f);

        return damage;
    }

    public int GetGoldGain()
    {
        return 5 + PlayerUpgradeData.bonusGoldGain;
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }

    public void IncreaseMaxHP(int amount)
    {
        maxHP += amount;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        PlayerStateService.savedHp = currentHP; // 씬 이동 시 체력 유지
        HUDManager.Instance?.UpdateHPUI(currentHP, maxHP);
    }
    public void RecoverHP(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        PlayerStateService.savedHp = currentHP; // 씬 이동 대비 저장
        HUDManager.Instance?.UpdateHPUI(currentHP, maxHP); // UI 갱신
    }
}