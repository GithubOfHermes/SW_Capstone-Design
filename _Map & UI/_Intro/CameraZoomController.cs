using UnityEngine;
using System.Collections;

public class CameraZoomController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomInSize = 3f;
    [SerializeField] private float zoomOutSize = 5f;
    [SerializeField] private float zoomSpeed = 1f;

    private void Awake()
    {
        if (mainCamera != null)
        {
            // ✅ 시작 시 줌인된 상태로 세팅
            mainCamera.orthographicSize = zoomInSize;
        }
    }

    // ✅ 줌인된 상태에서 줌 아웃으로만 전환
    public IEnumerator ZoomOut()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera가 설정되지 않았습니다.");
            yield break;
        }

        yield return Zoom(zoomInSize, zoomOutSize);
    }

    private IEnumerator Zoom(float from, float to)
    {
        float t = 0f;
        float duration = Mathf.Abs(to - from) / zoomSpeed;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duration);
            mainCamera.orthographicSize = Mathf.Lerp(from, to, progress);
            yield return null;
        }

        mainCamera.orthographicSize = to;
    }
}
