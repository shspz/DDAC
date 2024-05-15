using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyB : MonoBehaviour
{
    public float detectionRange = 15f;
    public float attackRange = 7f;
    public float chaseSpeed = 10f;
    public float chaseDistance = 5f;
    public float chaseDuration = 0.3f;
    public float chaseCooldown = 2f;
    public float decelerationRate = 1.5f;
    public float accuracyRange = 10f;
    public float directionalError = 15f;

    public int maxHealth = 150;
    private int currentHealth;
    public Color hitColor = Color.red;
    private Color originalColor;

    private GameObject player;
    private float playerDistance;
    private bool isChasing = false;
    private bool isCoolingDown = false;
    private bool isHit = false;
    private bool inChaseAttack = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerHand playerHandScript;

    public Sprite[] directionSprites;  // 방향에 따른 스프라이트 배열

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;

        GameObject playerHandObject = GameObject.FindGameObjectWithTag("PlayerHand");
        if (playerHandObject != null)
        {
            playerHandScript = playerHandObject.GetComponent<PlayerHand>();
        }
    }

    void OnEnable()
    {
        isChasing = false;
        isCoolingDown = false;
        isHit = false;
        inChaseAttack = false;
        playerDistance = 0f;


        currentHealth = maxHealth;
    }

    void Update()
    {
        CheckDistanceToPlayer();

        if (!isHit && playerDistance <= detectionRange && !isCoolingDown)
        {
            StartChase();
        }
    }

    private void CheckDistanceToPlayer()
    {
        if (player != null)
        {
            playerDistance = Vector3.Distance(transform.position, player.transform.position);
        }
    }

    private void StartChase()
    {
        if (!isChasing)
        {
            isChasing = true;
            Debug.Log("EnemyB is now chasing the player.");
            StartCoroutine(PerformChase());
        }
    }
    void UpdateSpriteDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            UpdateSpriteBasedOnDirection(direction);
        }
    }

    void UpdateSpriteBasedOnDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int directionIndex = (int)((angle + 360 + 22.5) % 360 / 45);
        spriteRenderer.sprite = directionSprites[directionIndex];
    }
    IEnumerator PerformChase()
    {
        float angleOffset = playerDistance > accuracyRange ? Random.Range(-directionalError, directionalError) : 0;
        Vector2 direction = (Quaternion.Euler(0, 0, angleOffset) * (player.transform.position - transform.position)).normalized;
        rb.velocity = direction * chaseSpeed;  // 직접 속도 설정

        // 스프라이트 방향 업데이트
        UpdateSpriteDirection(direction);

        // 지속 시간 동안 돌진 유지
        yield return new WaitForSeconds(chaseDuration);

        // 감속 시작
        float currentSpeed = chaseSpeed;
        while (currentSpeed > 0)
        {
            currentSpeed -= decelerationRate * Time.deltaTime;
            rb.velocity = direction * currentSpeed;
            yield return null;
        }

        rb.velocity = Vector2.zero;  // 완전히 멈춤
        inChaseAttack = false;
        isChasing = false;
        StartCoroutine(ChaseCooldown());
    }


    IEnumerator ChaseCooldown()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(chaseCooldown);
        isCoolingDown = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon") && !isHit && playerHandScript.isAttacking)
        {
            Debug.Log("EnemyB hit!");
            isHit = true;
            TakeDamage(10);
            spriteRenderer.color = hitColor;

            if (!inChaseAttack)
            {
                StartCoroutine(HandleHit());
            }

            StartCoroutine(ResetHit());
            StartCoroutine(RestoreColor());
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " is dead!");
        EnemyActivationManager.Instance.RemoveEnemyFromList(this.GetComponent<EnemyRoomTracker>());
        Destroy(gameObject);
    }


    private IEnumerator RestoreColor()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            spriteRenderer.color = Color.Lerp(hitColor, originalColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }

    private IEnumerator HandleHit()
    {
        if (!inChaseAttack)
        { // 돌진 중이 아닐 때만 넉백 효과 적용
            float retreatDuration = 0.1f;
            float retreatSpeed = 10f;
            Vector3 retreatDirection = -(player.transform.position - transform.position).normalized;

            float endTime = Time.time + retreatDuration;
            while (Time.time < endTime)
            {
                rb.velocity = retreatDirection * retreatSpeed;
                yield return null;
            }

            rb.velocity = Vector2.zero;
        }

        yield return new WaitForSeconds(1f);
        isChasing = false;
    }


    private IEnumerator ResetHit()
    {
        yield return new WaitUntil(() => !playerHandScript.isAttacking);
        isHit = false;
        Debug.Log("Hit status reset.");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, accuracyRange);
    }
}
