using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class MonsterSpawner : MonoBehaviour
{
    public GameObject[] monsterPrefabs; // 총 6종 몬스터
    public Transform[] spawnPoints;     // 스폰 포인트들
    public int spawnCount = 5;          // 포인트당 스폰 수

    private List<MonoBehaviour> initialMonsters = new List<MonoBehaviour>();
    private bool hasSpawned = false;
    private bool isSpawning = false;

    private RoomBehavior room; // 이 스포너가 속한 방

    void Start()
    {
        // 자신이 속한 RoomBehavior 찾기
        room = FindRoomBehaviorInParentHierarchy();
        // 현재 방(R1)의 부모 기준으로 몬스터들만 추적
        Transform roomRoot = transform.parent;

        CloseRangeMonsterController[] closeMonsters = roomRoot.GetComponentsInChildren<CloseRangeMonsterController>();
        foreach (var m in closeMonsters)
        {
            if (IsInThisRoom(m.transform.position))
            {
                initialMonsters.Add(m);
                m.OnMonsterDie += OnMonsterDead;
                room?.RegisterMonster(m);
            }
        }

        LongRangeMonsterController[] longMonsters = roomRoot.GetComponentsInChildren<LongRangeMonsterController>();
        foreach (var m in longMonsters)
        {
            if (IsInThisRoom(m.transform.position))
            {
                initialMonsters.Add(m);
                m.OnMonsterDie += OnMonsterDead;
                room?.RegisterMonster(m);
            }
        }

        FlyingSuicideMonsterController[] suiciders = roomRoot.GetComponentsInChildren<FlyingSuicideMonsterController>();
        foreach (var m in suiciders)
        {
            if (IsInThisRoom(m.transform.position))
            {
                initialMonsters.Add(m);
                m.OnMonsterDie += OnMonsterDead;
                room?.RegisterMonster(m);
            }
        }

        FlyingCloseRangeMonsterController[] flyers = roomRoot.GetComponentsInChildren<FlyingCloseRangeMonsterController>();
        foreach (var m in flyers)
        {
            if (IsInThisRoom(m.transform.position))
            {
                initialMonsters.Add(m);
                m.OnMonsterDie += OnMonsterDead;
                room?.RegisterMonster(m);
            }
        }


        if (SceneManager.GetActiveScene().name == "2 Way")
        {
            EliteMonsterController[] elites = roomRoot.GetComponentsInChildren<EliteMonsterController>();
            foreach (var elite in elites)
            {
                initialMonsters.Add(elite);
                elite.OnMonsterDie += OnMonsterDead; // 아래에서 선언해줄 것
                room?.RegisterMonster(elite);
            }
        }

        if (room != null)
        {
            if (initialMonsters.Count == 0)
            {
                room.OpenDoors();
            }
        }
    }

    void OnMonsterDead(MonoBehaviour monster)
    {
        initialMonsters.Remove(monster);

        if (!hasSpawned && initialMonsters.Count == 0)
        {
            hasSpawned = true;
            isSpawning = true;
            Invoke(nameof(SpawnRandomMonsters), 1f);
        }

        if (hasSpawned && !isSpawning && initialMonsters.Count == 0)
        {
            room?.OpenDoors();
        }
    }


    void SpawnRandomMonsters()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            Vector3[] offsets = {
            new Vector3(-2f, 0, 0),
            new Vector3(2f, 0, 0),
            new Vector3(4, 0, 0),
            new Vector3(-4, 0, 0),
            Vector3.zero
        };

            for (int i = 0; i < spawnCount; i++)
            {
                int rand = Random.Range(0, monsterPrefabs.Length);
                Vector3 spawnPos = spawnPoint.position + offsets[i % offsets.Length];

                GameObject monster = Instantiate(monsterPrefabs[rand], spawnPos, Quaternion.identity);
                FindObjectOfType<MonsterCollisionManager>()?.ApplyIgnoreCollision(monster);

                MonoBehaviour controller =
    monster.GetComponent<CloseRangeMonsterController>() as MonoBehaviour ??
    monster.GetComponent<LongRangeMonsterController>() as MonoBehaviour ??
    monster.GetComponent<FlyingSuicideMonsterController>() as MonoBehaviour ??
    monster.GetComponent<FlyingCloseRangeMonsterController>() as MonoBehaviour;

                if (controller != null)
                {
                    initialMonsters.Add(controller);

                    if (controller is CloseRangeMonsterController close)
                        close.OnMonsterDie += OnMonsterDead;
                    else if (controller is LongRangeMonsterController ranged)
                        ranged.OnMonsterDie += OnMonsterDead;
                    else if (controller is FlyingSuicideMonsterController suicide)
                        suicide.OnMonsterDie += OnMonsterDead;
                    else if (controller is FlyingCloseRangeMonsterController flyClose)
                        flyClose.OnMonsterDie += OnMonsterDead;

                    room?.RegisterMonster(controller);
                }
            }
        }

        isSpawning = false; // ✅ 리스폰 완료
    }


    private bool IsInThisRoom(Vector3 pos)
    {
        if (room == null) return false;

        Vector3 roomCenter = room.transform.position + new Vector3(0f, 12f, 0f); // ← Y축 보정 추가
        float roomWidth = 46f;
        float roomHeight = 30f;

        bool inX = Mathf.Abs(pos.x - roomCenter.x) <= roomWidth / 2f;
        bool inY = Mathf.Abs(pos.y - roomCenter.y) <= roomHeight / 2f;

        if (!inX || !inY)
        {
            Debug.LogWarning($"[IsInThisRoom] 몬스터 위치 {pos} → 보정된 RoomCenter: {roomCenter}, inX: {inX}, inY: {inY}");
        }

        return inX && inY;
    }


    private RoomBehavior FindRoomBehaviorInParentHierarchy()
    {
        Transform current = transform;

        while (current != null)
        {
            RoomBehavior rb = current.GetComponent<RoomBehavior>();
            if (rb != null)
            {
                return rb;
            }
            current = current.parent;
        }

        return null;
    }
}