using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    public Vector2 worldSize = new Vector2(4,4); // 맵 생성 시 사이즈
    public Room[,] rooms; // 방의 2D배열
    public List<Vector2> takenPositions = new List<Vector2>(); 
    public int gridSizeX, gridSizeY, numberOfRooms = 20;
    public GameObject roomWhiteObj;
    public GameObject currentRoomMarker; // 현재 방 표시 오브젝트
    public Vector2 currentRoomPosition; // 현재 방 좌표
    public GameObject realRoomPrefab; // 실제 방 프리팹
    public GameObject player; // 플레이어 오브젝트
    public GameObject playerHand; // 플레이어의 손 오브젝트
    public Camera mainCamera; // 메인 카메라
    public GameObject doorContainer; // 문을 포함하는 부모 오브젝트

    public GameObject doorTop, doorBottom, doorLeft, doorRight; // 각 방향의 문 오브젝트

    // 플레이어가 어느 방향으로 이동했는지 추적하는 변수
    private Vector2 lastRoomMoveDirection = Vector2.zero;

    public float cameraMoveSpeed = 5f; // 카메라 이동 속도 조절 변수
    private Vector3 targetCameraPosition; // 카메라의 목표 위치

    public GameObject enemyTypeA;
    public GameObject enemyTypeB;
    public GameObject enemyTypeC;
    public int maxRoomCost = 6;
    



    void Start() {
        if (numberOfRooms >= (worldSize.x * 2) * (worldSize.y * 2)){
            numberOfRooms = Mathf.RoundToInt((worldSize.x * 2) * (worldSize.y * 2));
        }
        gridSizeX = Mathf.RoundToInt(worldSize.x);
        gridSizeY = Mathf.RoundToInt(worldSize.y);
        CreateRooms();
        SetRoomDoors();
        SetExitRoom();
        DrawMap();
        // 시작 지점의 좌표를 현재 방 좌표로 설정
        currentRoomPosition = new Vector2(gridSizeX, gridSizeY);
        // 방 생성 완료. 이 밑 함수는 방 생성 이후에 실행되어야 하는 함수들이다.
        // 현재 방 표시 오브젝트를 현재 방 위치로 이동
        MoveCurrentRoomMarker();
        PositionPlayerAndCamera(); // 플레이어와 카메라 위치 설정 메서드 호출
        MoveDoorsToCurrentRoom(); // 문 위치 이동 함수 호출
        PositionPlayerAndCamera(); // 초기 플레이어와 카메라 위치 설정
    }

    void Update() {
        if (Vector3.Distance(mainCamera.transform.position, targetCameraPosition) > 0.01f) {
            // Vector3.Lerp를 사용하여 카메라 위치를 점진적으로 이동
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCameraPosition, Time.deltaTime * cameraMoveSpeed);
        }
    }

    public Vector2 GetCurrentRoomWorldPosition()
    {
        // Assuming each room is spaced by 20 units in x and 12 units in y
        Vector2 gridPosition = rooms[(int)currentRoomPosition.x, (int)currentRoomPosition.y].gridPos;
        return new Vector2(gridPosition.x * 20, gridPosition.y * 12);
    }

    public void MoveToRoom(Vector2 direction)
    {
        lastRoomMoveDirection = direction;
        currentRoomPosition += direction;
        MoveCurrentRoomMarker();
        PositionPlayerAndCamera();
        MoveDoorsToCurrentRoom();
    }

    void MoveDoorsToCurrentRoom()
    {
        Vector2 roomPos = rooms[(int)currentRoomPosition.x, (int)currentRoomPosition.y].gridPos;
        Vector3 newPosition = new Vector3(roomPos.x * 20, roomPos.y * 12, 0); // Z 좌표 유지
        doorContainer.transform.position = newPosition;
    }

    void MoveCurrentRoomMarker() {
        if (currentRoomMarker) {
            Vector2 roomPos = rooms[(int)currentRoomPosition.x, (int)currentRoomPosition.y].gridPos;
            // 각 방의 좌표를 실제 Unity 좌표로 변환
            Vector3 newPosition = new Vector3(roomPos.x * 1.6f + 30, roomPos.y * 0.8f, -10);
            currentRoomMarker.transform.position = newPosition;
            UpdateDoorsVisibility(currentRoomPosition);
        }
    }

    void UpdateDoorsVisibility(Vector2 roomPos)
    {
        int roomPosX = (int)roomPos.x;
        int roomPosY = (int)roomPos.y;

        doorTop.SetActive(roomPosY < gridSizeY * 2 - 1 && rooms[roomPosX, roomPosY + 1] != null);
        doorBottom.SetActive(roomPosY > 0 && rooms[roomPosX, roomPosY - 1] != null);
        doorLeft.SetActive(roomPosX > 0 && rooms[roomPosX - 1, roomPosY] != null);
        doorRight.SetActive(roomPosX < gridSizeX * 2 - 1 && rooms[roomPosX + 1, roomPosY] != null);
    }

    void PositionPlayerAndCamera() 
    {
        Vector2 roomPos = rooms[(int)currentRoomPosition.x, (int)currentRoomPosition.y].gridPos;
        Vector3 basePosition = new Vector3(roomPos.x * 20, roomPos.y * 12, 0);

        // 카메라 목표 위치 설정
        targetCameraPosition = new Vector3(basePosition.x, basePosition.y, -10);

        // 플레이어와 플레이어의 손 위치 조정
        Vector3 playerPosition = basePosition; // 플레이어 기본 위치

        // 방 이동 방향에 따라 플레이어 위치 조정
        if (lastRoomMoveDirection == Vector2.up) 
        {
            playerPosition.y -= 4; // 위로 이동 시 Y 좌표 강도 조정
        }
        else if (lastRoomMoveDirection == Vector2.down)
        {
            playerPosition.y += 4; // 아래로 이동 시 Y 좌표 강도 조정
        }
        else if (lastRoomMoveDirection == Vector2.left)
        {
            playerPosition.x += 8; // 왼쪽으로 이동 시 X 좌표 강도 조정
        }
        else if (lastRoomMoveDirection == Vector2.right)
        {
            playerPosition.x -= 8; // 오른쪽으로 이동 시 X 좌표 강도 조정
        }

        player.transform.position = new Vector3(playerPosition.x, playerPosition.y, -1); // 플레이어 Z축 위치 설정
        playerHand.transform.position = new Vector3(playerPosition.x, playerPosition.y, 0); // 플레이어 손 Z축 위치 설정
 
    }




    void CreateRooms() {
        rooms = new Room[gridSizeX * 2, gridSizeY * 2];
        Vector2 startRoomPos = new Vector2(gridSizeX, gridSizeY);
        rooms[(int)startRoomPos.x, (int)startRoomPos.y] = new Room(Vector2.zero, 1);
        takenPositions.Add(Vector2.zero);

        float randomCompareStart = 0.2f, randomCompareEnd = 0.01f;
        for (int i = 0; i < numberOfRooms - 1; i++) {
            float randomPerc = ((float) i) / (((float)numberOfRooms - 1));
            float randomCompare = Mathf.Lerp(randomCompareStart, randomCompareEnd, randomPerc);

            Vector2 checkPos = NewPosition();
            if (NumberOfNeighbors(checkPos, takenPositions) > 1 && Random.value > randomCompare) {
                int iterations = 0;
                do {
                    checkPos = SelectiveNewPosition();
                    iterations++;
                } while (NumberOfNeighbors(checkPos, takenPositions) > 1 && iterations < 100);
            }

            if (!takenPositions.Contains(checkPos)) {
                rooms[(int)checkPos.x + gridSizeX, (int)checkPos.y + gridSizeY] = new Room(checkPos, 0);
                takenPositions.Insert(0, checkPos);
                InstantiateRoomPrefab(checkPos); // 실제 방 프리팹 생성
            }
        }
    }

    void InstantiateRoomPrefab(Vector2 position)
    {
        Vector3 worldPosition = new Vector3(position.x * 20, position.y * 12, 1); // 방의 월드 좌표 계산
        GameObject newRoom = Instantiate(realRoomPrefab, worldPosition, Quaternion.identity);

        // 시작 지점과 도착 지점을 제외하고 적을 배치
        if (position != Vector2.zero && position != new Vector2(gridSizeX, gridSizeY))
        {
            PlaceEnemiesInRoom(position); // 여기서 position을 넘겨줘야 함
        }
    }




    void PlaceEnemiesInRoom(Vector2 roomGridPosition)
    {
        int currentCost = 0;
        List<GameObject> placedEnemies = new List<GameObject>();

        while (currentCost < maxRoomCost)
        {
            int enemyType = Random.Range(1, 4); // 1, 2, 3 중 선택 (A, B, C)
            GameObject enemyPrefab = null;
            int enemyCost = 0;

            switch (enemyType)
            {
                case 1:
                    enemyPrefab = enemyTypeA;
                    enemyCost = 1;
                    break;
                case 2:
                    enemyPrefab = enemyTypeB;
                    enemyCost = 3;
                    break;
                case 3:
                    enemyPrefab = enemyTypeC;
                    enemyCost = 2;
                    break;
            }

            if (currentCost + enemyCost > maxRoomCost) break;

            Vector3 spawnPosition = new Vector3(Random.Range(-8f, 8f), Random.Range(-4f, 4f), 0);
            spawnPosition += new Vector3(roomGridPosition.x * 20, roomGridPosition.y * 12, 0); // Room world coordinates

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            placedEnemies.Add(newEnemy);

            // Update the EnemyRoomTracker's room position
            EnemyRoomTracker tracker = newEnemy.GetComponent<EnemyRoomTracker>();
            if (tracker != null)
            {
                // 적의 위치 설정 시 중심을 (4, 4)로 변환
                tracker.roomPosition = roomGridPosition + new Vector2(gridSizeX, gridSizeY);
            }

            // Enforce maximum count constraints
            if (enemyType == 1 && CountEnemiesOfType(placedEnemies, enemyTypeA) > 4) break;
            if (enemyType == 2 && CountEnemiesOfType(placedEnemies, enemyTypeB) > 1) break;
            if (enemyType == 3 && CountEnemiesOfType(placedEnemies, enemyTypeC) > 2) break;

            currentCost += enemyCost;
        }
    }








    int CountEnemiesOfType(List<GameObject> enemies, GameObject type)
    {
        int count = 0;
        foreach (var enemy in enemies)
        {
            if (enemy == type)
            {
                count++;
            }
        }
        return count;
    }

    void SetExitRoom() {
        List<Vector2> potentialExits = new List<Vector2>();
        for (int x = 0; x < gridSizeX * 2; x++) {
            for (int y = 0; y < gridSizeY * 2; y++) {
                if (rooms[x, y] != null && (x != gridSizeX || y != gridSizeY)) { // 시작 지점을 제외
                    int neighborCount = NumberOfNeighbors(new Vector2(x - gridSizeX, y - gridSizeY), takenPositions);
                    if (neighborCount == 1) { // 문이 하나인 방
                        potentialExits.Add(new Vector2(x, y));
                    }
                }
            }
        }
        if (potentialExits.Count > 0) {
            Vector2 exitPos = potentialExits[Random.Range(0, potentialExits.Count)];
            rooms[(int)exitPos.x, (int)exitPos.y].type = 2; // 타입 2를 도착 지점으로 설정
        }
    }


    Vector2 NewPosition(){
        int x = 0, y = 0;
        Vector2 checkingPos = Vector2.zero;
        do{
            int index = Mathf.RoundToInt(Random.value * (takenPositions.Count -1));
            x = (int) takenPositions[index].x;
            y = (int) takenPositions[index].y;
            bool UpDown = Random.value < 0.5f;
            bool positive = Random.value < 0.5f;
            if (UpDown){
                if (positive){
                    y += 1;
                }else {
                    y -= 1;
                }
            }else{
                if (positive){
                    x += 1;
                }else{
                    x -= 1;
                }
            }
            checkingPos = new Vector2(x,y);
        }while (takenPositions.Contains(checkingPos) || x >= gridSizeX || x< -gridSizeX || y >= gridSizeY || y < -gridSizeY);
        return checkingPos;
    }

    Vector2 SelectiveNewPosition(){
        int index = 0, inc = 0;
        int x = 0, y = 0;
        Vector2 checkingPos = Vector2.zero;
        do{
            inc = 0;
            do{
                index = Mathf.RoundToInt(Random.value * (takenPositions.Count - 1));
                inc ++;
            }while (NumberOfNeighbors(takenPositions[index], takenPositions) > 1 && inc < 100);
            x = (int) takenPositions[index].x;
            y = (int) takenPositions[index].y;
            bool UpDown = (Random.value < 0.5f);
            bool positive = (Random.value < 0.5F);
            if (UpDown){
                if (positive){
                    y += 1;
                }else {
                    y -= 1;
                }
            }else{
                if (positive){
                    x += 1;
                }else{
                    x -= 1;
                }
            }
            checkingPos = new Vector2(x,y);
        }while (takenPositions.Contains(checkingPos) || x >= gridSizeX || x< -gridSizeX || y >= gridSizeY || y < -gridSizeY);
        if (inc >= 100){
            print("Error: could not find position with only one neighbor");
        }
        return checkingPos;
    }

    int NumberOfNeighbors(Vector2 checkingPos, List<Vector2> usedPositions){
        int ret = 0;
        if (usedPositions.Contains(checkingPos + Vector2.right)) {
            ret++;
        }
        if (usedPositions.Contains(checkingPos + Vector2.left)) {
            ret++;
        }
        if (usedPositions.Contains(checkingPos + Vector2.up)) {
            ret++;
        }
        if (usedPositions.Contains(checkingPos + Vector2.down)) {
            ret++;
        }
        return ret;
    }

    void DrawMap(){
        foreach (Room room in rooms){
            if (room == null){
                continue;
            }
            Vector2 drawPos = room.gridPos;
            drawPos.x *= 1.6f;
            drawPos.x += 30;
            drawPos.y *= 0.8f;
            MapSpriteSelector mapper = Object.Instantiate(roomWhiteObj, drawPos, Quaternion.identity).GetComponent<MapSpriteSelector>();
            mapper.type = room.type;
            mapper.up = room.doorTop;
            mapper.down = room.doorBot;
            mapper.left = room.doorLeft;
            mapper.right = room.doorRight;
        }
    }

    void SetRoomDoors(){
        for (int x = 0; x< ((gridSizeX * 2)); x++){
            for (int y = 0; y < ((gridSizeY * 2)); y++){
                if (rooms[x,y] == null){
                    continue;
                }
                Vector2 gridPosition = new Vector2(x,y);
                if (y - 1 < 0){
                    rooms[x,y].doorBot = false;
                }else{
                    rooms[x,y].doorBot = (rooms[x,y-1] != null);
                }
                if (y + 1 >= gridSizeY * 2){
                    rooms[x,y].doorTop = false;
                }else{
                    rooms[x,y].doorTop = (rooms[x,y+1] != null);
                }
                if (x - 1< 0){
                    rooms[x,y].doorLeft = false;
                }else{
                    rooms[x,y].doorLeft = (rooms[x-1,y] != null);
                }
                if (x + 1 >= gridSizeX * 2){
                    rooms[x,y].doorRight = false;
                }else{
                    rooms[x,y].doorRight = (rooms[x+1,y] != null);
                }
            }
        }
    }

}