using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadMovement : MonoBehaviour
{
    public Transform playerBody; // 플레이어 몸체의 Transform
    public float maxHeadMoveDistance = 0.5f; // 머리가 이동할 수 있는 최대 거리
    private Vector3 originalPosition; // 원래 머리 위치

    private void Start()
    {
        originalPosition = transform.localPosition; // 초기 위치 저장
    }

    private void Update()
    {
        MoveHeadTowardsMouse();
    }

    void MoveHeadTowardsMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z; // Z좌표는 머리의 Z좌표와 동일하게 설정

        // 플레이어 몸체에서 마우스 위치까지의 벡터 계산
        Vector3 direction = mousePosition - playerBody.position;
        direction.z = 0; // 2D 게임이므로 Z는 무시

        // 마우스 위치가 너무 가까우면 이동하지 않음
        if (direction.magnitude > 0.1f)
        {
            // 최대 거리 제한을 적용하여 마우스 방향으로 머리를 조금 이동시킴
            Vector3 targetPosition = originalPosition + direction.normalized * Mathf.Min(maxHeadMoveDistance, direction.magnitude);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 5);
        }
        else
        {
            // 마우스 위치가 가까우면 원래 위치로 복귀
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 5);
        }
    }
}
