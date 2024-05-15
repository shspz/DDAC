using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Tooltip("방을 표시할 프리팹입니다.")]
    public GameObject roomPrefab;
    
    [Tooltip("미니맵을 담을 컨테이너입니다.")]
    public GameObject miniMapContainer;

    [Tooltip("맵의 너비입니다.")]
    public int width = 10;
    
    [Tooltip("맵의 높이입니다.")]
    public int height = 10;
    
    private bool[,] map; // 맵 데이터

    [Tooltip("생성할 방의 개수입니다.")]
    public int numberOfRooms = 20; // 방 생성 개수를 public 변수로 추가

    void Start()
    {
        GenerateMap();
        PrintMap(); // 맵을 생성한 후 맵을 출력
        CreateMiniMap();
    }

    void GenerateMap()
    {
        map = new bool[width, height];
        int x = width / 2;
        int y = height / 2;
        int lastDirection = -1; // 마지막 방향을 저장하는 변수, 초기값은 -1

        for (int i = 0; i < numberOfRooms; i++) // numberOfRooms 개수만큼 방을 생성
        {
            map[x, y] = true;

            int direction = GetRandomDirection(lastDirection); // 변경된 방향 선택 로직

            switch (direction)
            {
                case 0: if (y < height - 1) y++; break; // 위로 이동
                case 1: if (y > 0) y--; break;          // 아래로 이동
                case 2: if (x > 0) x--; break;          // 왼쪽으로 이동
                case 3: if (x < width - 1) x++; break;  // 오른쪽으로 이동
            }
            lastDirection = direction; // 마지막 방향 업데이트
        }
    }

    int GetRandomDirection(int lastDirection)
    {
        int[] weights = { 10, 10, 10, 10 }; // 모든 방향에 초기 가중치 부여
        if (lastDirection != -1)
        {
            weights[lastDirection] = 3; // 최근 방향의 가중치를 줄여 다른 방향으로 갈 확률을 높임
        }

        int totalWeight = weights[0] + weights[1] + weights[2] + weights[3];
        int randomPoint = Random.Range(0, totalWeight);

        for (int i = 0; i < 4; i++)
        {
            if (randomPoint < weights[i])
                return i;
            randomPoint -= weights[i];
        }
        return 0;
    }

    void PrintMap()
    {
        string mapString = "";
        for (int j = 0; j < height; j++) // 높이 우선 순회
        {
            for (int i = 0; i < width; i++) // 너비 순회
            {
                // 현재 위치에 방이 있으면 '1', 없으면 '0'을 추가
                mapString += (map[i, j] ? "1" : "0") + " ";
            }
            mapString += "\n"; // 각 행이 끝나면 줄바꿈 추가
        }
        Debug.Log(mapString); // 완성된 맵 문자열을 로그로 출력
    }

    void CreateMiniMap()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[i, j])
                {
                    GameObject room = Instantiate(roomPrefab, new Vector3(i, j, 0), Quaternion.identity);
                    room.transform.parent = miniMapContainer.transform;
                    room.transform.localPosition = new Vector3(i, j, 0);
                    room.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
    }
}
