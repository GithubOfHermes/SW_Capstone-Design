using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    [Header("상자 프리팹 및 드롭 설정")]
    [SerializeField] private GameObject openChestPrefab;
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    private bool playerInRange = false;
    private bool isOpened = false;

    void Update()
    {
        if (playerInRange && !isOpened && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        isOpened = true;

        // 열린 상자 생성
        Vector3 chestPos = transform.position + new Vector3(0f, 0.107f, 0f);
        GameObject openChest = Instantiate(openChestPrefab, chestPos, transform.rotation);
        openChest.transform.localScale = new Vector3(2.8f, 2.8f, 1f);

        // 아이템 드롭
        foreach (GameObject prefab in itemPrefabs)
        {
            ItemDropper.Drop(prefab, spawnPoint.position);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}