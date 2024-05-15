using System.Collections;
using UnityEngine;

public class EnemyC : MonoBehaviour
{
    public float attackRange = 10f;
    public float attackCooldown = 1f;
    public Sprite[] directionSprites;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public float decelerationRate = 0.1f;  // 감속 비율 추가

    public Color hitColor = Color.red;
    public int maxHealth = 100;
    private int currentHealth;
    private Color originalColor;

    private GameObject player;
    private float playerDistance;
    private float cooldownTimer = 0f;
    private bool isHit = false;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PlayerHand playerHandScript;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        GameObject playerHandObject = GameObject.FindGameObjectWithTag("PlayerHand");
        if (playerHandObject != null)
        {
            playerHandScript = playerHandObject.GetComponent<PlayerHand>();
        }
    }

    void Update()
    {
        CheckDistanceToPlayer();
        HandleAttack();
        UpdateSpriteDirection(player.transform.position - transform.position);
        ApplyDeceleration(); // 감속 적용
    }

    private void ApplyDeceleration()
    {
        if (rb.velocity.magnitude > 0)
        {
            rb.velocity -= decelerationRate * rb.velocity.normalized * Time.deltaTime;
            if (rb.velocity.magnitude < decelerationRate * Time.deltaTime)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    private void CheckDistanceToPlayer()
    {
        if (player != null)
        {
            playerDistance = Vector3.Distance(transform.position, player.transform.position);
        }
    }

    private void HandleAttack()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (playerDistance <= attackRange && cooldownTimer <= 0)
        {
            Debug.Log("공격!");
            FireBullet();
            cooldownTimer = attackCooldown;
        }
    }

    private void FireBullet()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon") && !isHit && playerHandScript.isAttacking)
        {
            Debug.Log("피격!");
            isHit = true;
            TakeDamage(10);
            spriteRenderer.color = hitColor;
            StartCoroutine(HandleHit());
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
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ResetHit()
    {
        yield return new WaitUntil(() => !playerHandScript.isAttacking);
        isHit = false;
        Debug.Log("피격 상태 리셋 완료.");
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            int directionIndex = (int)((angle + 360 + 22.5) % 360 / 45);
            spriteRenderer.sprite = directionSprites[directionIndex];
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
