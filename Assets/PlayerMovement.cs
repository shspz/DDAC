using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Tooltip("플레이어의 기본 이동 속도입니다.")]
    public float moveSpeed = 5f;
    
    [Tooltip("플레이어의 Rigidbody2D 컴포넌트를 참조합니다.")]
    public Rigidbody2D rb;
    
    private Vector2 movement; // 플레이어의 이동 방향 및 크기
    public float smoothTime = 0.1F; // 이동 시 부드러운 전환을 위한 시간
    private Vector2 velocity = Vector2.zero; // 현재 속도 저장

    [Tooltip("이동 범위를 제한하는 직사각형의 너비입니다.")]
    public float width = 10f;
    
    [Tooltip("이동 범위를 제한하는 직사각형의 높이입니다.")]
    public float height = 6f;

    [Tooltip("PlayerHand 스크립트에 대한 참조입니다.")]
    public PlayerHand playerHand; // PlayerHand 스크립트 참조

    [Tooltip("부스터 모드에서 속도 배수입니다.")]
    public float boosterSpeedMultiplier = 2f; // 부스터 모드에서의 속도 배수
    
    [Tooltip("부스터 모드에서 소모되는 스테미나 양입니다.")]
    public float staminaDrainRate = 10f; // 부스터 모드에서의 스테미나 소모율
    
    [Tooltip("부스터 모드에서의 부드러운 이동 조정 시간입니다.")]
    public float boosterSmoothTime = 0.3F; // 부스터 모드에서의 부드러운 이동 조정 시간

    [Tooltip("플레이어 몸통 그래픽 객체입니다.")]
    public GameObject bodyGraphic; // 플레이어 몸통 그래픽
    
    [Tooltip("잔상이 생성되는 시간 간격입니다.")]
    public float afterimageDelay = 0.1f; // 잔상 생성 간격
    private float timer; // 잔상 생성 타이머

    [Tooltip("부스트 스테미나의 최대치입니다.")]
    public float maxBoostStamina = 100f;
    [Tooltip("부스트 스테미나 회복 속도입니다.")]
    public float boostStaminaRecoveryRate = 5f;
    [Tooltip("부스트 스테미나가 완전히 소진된 후 재충전 대기 시간입니다.")]
    public float boostRechargeDelay = 2f;
    
    public float boostStamina;
    public float boostRechargeTimer = 0f;

    public float fastRecoveryRate = 20f; // 빠른 회복 속도

    private bool isBoostCheckMode = false; // 부스트 점검 상태
    public LevelGeneration levelGen; // 레벨 생성기 참조
    void Start()
    {
        boostStamina = maxBoostStamina;
    }

    void Update()
    {
        // 플레이어 이동 입력 받기
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        DrawBounds(); // 이동 범위 그리기
        HandleBoostStaminaRecovery();
    }

    public Vector2 GetCurrentVelocity()
    {
        // 현재 속도 반환
        return rb.velocity;
    }

    public bool IsBoosting()
    {
        // 부스터 상태 여부 반환
        return Input.GetKey(KeyCode.LeftShift) && playerHand.totalStamina > 0;
    }

    void FixedUpdate()
    {
        float currentSpeed = moveSpeed;
        float currentSmoothTime = smoothTime;
        bool canBoost = boostStamina > 0 && boostRechargeTimer <= 0 && !isBoostCheckMode;

        if (Input.GetKey(KeyCode.LeftShift) && canBoost)
        {
            currentSpeed *= boosterSpeedMultiplier;
            currentSmoothTime = boosterSmoothTime;
            boostStamina -= staminaDrainRate * Time.fixedDeltaTime;
            playerHand.isBoosting = true;
            timer += Time.fixedDeltaTime;

            if (timer >= afterimageDelay)
            {
                CreateAfterimage();
                timer = 0;
            }
        }
        else
        {
            playerHand.isBoosting = false;
        }

        Vector2 desiredVelocity = movement * currentSpeed;
        rb.velocity = Vector2.SmoothDamp(rb.velocity, desiredVelocity, ref velocity, currentSmoothTime);

        // 범위 내로 이동 제한
        Vector2 roomWorldPos = levelGen.GetCurrentRoomWorldPosition();
        Vector2 minBounds = roomWorldPos + new Vector2(-width / 2, -height / 2);
        Vector2 maxBounds = roomWorldPos + new Vector2(width / 2, height / 2);

        // Clamp player position within these bounds
        rb.position = new Vector2(
            Mathf.Clamp(rb.position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(rb.position.y, minBounds.y, maxBounds.y)
        );
    }

    void HandleBoostStaminaRecovery()
    {
        if (boostRechargeTimer > 0)
        {
            boostRechargeTimer -= Time.deltaTime;
        }

        float recoveryRate = isBoostCheckMode ? fastRecoveryRate : boostStaminaRecoveryRate;

        if (boostStamina < maxBoostStamina)
        {
            if (boostRechargeTimer <= 0f) boostStamina += recoveryRate * Time.deltaTime; // **수정된 부분**
            boostStamina = Mathf.Min(boostStamina, maxBoostStamina);

            if (boostStamina == maxBoostStamina && isBoostCheckMode)
            {
                isBoostCheckMode = false; // 부스트 점검 상태 해제
            }
        }

        if (boostStamina <= 0 && !isBoostCheckMode)
        {
            isBoostCheckMode = true; // 부스트 점검 상태 설정
            boostRechargeTimer = boostRechargeDelay;
        }
    }

    void CreateAfterimage()
    {
        // 잔상 생성
        GameObject afterimage = Instantiate(bodyGraphic, bodyGraphic.transform.position, Quaternion.identity);
        SpriteRenderer sr = afterimage.GetComponent<SpriteRenderer>();
        sr.sprite = bodyGraphic.GetComponent<SpriteRenderer>().sprite;

        // 스케일 및 Z 좌표 조정
        afterimage.transform.localScale = new Vector3(0.49f, 0.7f, 0.7f);
        Vector3 newPosition = afterimage.transform.position;
        newPosition.z = 0.2f;
        afterimage.transform.position = newPosition;

        StartCoroutine(FadeOutAfterimage(sr));
    }

    IEnumerator FadeOutAfterimage(SpriteRenderer afterimageSR)
    {
        // 잔상 서서히 사라지게 처리
        float duration = 0.5f; // 잔상 소멸 시간
        float alpha = 1f;

        while (alpha > 0)
        {
            alpha -= Time.deltaTime / duration;
            afterimageSR.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        Destroy(afterimageSR.gameObject);
    }

    void DrawBounds()
    {
        // 이동 가능 범위를 시각적으로 표시
        Vector3 minBounds = new Vector3(-width / 2, -height / 2, 0);
        Vector3 maxBounds = new Vector3(width / 2, height / 2, 0);
        Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);
        Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);
        Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);
        Debug.DrawLine(topLeft, topRight, Color.red);
        Debug.DrawLine(topRight, bottomRight, Color.red);
        Debug.DrawLine(bottomRight, bottomLeft, Color.red);
        Debug.DrawLine(bottomLeft, topLeft, Color.red);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 방 이동 로직
        if (other.gameObject.CompareTag("DoorTop") && levelGen.doorTop.activeSelf)
        {
            levelGen.MoveToRoom(new Vector2(0, 1));
        }
        else if (other.gameObject.CompareTag("DoorBottom") && levelGen.doorBottom.activeSelf)
        {
            levelGen.MoveToRoom(new Vector2(0, -1));
        }
        else if (other.gameObject.CompareTag("DoorLeft") && levelGen.doorLeft.activeSelf)
        {
            levelGen.MoveToRoom(new Vector2(-1, 0));
        }
        else if (other.gameObject.CompareTag("DoorRight") && levelGen.doorRight.activeSelf)
        {
            levelGen.MoveToRoom(new Vector2(1, 0));
        }
    }

}