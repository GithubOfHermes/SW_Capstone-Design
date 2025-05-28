using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorParticleTrigger : MonoBehaviour
{
    private static bool globalHandling = false;

    private Direction direction;
    private Vector2Int roomOffset;
    private bool isHandling = false;

    private void Awake()
    {
        globalHandling = false;
        float rotZ = Mathf.Round(transform.eulerAngles.z) % 360f;

        direction = rotZ switch
        {
            180f => Direction.Left,
            90f => Direction.Down,
            0f => Direction.Right,
            270f => Direction.Up,
            _ => Direction.Up // fallback
        };

        roomOffset = direction.ToOffset();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isHandling || globalHandling || !other.CompareTag("Player")) return;
        StartCoroutine(HandleDoor());
    }

    private IEnumerator HandleDoor()
    {
        isHandling = true;
        globalHandling = true;
        yield return null;
        PlayerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        if (player != null)
        {
            StartCoroutine(player.SceneTransitionInvincibility(2.5f)); // ← 무적 2.5초
        }

        yield return FadeController.Instance.FadeOut();
        RoomSpawner.Instance.MovePlayerToNextRoom(roomOffset, direction);
        yield return FadeController.Instance.FadeIn();

        yield return new WaitForSeconds(0.01f);
        isHandling = false;
        globalHandling = false;
    }

    public void LockDoor()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().color = Color.red;
        GetComponent<Animator>().enabled = false;
    }

    public void UnlockDoor()
    {
        GetComponent<Collider2D>().enabled = true;
        GetComponent<SpriteRenderer>().color = Color.white;
        GetComponent<Animator>().enabled = true;
    }

}
