using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ScenePortalTrigger : MonoBehaviour
{
    [Header("씬 전환 조건")]
    [Tooltip("플레이어가 W 키를 눌러야 씬이 전환됩니다.")]
    public bool requireKeyPress = false;

    private bool playerInTrigger = false;

    private void Update()
    {
        if (requireKeyPress && playerInTrigger && Input.GetKeyDown(KeyCode.W))
        {
            TryTransition();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (requireKeyPress)
        {
            playerInTrigger = true;
        }
        else
        {
            TryTransition();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (requireKeyPress && other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }

    private void TryTransition()
    {
        PlayerController player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (player != null)
        {
            PlayerStateService.savedHp = player.GetCurrentHP(); // 저장
        }

        PlayerController.pendingInvincibilityTime = 2.5f;
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadNextScene();
        }
    }

}
