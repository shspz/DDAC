using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyTilt : MonoBehaviour
{
    public PlayerMovement playerMovement;  // PlayerMovement 스크립트 참조
    public Transform bodyGraphic;  // 플레이어 몸통 그래픽의 Transform
    public float maxTiltAngle = 15f;  // 최대 기울기 각도

    private void Update()
    {
        if (playerMovement != null && bodyGraphic != null)
        {
            // 이동 벡터에 따라 각도를 계산
            Vector2 movement = playerMovement.GetCurrentVelocity().normalized;
            float speedFactor = playerMovement.GetCurrentVelocity().magnitude / playerMovement.moveSpeed;
            float tiltAngleX = maxTiltAngle * -movement.y * speedFactor;  // 위/아래 기울기
            float tiltAngleZ = -maxTiltAngle * movement.x * speedFactor;  // 좌/우 기울기

            // 목표 회전 설정
            Quaternion targetRotation = Quaternion.Euler(tiltAngleX, 0f, tiltAngleZ);

            // 부드러운 회전 적용
            bodyGraphic.rotation = Quaternion.Lerp(bodyGraphic.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}