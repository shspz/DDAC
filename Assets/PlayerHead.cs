using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHead : MonoBehaviour
{
    public SpriteRenderer headRenderer; // 플레이어 머리 그래픽의 SpriteRenderer 컴포넌트
    public Sprite[] directionSprites; // 방향에 따른 스프라이트 배열
    public Transform defaultHeadPosition; // 머리의 기본 위치
    public PlayerMovement playerMovement; // 플레이어 이동 스크립트 참조

    private Camera mainCamera; // 메인 카메라
    private Vector3 targetHeadPosition; // 머리의 목표 위치

    void Start()
    {
        mainCamera = Camera.main; // 메인 카메라 초기화
        targetHeadPosition = defaultHeadPosition.position; // 초기 목표 위치 설정
    }

    void Update()
    {
        Vector3 mousePosition = GetMousePosition();
        Vector3 direction = (mousePosition - transform.position).normalized;
        UpdateHeadSprite(direction);
        MoveHeadBasedOnPlayerVelocity();
    }

    Vector3 GetMousePosition()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z - mainCamera.transform.position.z));
        return new Vector3(mousePos.x, mousePos.y, 0);
    }

    void UpdateHeadSprite(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int directionIndex = (int)((angle + 360 + 22.5) % 360 / 45); // 45도 간격으로 8 방향 계산
        headRenderer.sprite = directionSprites[directionIndex];
    }

    void MoveHeadBasedOnPlayerVelocity()
    {
        if (playerMovement != null)
        {
            // 이동 속도에서 수직 성분을 무시하고 수평 성분만 사용
            Vector2 velocity = playerMovement.GetCurrentVelocity();
            Vector3 forwardOffset = Vector3.right * velocity.x * 0.04f; // 오직 x 축 방향으로만 기울임

            targetHeadPosition = defaultHeadPosition.position + forwardOffset; // 목표 위치 업데이트
        }

        // 부드러운 이동 적용
        transform.position = Vector3.Lerp(transform.position, targetHeadPosition, Time.deltaTime * 5f);
    }


    
}
