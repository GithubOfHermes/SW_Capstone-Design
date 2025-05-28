using UnityEngine;
using System.Collections;

public class PlatformSwitch : MonoBehaviour
{
    [Header("플랫폼 Tilemap들")]
    public GameObject[] tilemapPlatforms; // 9개의 Tilemap 오브젝트

    [Header("시간 설정")]
    public float activeDuration = 2f;     // 각 플랫폼이 켜지는 시간
    public float overlapDuration = 0.5f;  // 두 플랫폼 그룹이 겹치는 시간

    private void Start()
    {
        StartCoroutine(AlternatePlatformCycle());
    }

    IEnumerator AlternatePlatformCycle()
    {
        while (true)
        {
            // 홀수 플랫폼만 활성화
            SetPlatformsActive(odd: true);
            SetPlatformsActive(odd: false, active: false);
            yield return new WaitForSeconds(activeDuration);

            // 홀수와 짝수 모두 활성화
            SetPlatformsActive(odd: false, active: true); // 짝수 활성화
            yield return new WaitForSeconds(overlapDuration);

            // 짝수만 활성화, 홀수 비활성화
            SetPlatformsActive(odd: true, active: false);
            yield return new WaitForSeconds(activeDuration);

            // 짝수와 홀수 모두 활성화
            SetPlatformsActive(odd: true, active: true);
            yield return new WaitForSeconds(overlapDuration);
        }
    }

    void SetPlatformsActive(bool odd, bool active = true)
    {
        for (int i = 0; i < tilemapPlatforms.Length; i++)
        {
            if ((i % 2 != 0) == odd)
            {
                tilemapPlatforms[i].SetActive(active);
            }
        }
    }
}
