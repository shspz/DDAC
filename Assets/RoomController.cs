using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject doorUp;
    public GameObject doorDown;
    public GameObject doorLeft;
    public GameObject doorRight;

    // ���� ���¸� �����ϴ� �޼���
    public void SetDoorStates(bool up, bool down, bool left, bool right)
    {
        doorUp.SetActive(up);
        doorDown.SetActive(down);
        doorLeft.SetActive(left);
        doorRight.SetActive(right);
    }
}
