using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActivationManager : MonoBehaviour
{
    private List<EnemyRoomTracker> allEnemies = new List<EnemyRoomTracker>(); // 모든 적의 참조를 저장하는 리스트
    public LevelGeneration levelGeneration; // LevelGeneration 스크립트 참조
    public GameObject doorContainer; // 문들을 포함하는 부모 오브젝트 참조

    public static EnemyActivationManager Instance;  // 싱글톤 인스턴스

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        levelGeneration = FindObjectOfType<LevelGeneration>();
        allEnemies.AddRange(FindObjectsOfType<EnemyRoomTracker>());
    }

    void Update()
    {
        ActivateEnemiesInCurrentRoom();
    }

    void ActivateEnemiesInCurrentRoom()
    {
        Vector2 currentRoomPosition = levelGeneration.currentRoomPosition;
        int activeEnemiesCount = 0; // 활성화된 적의 수를 카운트합니다.

        for (int i = allEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = allEnemies[i];
            if (enemy == null)
            {
                allEnemies.RemoveAt(i);
                continue;
            }

            if (enemy.roomPosition == currentRoomPosition)
            {
                enemy.gameObject.SetActive(true);
                activeEnemiesCount++; // 활성화된 적을 카운트
            }
            else
            {
                enemy.gameObject.SetActive(false);
            }
        }

        // 적이 한 명 이상 활성화되면 문을 비활성화
        if (activeEnemiesCount > 0)
        {
            doorContainer.SetActive(false);
        }
        else
        {
            doorContainer.SetActive(true);
        }
    }

    public void RemoveEnemyFromList(EnemyRoomTracker enemy)
    {
        allEnemies.Remove(enemy);
    }
}
