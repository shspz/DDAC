using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [Tooltip("플레이어 오브젝트를 참조합니다. 플레이어 손이 이 오브젝트를 따라갑니다.")]
    public Transform player;

    [Tooltip("손이 플레이어를 따라가는 속도입니다. 높을수록 빠르게 따라갑니다.")]
    public float followSpeed = 10f;

    [Tooltip("공격 상태일 때 손의 이동 속도입니다.")]
    public float attackMoveSpeed = 15f;

    [Tooltip("플레이어와 손 사이의 최소 거리를 정의합니다.")]
    public float minDistanceFromPlayer = 0.7f;

    [Tooltip("공격 상태일 때 손과 플레이어 사이의 거리가 어느 정도로 확장되는지를 정의합니다.")]
    public float expandedRadiusDuringAttack = 1f;

    [Tooltip("공격 상태에서 기본 상태로 전환된 후 다음 공격이 가능할 때까지의 대기 시간입니다.")]
    public float attackCooldown = 1f;

    [Tooltip("스테미나 소모가 적을 경우 적용되는 감소된 쿨다운 시간입니다.")]
    public float reducedCooldown = 0.5f;

    [Tooltip("쿨다운 감소를 적용하기 위한 스테미나 소모 임계값입니다.")]
    public float staminaUsageThreshold = 2f;

    [Tooltip("전체 스테미나의 최대치입니다.")]
    public float maxTotalStamina = 100f;

    [Tooltip("공격 스테미나의 최대치입니다.")]
    public float maxAttackStamina = 50f;

    [Tooltip("스테미나 회복 속도를 정의합니다.")]
    public float staminaRecoveryRate = 5f;

    [Tooltip("비활성 상태에서 스테미나가 감소하는 속도입니다.")]
    public float passiveStaminaDrain = 1f;

    public bool isAttacking = false;  // 현재 공격 중인지 여부를 나타냅니다.
    public bool isDefending = false;  // 현재 방어 상태인지 여부를 나타냅니다.
    public float cooldownTimer = 0f;  // 쿨다운을 계산하는 타이머입니다.
    private float initialAttackAngle;  // 공격 시작 시의 각도입니다.
    public float totalStamina;         // 현재 전체 스테미나입니다.
    public float attackStamina;        // 현재 공격 스테미나입니다.

    public bool isBoosting = false;  // 부스트 중인지 여부를 나타냅니다.

    // 방어 상태 쿨타임 및 지속 시간 관련 변수
    [Tooltip("방어 상태의 최대 지속 시간입니다.")]
    public float maxDefenseDuration = 5f; // 최대 방어 지속 시간
    [Tooltip("방어 상태의 쿨타임입니다.")]
    public float defenseCooldown = 2f; // 방어 쿨타임

    private float defenseTimer = 0f; // 방어 지속 타이머
    private float defenseCooldownTimer = 0f; // 방어 쿨다운 타이머

    [Tooltip("공격 상태가 지속되어야 하는 최소 시간입니다.")]
    public float minimumAttackDuration = 0.25f; // 공격 상태 최소 지속 시간

    private float attackStartTime; // 공격 시작 시간
    public float attackDuration; // 실제 공격 지속 시간
    

    void Start()
    {
        totalStamina = maxTotalStamina;
        attackStamina = 0f;
    }

    void Update()
    {
        // 쿨다운 및 방어 타이머 감소
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        if (defenseCooldownTimer > 0)
        {
            defenseCooldownTimer -= Time.deltaTime;
        }

        // 스테미나 회복
        if (totalStamina < maxTotalStamina && !isAttacking)
        {
            totalStamina += staminaRecoveryRate * Time.deltaTime;
        }
        totalStamina = Mathf.Min(totalStamina, maxTotalStamina);

        // 입력 처리
        HandleInput();

        // 상태에 따른 손의 움직임 처리
        if (isAttacking)
        {
            AttackMovement();
        }
        else if (isDefending)
        {
            DefendMovement();
        }
        else
        {
            FollowPlayerWithInterpolation();
        }

        PointTowardsMouse();
    }



    void HandleInput()
    {
        // 공격 상태 전환 로직: 부스트 중이 아닐 때만 공격 가능
        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0 && totalStamina > 0 && !isAttacking && !isDefending && !isBoosting)
        {
            StartAttack();
        }
        else if (isAttacking && (!Input.GetMouseButton(0) || attackStamina <= 0))
        {
            EndAttack();
        }

        // 방어 상태 전환 로직: 부스트 중이 아닐 때만 방어 가능
        if (Input.GetMouseButtonDown(1) && !isAttacking && defenseCooldownTimer <= 0 && !isBoosting)
        {
            StartDefense();
        }
        else if (isDefending && (!Input.GetMouseButton(1) || defenseTimer >= maxDefenseDuration))
        {
            EndDefense();
        }
    }

    void StartDefense()
    {
        isDefending = true;
        defenseTimer = 0f; // 방어 타이머 초기화
    }

    void EndDefense()
    {
        isDefending = false;
        defenseCooldownTimer = defenseCooldown; // 방어 쿨다운 설정
    }

    void StartAttack()
    {
        isAttacking = true;
        attackStartTime = Time.time; // 공격 시작 시간 기록
        initialAttackAngle = GetCurrentAngle();
        attackStamina = maxAttackStamina;
    }

    void EndAttack()
    {
        isAttacking = false;
        attackDuration = Time.time - attackStartTime; // 공격 지속 시간 계산

        // 공격 지속 시간이 설정된 최소 지속 시간보다 짧다면 쿨다운 감소
        cooldownTimer = (attackDuration <= minimumAttackDuration) ? reducedCooldown : attackCooldown;
    }

    void AttackMovement()
    {
        Vector3 targetPosition = CalculateTargetPosition(expandedRadiusDuringAttack);
        transform.position = Vector3.Lerp(transform.position, targetPosition, attackMoveSpeed * Time.deltaTime);
        UpdateStaminaCost();
    }

    void DefendMovement()
    {
        // 플레이어와 마우스 사이의 방향 벡터를 계산
        Vector3 directionToMouse = (GetMousePosition() - player.position).normalized;
        
        // 방향 벡터를 기준으로 오른쪽 방향을 계산
        Vector3 rightOffset = Quaternion.Euler(0, 0, 20) * directionToMouse;

        // 방어 상태에서의 위치를 플레이어의 오른쪽으로 조정
        Vector3 targetPosition = player.position + rightOffset * 1f; // 오른쪽으로 1 유닛 이동

        // 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        defenseTimer += Time.deltaTime; // 방어 타이머 증가
    }


    void FollowPlayerWithInterpolation()
    {
        Vector3 targetPosition = CalculateTargetPosition(minDistanceFromPlayer);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    Vector3 CalculateTargetPosition(float radius)
    {
        // 손의 목표 위치 계산
        Vector3 mousePosition = GetMousePosition();
        return player.position + (mousePosition - player.position).normalized * radius;
    }

    void UpdateStaminaCost()
    {
        // 스테미나 감소 계산 및 업데이트
        float currentAngle = GetCurrentAngle();
        float angleChange = Mathf.Abs(Mathf.DeltaAngle(initialAttackAngle, currentAngle));
        float staminaCost = angleChange * 0.1f;
        attackStamina -= staminaCost + passiveStaminaDrain * Time.deltaTime;
        totalStamina -= staminaCost + passiveStaminaDrain * Time.deltaTime;
        initialAttackAngle = currentAngle;
    }

    float GetCurrentAngle()
    {
        // 현재 마우스 위치로부터 각도 계산
        Vector2 direction = GetMousePosition() - player.position;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    Vector3 GetMousePosition()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z축은 사용하지 않습니다.
        return mousePosition;
    }

    void PointTowardsMouse()
    {
        // 마우스 위치로부터 현재 방향 계산
        Vector3 direction = GetMousePosition() - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 방어 상태일 때 추가로 90도 회전
        if (isDefending)
        {
            angle -= 70;
        }

        // 계산된 각도로 손의 회전을 적용
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

}
