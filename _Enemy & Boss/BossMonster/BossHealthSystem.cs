using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class BossHealthSystem : MonoBehaviour
{
    [Header("보스 체력 설정")]
    public float maxHealth = 300f;
    private float currentHealth;

    [Header("체력 UI 연결")]
    public RectTransform healthBarFill;

    private bool isDead = false;

    private Animator animator;
    private BossController bossController;
    private Collider2D bossCollider;
    private Rigidbody2D rb;

    private void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        bossController = GetComponent<BossController>();
        bossCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
        else if (currentHealth <= maxHealth * 0.4f)
        {
            if (bossController != null && !bossController.isPhase2)
            {
                bossController.EnterPhase2();
            }
        }
    }

    public void RestoreHealthOverTime(float duration)
    {
        StartCoroutine(SmoothRestoreHealth(duration));
    }

    private System.Collections.IEnumerator SmoothRestoreHealth(float duration)
    {
        float startHealth = currentHealth;
        float targetHealth = maxHealth;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            currentHealth = Mathf.Lerp(startHealth, targetHealth, timer / duration);
            UpdateHealthUI();
            yield return null;
        }

        currentHealth = targetHealth;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            Vector3 scale = healthBarFill.localScale;
            scale.x = currentHealth / maxHealth;
            healthBarFill.localScale = scale;
        }
    }
    public void AnimateHealthFill()
    {
        StartCoroutine(AnimateFillRoutine());
    }

    private IEnumerator AnimateFillRoutine()
    {
        currentHealth = 0f;
        float targetHealth = maxHealth;
        float duration = 1.5f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            currentHealth = Mathf.Lerp(0f, targetHealth, timer / duration);
            UpdateHealthUI();
            yield return null;
        }

        currentHealth = targetHealth;
        UpdateHealthUI();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
            animator.Play("Die");

        if (bossController != null)
            bossController.OnBossDie();

        if (rb != null)
            rb.simulated = false;

        if (bossCollider != null)
            bossCollider.enabled = false;
    }
}