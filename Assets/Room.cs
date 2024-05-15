using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public Vector2 gridPos; // 방의 그리드 위치
    public int type; // 방의 타입
    public bool doorTop, doorBot, doorLeft, doorRight; // 각 문의 존재 여부

    // Room 클래스의 생성자
    public Room(Vector2 _gridPos, int _type)
    {
        gridPos = _gridPos; // 그리드 위치 설정
        type = _type; // 방 타입 설정
        doorTop = doorBot = doorLeft = doorRight = false; // 문 초기화
    }
}
