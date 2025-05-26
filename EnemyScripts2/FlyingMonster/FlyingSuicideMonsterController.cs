using UnityEngine;

public class FlyingSuicideMonsterController : MonoBehaviour
{
    public enum State { Wander, Chase, Explode, Dead }
    public State currentState = State.Wander;

    [Header("Movement")]
    public float wanderRadius = 3f;
    public float wanderDelay = 1.5f;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Combat")]
    public float chaseRangeX = 5f;
    public float chaseRangeY = 5f;
    public float explodeRange = 0.5f;
    public int damage = 30;

    [Header("Detection")]
    public LayerMask obstacleLayers;

    private Vector2 wanderTarget;
    private Transform player;
    private Rigidbody2D rb;
    private Vector2 currentVelocity;
    private Animator animator;

    [Header("Sound")]
    public AudioClip explodeSound;

    private bool isExploding = false;
    private float flipCooldown = 0.5f;
    private float lastFlipTime = -999f;

    private Vector2 lockedTargetPosition;
    public System.Action<MonoBehaviour> OnMonsterDie;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0;
        player = GameObject.FindWithTag("Player").transform;

        IgnorePlatformCollisions();
        PickRandomTarget();
        InvokeRepeating(nameof(PickRandomTarget), wanderDelay, wanderDelay);
    }

    private void Update()
    {
        if (currentState == State.Dead || isExploding) return;

        float xDist = Mathf.Abs(transform.position.x - player.position.x);
        float yDist = Mathf.Abs(transform.position.y - player.position.y);
        float totalDist = Vector2.Distance(transform.position, player.position);

        CheckObstacleAndFlip();

        switch (currentState)
        {
            case State.Wander:
                MoveTo(wanderTarget, moveSpeed);
                FlipByDirection(wanderTarget - (Vector2)transform.position);
                animator.Play("Idle");

                if (xDist <= chaseRangeX && yDist <= chaseRangeY)
                    currentState = State.Chase;
                break;

            case State.Chase:
                Vector2 dirToPlayer = (player.position - transform.position).normalized;
                MoveTo(transform.position + (Vector3)dirToPlayer, chaseSpeed);
                FlipByDirection(dirToPlayer);
                animator.Play("Idle");

                if (totalDist <= 4f)
                {
                    currentState = State.Explode;
                    lockedTargetPosition = player.position;
                    animator.Play("Attack");
                }
                break;

            case State.Explode:
                Vector2 dir = (lockedTargetPosition - (Vector2)transform.position).normalized;
                rb.linearVelocity = dir * chaseSpeed * 1.5f;
                FlipByDirection(dir);

                if (!isExploding && Vector2.Distance(transform.position, lockedTargetPosition) < explodeRange)
                {
                    TriggerExplosion();
                }
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

    void PickRandomTarget()
    {
        if (currentState != State.Wander) return;
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = (Vector2)transform.position + randomOffset;
    }

    void MoveTo(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        Vector2 targetVelocity = dir * speed;
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, 0.2f);
    }

    void CheckObstacleAndFlip()
    {
        if (Time.time - lastFlipTime < flipCooldown) return;

        float checkDistance = 0.3f;
        Vector2 forward = rb.linearVelocity.normalized;
        if (forward == Vector2.zero) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, forward, checkDistance, obstacleLayers);
        if (hit.collider != null)
        {

            if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Trap"))
            {
                PickRandomTarget();
                lastFlipTime = Time.time;
            }
        }
    }

    void FlipByDirection(Vector2 direction)
    {
        if (direction.x > 0.01f)
            transform.localScale = new Vector3(2, 2, 1); // 오른쪽
        else if (direction.x < -0.01f)
            transform.localScale = new Vector3(-2, 2, 1); // 왼쪽
    }

    void TriggerExplosion()
    {
        isExploding = true;

        animator.Play("Die"); // 자폭 애니메이션 실행

        PlaySoundDetached(explodeSound, 0.4f);

        if (Vector2.Distance(transform.position, player.position) <= explodeRange)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.TakeDamage(damage);
        }

        Debug.Log("자폭 실행");
        currentState = State.Dead;
        OnMonsterDie?.Invoke(this);
        Destroy(gameObject, 0.5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Explode && !isExploding)
        {
            if (collision.collider.CompareTag("Player"))
            {
                TriggerExplosion(); // 플레이어와 충돌하면 즉시 자폭
            }
            else if (collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Wall"))
            {
                TriggerExplosion();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(chaseRangeX * 2, chaseRangeY * 2, 0));

        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 0.3f);
        }
    }

    void PlaySoundDetached(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        GameObject soundObj = new GameObject("TempSound");
        AudioSource tempAudio = soundObj.AddComponent<AudioSource>();
        tempAudio.clip = clip;
        tempAudio.volume = volume;
        tempAudio.Play();

        Destroy(soundObj, clip.length); // 소리 다 끝난 후 삭제
    }
}
