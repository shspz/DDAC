using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyA : MonoBehaviour
{
    private List<GameObject> activeAfterImages = new List<GameObject>(); // 활성 잔상 오브젝트 리스트

    public float detectionRange = 10f;
    public float attackRange = 5f;
    public float chargeDistance = 3f;
    public float chargeSpeed = 10f;
    public float coneAngle = 40f;
    public float moveSpeed = 3f;

    public float stunLevel = 100f;
    public int health = 100;
    public int attackDamage = 20;
    public int defense = 5;

    public GameObject afterImagePrefab; // 잔상 프리팹

    // 방향에 따른 스프라이트 배열
    public Sprite[] directionSprites;

    private GameObject player;
    private Vector3 playerDirection;
    private Vector3 lastMoveDirection;
    private float playerDistance;
    private bool isChasing = false;
    private bool isAttacking = false;
    private float initialAttackAngle;
    private Vector3 attackDirection;
    private float attackCooldown = 3f;
    private float reducedCooldown = 0.5f; // 짧은 쿨타임
    private bool canAttack = true;

    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러

    private Rigidbody2D rb; // Rigidbody2D 컴포넌트 참조
    // 피격 상태 변수 추가
    private bool isHit = false;
    // PlayerHand 스크립트를 참조하기 위한 변수
    public PlayerHand playerHandScript;

    public Color hitColor = Color.red; // 피격 시 색상
    public Color originalColor; // 원래 색상 저장용

    // 변수 선언
    public int maxHealth = 100;  // 최대 체력
    public int currentHealth;    // 현재 체력

    

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 컴포넌트 가져오기
        currentHealth = maxHealth;  // 시작 시 현재 체력을 최대 체력으로 설정

        // PlayerHand 태그를 가진 오브젝트에서 PlayerHand 컴포넌트를 찾아 할당
        GameObject playerHandObject = GameObject.FindGameObjectWithTag("PlayerHand");
        if (playerHandObject != null)
        {
            playerHandScript = playerHandObject.GetComponent<PlayerHand>();
            if (playerHandScript == null)
            {
                Debug.LogError("PlayerHand script not found on 'PlayerHand' tagged object!");
            }
        }
        else
        {
            Debug.LogError("No object with tag 'PlayerHand' found!");
        }
    }

    void OnEnable()
    {
        // Reset variables and state when the enemy is reactivated
        isChasing = false;
        isAttacking = false;
        isHit = false;
        canAttack = true;
        playerDirection = Vector3.zero;
        lastMoveDirection = Vector3.zero;
        playerDistance = 0f;
        initialAttackAngle = 0f;
        attackDirection = Vector3.zero;

    }

    void OnDisable()
    {
        // 비활성화 시 모든 잔상 오브젝트 제거
        foreach (GameObject afterImage in activeAfterImages)
        {
            if (afterImage != null) // null 체크
            {
                Destroy(afterImage);
            }
        }
        activeAfterImages.Clear(); // 리스트 클리어

        // 모든 코루틴 중지
        StopAllCoroutines();
    }



    void Update()
    {
        playerDirection = player.transform.position - transform.position;
        playerDistance = playerDirection.magnitude;

        if (playerDistance <= detectionRange && !isAttacking && canAttack)
        {
            if (!isChasing)
            {
                isChasing = true;
                Debug.Log("Player detected. Chasing...");
            }
            StartChase();
        }
        else if (!canAttack && playerDistance <= detectionRange)
        {
            if (!isChasing)
            {
                isChasing = true;
                Debug.Log("Resuming chase during cooldown...");
            }
            MoveTowardsPlayer(Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg);
        }
        else if (isChasing)
        {
            isChasing = false;
            Debug.Log("Player lost. Stopping chase...");
        }

        // 이동 방향에 따라 스프라이트 업데이트
        UpdateSpriteDirection();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon") && !isHit && playerHandScript.isAttacking)
        {
            Debug.Log("피격!");
            isHit = true;
            TakeDamage(10);  // 피격 시 10의 데미지를 입음
            spriteRenderer.color = hitColor; // 색상 변경
            StartCoroutine(HandleHit());
            StartCoroutine(ResetHit());
            StartCoroutine(RestoreColor()); // 색상 복원 코루틴 시작
        }
    }
    void TakeDamage(int damage)
    {
        currentHealth -= damage;  // 피해량만큼 체력 감소
        Debug.Log("Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();  // 체력이 0 이하가 되면 사망 처리
        }
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " is dead!");
        EnemyActivationManager.Instance.RemoveEnemyFromList(this.GetComponent<EnemyRoomTracker>());  // EnemyActivationManager에서 이 적 제거
        Destroy(gameObject);  // 게임 오브젝트 제거
    }

    
    private IEnumerator RestoreColor()
    {
        float duration = 0.3f; // 색상 복원에 걸리는 시간
        float elapsed = 0f;

        while (elapsed < duration)
        {
            spriteRenderer.color = Color.Lerp(hitColor, originalColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalColor; // 최종 색상을 원래 색상으로 설정
    }

    private IEnumerator HandleHit()
    {
        // 추격 중지
        isChasing = false;

        // 피격 반응으로 일정 거리 뒤로 이동
        float retreatDuration = 0.1f; // 뒤로 물러나는 시간
        float retreatSpeed = 10f; // 뒤로 물러나는 속도
        Vector3 retreatDirection = -(player.transform.position - transform.position).normalized; // 플레이어 반대 방향

        float endTime = Time.time + retreatDuration;
        while (Time.time < endTime)
        {
            rb.velocity = retreatDirection * retreatSpeed; // Rigidbody를 사용하여 뒤로 이동
            yield return null;
        }

        rb.velocity = Vector2.zero; // 이동 정지

        // 짧은 대기 후 추격 재개
        yield return new WaitForSeconds(1f);
        isChasing = true;
    }

    private IEnumerator ResetHit()
    {
        // 플레이어의 공격 상태가 false가 될 때까지 기다림
        yield return new WaitUntil(() => !playerHandScript.isAttacking);
        isHit = false; // 피격 상태를 리셋
        Debug.Log("피격 상태 리셋 완료.");
    }
 

    void StartChase()
    {
        float angleToPlayer = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;

        if (!isAttacking && canAttack && playerDistance <= attackRange)
        {
            isAttacking = true;
            initialAttackAngle = angleToPlayer;
            lastMoveDirection = attackDirection;
            StartCoroutine(AttackState());
            Debug.Log("Attack initiated.");
        }

        if (isChasing && !isAttacking)
        {
            MoveTowardsPlayer(angleToPlayer);
        }
    }

    IEnumerator AttackState()
    {
        rb.velocity = Vector2.zero;
        Debug.Log("Attack mode start");
        attackDirection = (player.transform.position - transform.position).normalized;
        StartCoroutine(Shake(0.5f, 0.1f));  // 진동 효과 시작
        yield return new WaitForSeconds(0.5f);  // 플레이어 타겟팅 대기

        if (true/*IsPlayerInAttackCone(Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg)*/)
        {
            Debug.Log("Player within attack cone.");  // 플레이어 타겟팅 완료
            float startTime = Time.time;
            float nextImageTime = 0;

            while (Time.time < startTime + (chargeDistance / chargeSpeed))
            {
                if (Time.time >= nextImageTime)
                {
                    CreateAfterImage();
                    nextImageTime = Time.time + 0.05f;  // 다음 잔상 생성 시간 설정
                }

                Vector3 newPos = transform.position + attackDirection * chargeSpeed * Time.deltaTime;
                transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
                yield return null;
            }
            Debug.Log("Attack charge complete.");
            StartCoroutine(CooldownAttack(false));
        }
        else
        {
            Debug.Log("Target lost early. Reducing cooldown.");
            StartCoroutine(CooldownAttack(true));
        }

        isAttacking = false;
        canAttack = false;
    }


    IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos; // 위치를 원래대로 복구
    }



    void CreateAfterImage()
    {
        GameObject afterImage = Instantiate(afterImagePrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.05f), transform.rotation);
        SpriteRenderer sr = afterImage.GetComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>().sprite;
        sr.color = new Color(1f, 1f, 1f, 0.5f);
        activeAfterImages.Add(afterImage); // 리스트에 추가
        StartCoroutine(FadeOutAfterImage(sr, afterImage)); // 수정된 코루틴 호출
    }


    IEnumerator FadeOutAfterImage(SpriteRenderer sr, GameObject afterImageInstance)
    {
        float duration = 0.5f;
        while (sr.color.a > 0)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a - Time.deltaTime / duration);
            yield return null;
        }
        activeAfterImages.Remove(afterImageInstance); // 제거 완료 시 리스트에서도 제거
        Destroy(afterImageInstance);
    }

    IEnumerator FadeOutAfterImage(SpriteRenderer afterImageSR)
    {
        float duration = 0.5f;
        while (afterImageSR.color.a > 0)
        {
            afterImageSR.color = new Color(afterImageSR.color.r, afterImageSR.color.g, afterImageSR.color.b, afterImageSR.color.a - Time.deltaTime / duration);
            yield return null;
        }
        Destroy(afterImageSR.gameObject);
    }

    IEnumerator CooldownAttack(bool earlyExit)
    {
        float cooldownTime = earlyExit ? reducedCooldown : attackCooldown;
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
        Debug.Log("Cooldown finished, ready to attack.");
    }

    bool IsPlayerInAttackCone(float currentAngle)
    {
        float startAngle = initialAttackAngle - coneAngle / 2;
        float endAngle = initialAttackAngle + coneAngle / 2;
        return currentAngle >= startAngle && currentAngle <= endAngle && playerDistance <= attackRange;
    }

    void MoveTowardsPlayer(float angleToPlayer)
    {
        Vector3 moveDirection = (new Vector3(playerDirection.x, playerDirection.y, 0)).normalized;
        rb.velocity = moveDirection * moveSpeed; // Rigidbody를 사용하여 이동
    }

    void UpdateSpriteDirection()
    {
        Vector2 directionVector;
        if (isAttacking && attackDirection != Vector3.zero) // 공격 중이고 공격 방향이 설정된 경우
        {
            directionVector = new Vector2(attackDirection.x, attackDirection.y).normalized;
        }
        else if (!isAttacking && rb.velocity != Vector2.zero) // 공격 중이 아니고 이동 중인 경우
        {
            directionVector = new Vector2(rb.velocity.x, rb.velocity.y).normalized;
        }
        else
        {
            return; // 이동하지 않고 공격도 하지 않는 경우 스프라이트 업데이트 없음
        }

        UpdateSpriteBasedOnDirection(directionVector);
    }

    void UpdateSpriteBasedOnDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return; // 방향이 없는 경우 업데이트 하지 않음

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int directionIndex = (int)((angle + 360 + 22.5) % 360 / 45);
        spriteRenderer.sprite = directionSprites[directionIndex];
    }


    private void OnDrawGizmos()
    {
        if (isAttacking)
        {
            Vector3 startPosition = Quaternion.Euler(0, 0, initialAttackAngle - coneAngle / 2) * Vector3.right * attackRange;
            Vector3 endPosition = Quaternion.Euler(0, 0, initialAttackAngle + coneAngle / 2) * Vector3.right * attackRange;
            Debug.DrawLine(transform.position, transform.position + startPosition, Color.red);
            Debug.DrawLine(transform.position, transform.position + endPosition, Color.red);
            Debug.DrawLine(transform.position + startPosition, transform.position + endPosition, Color.red);
        }
    }
}