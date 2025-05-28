using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    [Header("등장 및 패턴 관련")]
    public Transform appearTargetPoint;
    public Transform appearStartPoint;
    public float appearDuration = 1.5f;
    public float jumpHeight = 7f;
    private List<int> patternQueue = new List<int>();
    public GameObject landingEffect;

    [Header("공격 패턴")]
    //public Collider2D attackCollider;
    public GameObject attackObject;
    public Transform player;
    public float attackDamage = 20;
    public int attackRepeatCount = 3;
    public float teleportOffsetX = 1.5f;
    public float preAttackDelay = 0.7f;
    public float postAttackDelay = 2f;
    public GameObject teleportEffect;

    [Header("돌진 패턴")]
    public Transform rushStartPoint;
    public Transform rushEndPoint;
    public float rushSpeed = 15f;
    public float rushDelay = 1f;
    public GameObject rushEffect;

    [Header("Phase 2 관련")]
    public Transform phase2Point;
    public GameObject phase2EffectObject;

    [Header("Phase 2 Sword Barrage")]
    public Transform[] swordSpawnPoints;
    public GameObject swordPrefab;
    public float swordShootDelay = 0.5f;
    public float swordSpeed = 10f;

    [Header("Phase 2 JumpAttack ")]
    public GameObject earthSpikePrefab;
    public int spikeCountPerSide = 5;
    public float spikeInterval = 1f;
    public float spikeSpawnDelay = 0.15f;
    public Transform GroundPoint;

    [Header("사운드 클립")]
    public AudioClip landingSound;
    public AudioClip jumpSound;
    public AudioClip jumpAttackSound;
    public AudioClip teleportSound;
    public AudioClip behindAttackSound;
    public AudioClip rushSound;
    public AudioClip energyBallStartSound;
    public AudioClip energyBallSpawnSound;
    public AudioClip phase2Sound;
    public AudioClip deathSound;
    public AudioClip spikeSpawnSound;
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCollider;
    private bool isAppearing = true;
    private float appearTimer = 0f;
    private bool isAttacking = false;
    public bool isPhase2 = false;

    private float originalGravity;

    public GameObject bossHealthUI;
    private BossHealthSystem bossHealthSystem;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        bossHealthSystem = GetComponent<BossHealthSystem>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
            player = GameObject.FindWithTag("Player").transform;

        originalGravity = rb.gravityScale;

        transform.position = appearStartPoint.position;
        rb.simulated = false;
        spriteRenderer.flipX = (appearTargetPoint.position.x < appearStartPoint.position.x);
        UpdateAttackColliderDirection();

        animator.Play("Idle");
        //attackCollider.enabled = false;
        attackObject.SetActive(false);

        if (phase2EffectObject != null)
            phase2EffectObject.SetActive(false);
    }

    public void StartAppearance()
    {
        animator.Play("Jump");
        StartCoroutine(Appear());

        if (bossHealthUI != null)
        {
            bossHealthUI.SetActive(true);
        }

        GetComponent<BossHealthSystem>()?.AnimateHealthFill();
    }

    private System.Collections.IEnumerator Appear()
    {
        yield return new WaitForSeconds(0.5f);

        while (appearTimer < appearDuration)
        {
            appearTimer += Time.deltaTime;
            float t = Mathf.Clamp01(appearTimer / appearDuration);

            float newX = Mathf.Lerp(appearStartPoint.position.x, appearTargetPoint.position.x, t);
            float heightOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            float newY = Mathf.Lerp(appearStartPoint.position.y, appearTargetPoint.position.y, t) + heightOffset;

            transform.position = new Vector2(newX, newY);

            yield return null;
        }

        transform.position = appearTargetPoint.position;
        rb.simulated = true;
        isAppearing = false;

        if (landingEffect != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0f, 0.8f, 0f);
            GameObject effect = Instantiate(landingEffect, spawnPos, Quaternion.identity);
            Destroy(effect, 0.3f);
        }

        PlayLandingSound();

        animator.Play("Landing");

        yield return new WaitForSeconds(0.5f);

        animator.Play("Idle");

        yield return new WaitForSeconds(2f);

        StartCoroutine(PatternLoop());
    }

    private void InitializePatternQueue(int count)
    {
        patternQueue.Clear();

        for (int i = 0; i < count; i++)
        {
            patternQueue.Add(i);
        }

        Shuffle(patternQueue);
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
    }

    private int GetNextPattern()
    {
        if (patternQueue.Count == 0)
        {
            int patternCount = isPhase2 ? 4 : 3;
            InitializePatternQueue(patternCount);
        }

        int nextPattern = patternQueue[0];
        patternQueue.RemoveAt(0);
        return nextPattern;
    }

    private IEnumerator PatternLoop()
    {
        while (true)
        {
            if (!isAttacking)
            {
                int patternIndex = GetNextPattern();

                if (patternIndex == 0)
                    yield return StartCoroutine(AttackPattern_TeleportBehindAndAttack());
                else if (patternIndex == 1)
                    yield return StartCoroutine(AttackPattern_RushAttack());
                else if (patternIndex == 2)
                    yield return StartCoroutine(AttackPattern_JumpToPlayerAttack());
                else if (patternIndex == 3)
                    yield return StartCoroutine(AttackPattern_Phase2SwordBarrage());

                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void EnterPhase2()
    {
        if (isPhase2) return;
        isPhase2 = true;

        var attackTrigger = attackObject.GetComponent<BossAttackTrigger>();
        if (attackTrigger != null)
        {
            attackTrigger.SetDamage(20f);
        }

        var damageComponent = GetComponentInChildren<EnemyDamage>();
        if (damageComponent != null)
        {
            damageComponent.SetDamage(20f);
        }

        StopAllCoroutines();
        isAttacking = false;

        transform.position = phase2Point.position;
        PlayPhase2Sound();

        if (phase2EffectObject != null)
            phase2EffectObject.SetActive(true);

        rb.simulated = false;
        bossCollider.enabled = false;
        rb.gravityScale = 0f;

        if (TryGetComponent<BossHealthSystem>(out BossHealthSystem healthSystem))
        {
            healthSystem.RestoreHealthOverTime(3f);
        }

        animator.Play("Phase2In");
        StartCoroutine(Phase2StartSequence());
    }

    private System.Collections.IEnumerator Phase2StartSequence()
    {
        yield return new WaitForSeconds(2f);

        rb.gravityScale = originalGravity;
        rb.simulated = true;
        bossCollider.enabled = true;

        attackRepeatCount = 5;
        rushSpeed *= 1.5f;
        postAttackDelay *= 0.5f;

        animator.Play("Idle");
        yield return new WaitForSeconds(2f);

        StartCoroutine(PatternLoop());
    }

    private System.Collections.IEnumerator AttackPattern_Phase2SwordBarrage()
    {
        isAttacking = true;

        Debug.Log("[Phase 2] 검 소환 패턴 시작");

        animator.Play("SwordAttack");
        PlayEnergyBallStartSound();
        yield return new WaitForSeconds(0.5f);
        animator.Play("Idle");

        GameObject[] swords = new GameObject[swordSpawnPoints.Length];

        for (int i = 0; i < swordSpawnPoints.Length; i++)
        {
            swords[i] = Instantiate(swordPrefab, swordSpawnPoints[i].position, Quaternion.identity);

            PlayEnergyBallSpawnSound();

            Vector2 dir = (player.position - swords[i].transform.position).normalized;
            swords[i].transform.right = dir;

            Rigidbody2D rb = swords[i].GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < swords.Length; i++)
        {
            Rigidbody2D rb = swords[i].GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (player.position - swords[i].transform.position).normalized;
                rb.linearVelocity = dir * swordSpeed;
            }

            yield return new WaitForSeconds(swordShootDelay);
        }

        yield return new WaitForSeconds(1f);

        animator.Play("Idle");
        isAttacking = false;
    }

    private System.Collections.IEnumerator AttackPattern_TeleportBehindAndAttack()
    {
        isAttacking = true;

        float preDelay = isPhase2 ? preAttackDelay * 0.6f : preAttackDelay;
        float postDelay = isPhase2 ? postAttackDelay * 0.6f : postAttackDelay;

        for (int i = 0; i < attackRepeatCount; i++)
        {

            // 순간이동 전 위치 저장
            Vector3 preTeleportPos = transform.position;

            Vector3 playerPos = player.position;
            bool isPlayerFacingRight = player.localScale.x > 0f;
            Vector3 behindPos = playerPos + new Vector3(isPlayerFacingRight ? -teleportOffsetX : teleportOffsetX, 0f, 0f);

            transform.position = behindPos;
            spriteRenderer.flipX = (player.position.x < transform.position.x);
            UpdateAttackColliderDirection();
            PlayTeleportSound();

            if (teleportEffect != null)
            {
                GameObject effect = Instantiate(teleportEffect, preTeleportPos, Quaternion.identity);
                Destroy(effect, 0.5f);
            }

            animator.Play("Idle");
            yield return new WaitForSeconds(preDelay);

            animator.Play("BehindAttack");
            attackObject.SetActive(true);
            //attackCollider.enabled = true;
            PlayBehindAttackSound();

            yield return new WaitForSeconds(0.2f);

            attackObject.SetActive(false);
            //attackCollider.enabled = false;
            yield return new WaitForSeconds(0.3f);
            animator.Play("Idle");
            yield return new WaitForSeconds(postDelay);
        }

        animator.Play("Idle");
        isAttacking = false;
    }

    private System.Collections.IEnumerator AttackPattern_RushAttack()
    {
        isAttacking = true;

        Vector2[] rushPoints = new Vector2[] { rushStartPoint.position, rushEndPoint.position };

        for (int i = 0; i < 2; i++)
        {
            Vector2 start = rushPoints[i % 2];
            Vector2 end = rushPoints[(i + 1) % 2];

            transform.position = start;
            animator.Play("Idle");
            yield return new WaitForSeconds(0.5f);

            animator.Play("Rush");
            PlayRushSound();
            float tempGravity = rb.gravityScale;
            rb.gravityScale = 0f;
            bossCollider.isTrigger = true;

            if (rushEffect != null)
            {
                bool isGoingLeft = end.x < start.x;
                float xOffset = isGoingLeft ? 0.5f : -0.5f;
                Vector3 spawnPos = transform.position + new Vector3(xOffset, 0f, 0f);

                GameObject effect = Instantiate(rushEffect, spawnPos, Quaternion.identity);

                SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.flipX = !isGoingLeft;
                    sr.sortingOrder = 1;
                }

                Destroy(effect, 0.3f);
            }

            while (Vector2.Distance(transform.position, end) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, end, rushSpeed * Time.deltaTime);
                spriteRenderer.flipX = (end.x < transform.position.x);
                UpdateAttackColliderDirection();
                yield return null;
            }

            rb.gravityScale = tempGravity;
            bossCollider.isTrigger = false;

            animator.Play("Idle");
            yield return new WaitForSeconds(rushDelay);
        }

        animator.Play("Idle");
        isAttacking = false;
    }

    private System.Collections.IEnumerator AttackPattern_JumpToPlayerAttack()
    {
        isAttacking = true;

        int repeatCount = 3;
        float duration = isPhase2 ? 0.5f : 1.0f; // 점프 속도 빠르게
        float arcHeight = isPhase2 ? 7f : 5f; // 궤적 높이 증가
        float preDelay = isPhase2 ? 0.3f : 0.5f; // 준비 딜레이 줄임
        float postDelay = isPhase2 ? 1f : 2f; // 후 딜레이 줄임

        for (int i = 0; i < repeatCount; i++)
        {
            Vector3 start = transform.position;
            Vector3 target = player.position;
            float timer = 0f;

            spriteRenderer.flipX = (target.x < start.x);
            UpdateAttackColliderDirection();

            animator.Play("Jump");
            PlayJumpSound();
            yield return new WaitForSeconds(preDelay);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);

                float newX = Mathf.Lerp(start.x, target.x, t);
                float heightOffset = Mathf.Sin(t * Mathf.PI) * arcHeight;
                float newY = Mathf.Lerp(start.y, target.y, t) + heightOffset;

                transform.position = new Vector2(newX, newY);

                yield return null;
            }
            if (landingEffect != null)
            {
                Vector3 spawnPos = transform.position + new Vector3(0f, 0.7f, 1f);
                GameObject effect = Instantiate(landingEffect, spawnPos, Quaternion.identity);
                Destroy(effect, 0.3f);
            }
            PlayLandingSound();

            animator.Play("JumpAttack");
            PlayJumpAttackSound();
            yield return new WaitForSeconds(0.8f);

            attackObject.SetActive(true);
            //attackCollider.enabled = true;

            if (isPhase2 && earthSpikePrefab != null)
            {
                StartCoroutine(SpawnEarthSpikesWave());
            }

            yield return new WaitForSeconds(0.5f);

            attackObject.SetActive(false);
            //attackCollider.enabled = false;
            yield return new WaitForSeconds(0.2f);
            animator.Play("Idle");

            yield return new WaitForSeconds(postDelay);
        }
        isAttacking = false;
    }

    private System.Collections.IEnumerator SpawnEarthSpikesWave()
    {
        if (GroundPoint == null)
        {
            Debug.LogWarning("FeetPoint가 할당되지 않았습니다. Boss 중심에서 스폰합니다.");
        }

        Vector3 basePos = GroundPoint.position;

        for (int i = 0; i <= spikeCountPerSide; i++)
        {
            Vector3 rightPos = basePos + Vector3.right * spikeInterval * i;
            Vector3 leftPos = basePos + Vector3.left * spikeInterval * i;

            PlaySpikeSound();

            Instantiate(earthSpikePrefab, rightPos, Quaternion.identity);
            if (i != 0) Instantiate(earthSpikePrefab, leftPos, Quaternion.identity);

            yield return new WaitForSeconds(spikeSpawnDelay);
        }
    }

    public void OnBossDie()
    {
        StopAllCoroutines();
        isAttacking = false;
        PlayDeathSound();
        if (phase2EffectObject != null)
            phase2EffectObject.SetActive(false);

        if (bossHealthUI != null)
            StartCoroutine(HideBossUIAfterDelay(1.5f));
        StartCoroutine(HandleBossDeathSequence());
    }

    private IEnumerator HandleBossDeathSequence()
    {
        yield return new WaitForSeconds(2f); // 애니메이션 후 대기

        SceneTransitionManager.Instance?.TransitionTo("4 Fin");
    }

    private void UpdateAttackColliderDirection()
    {
        if (attackObject == null) return;

        Vector3 localPos = attackObject.transform.localPosition; //attackCollider
        float x = Mathf.Abs(localPos.x);
        attackObject.transform.localPosition = new Vector3(spriteRenderer.flipX ? -x : x, localPos.y, localPos.z);
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
            }
        }
    }*/

    private IEnumerator HideBossUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bossHealthUI != null)
        {
            bossHealthUI.SetActive(false);
        }
    }

    private void PlayLandingSound()
    {
        if (landingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(landingSound, 0.3f);
        }
    }

    private void PlayJumpSound()
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound, 0.1f);
        }
    }

    private void PlayJumpAttackSound()
    {
        if (jumpAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpAttackSound, 0.1f);
        }
    }

    private void PlayTeleportSound()
    {
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound, 0.3f);
        }
    }

    private void PlayBehindAttackSound()
    {
        if (behindAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(behindAttackSound, 0.3f);
        }
    }

    private void PlayRushSound()
    {
        if (rushSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(rushSound, 0.3f);
        }
    }

    private void PlayEnergyBallStartSound()
    {
        if (energyBallStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(energyBallStartSound, 0.3f);
        }
    }

    private void PlayEnergyBallSpawnSound()
    {
        if (energyBallSpawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(energyBallSpawnSound, 0.1f);
        }
    }

    private void PlayPhase2Sound()
    {
        if (phase2Sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(phase2Sound, 0.3f);
        }
    }

    private void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound, 0.3f);
        }
    }
    private void PlaySpikeSound()
    {
        if (spikeSpawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spikeSpawnSound, 0.1f);
        }
    }
}