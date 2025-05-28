using UnityEngine;

public class CloseDoor : MonoBehaviour
{
    [Header("닫힐 타일맵 오브젝트")]
    public GameObject closeTilemap;

    [Header("카메라 전환")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject eliteCamera;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!activated && other.CompareTag("Player"))
        {
            if (closeTilemap != null)
            {
                closeTilemap.SetActive(true);
                activated = true;

                SwitchToCamera();
            }
            else
            {
                Debug.LogWarning("Close Tilemap 오브젝트가 설정되지 않았습니다.");
            }
        }
    }

    private void SwitchToCamera()
    {
        if (mainCamera != null)
            mainCamera.SetActive(false);

        if (eliteCamera != null)
            eliteCamera.SetActive(true);
        else
            Debug.LogWarning("Elite Camera 오브젝트가 설정되지 않았습니다.");
    }
}
