using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target; // 플레이어의 Transform
    [SerializeField] private float smoothSpeed = 0.125f; // 카메라 이동 부드러움 정도
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // 카메라와 플레이어 사이의 거리

    private void Start()
    {
        // 시작할 때 Player 태그를 가진 오브젝트를 찾아서 target으로 설정
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Player tagged object not found!");
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;
        
        // 부드러운 이동을 위한 Lerp 사용
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // 카메라 위치 업데이트
        transform.position = smoothedPosition;
    }
}
