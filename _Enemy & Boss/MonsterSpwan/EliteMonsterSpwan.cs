using UnityEngine;
using System.Collections.Generic;

public class EliteMonsterSpwan : MonoBehaviour
{
    public GameObject[] monsterPrefabs;
    public GameObject eliteBossPrefab;

    public Transform eliteBossSpawnPoint;
    public Transform[] spawnPoints;
    public int spawnCount = 6;

    public GameObject portalToNextScene; // ✅ 씬에 있는 포털 GameObject 참조

    public void SpawnEliteAndMonsters()
    {
        Debug.Log("SpawnEliteAndMonsters() 호출됨");

        if (eliteBossPrefab != null && eliteBossSpawnPoint != null)
        {
            Debug.Log("엘리트 보스 생성 시작");
            GameObject eliteBossInstance = Instantiate(eliteBossPrefab, eliteBossSpawnPoint.position, Quaternion.identity);

            EliteMonsterController eliteController = eliteBossInstance.GetComponent<EliteMonsterController>();
            if (eliteController != null)
            {
                eliteController.portalToNextScene = this.portalToNextScene;
            }
            else
            {
                Debug.LogWarning("생성된 엘리트 보스 Prefab에 EliteMonsterController가 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("보스 프리팹 또는 스폰 지점이 설정되지 않았습니다.");
        }

        foreach (Transform spawnPoint in spawnPoints)
        {
            Vector3[] offsets = {
                new Vector3(-4f, 0, 0),
                new Vector3(4f, 0, 0),
                new Vector3(6f, 0, 0),
                new Vector3(-6f, 0, 0),
                new Vector3(8f, 0, 0),
                new Vector3(-8f, 0, 0)};

            for (int i = 0; i < spawnCount; i++)
            {
                int rand = Random.Range(0, monsterPrefabs.Length);
                Vector3 spawnPos = spawnPoint.position + offsets[i % offsets.Length];

                GameObject monster = Instantiate(monsterPrefabs[rand], spawnPos, Quaternion.identity);
                FindObjectOfType<MonsterCollisionManager>()?.ApplyIgnoreCollision(monster);
            }
        }
    }
}