using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject doorUp;
    public GameObject doorDown;
    public GameObject doorLeft;
    public GameObject doorRight;

    // 문의 상태를 설정하는 메서드
    public void SetDoorStates(bool up, bool down, bool left, bool right)
    {
        doorUp.SetActive(up);
        doorDown.SetActive(down);
        doorLeft.SetActive(left);
        doorRight.SetActive(right);
    }
}
