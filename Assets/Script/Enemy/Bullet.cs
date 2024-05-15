using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float maxDistance = 50f; // 최대 이동 거리
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // 초기 위치 저장
    }

    void Update()
    {
        CheckDistanceTraveled();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어에 닿았을 때 또는 검(무기)에 닿았는데 플레이어가 방어 중일 경우 바로 삭제
        if (collision.CompareTag("Player") || (collision.CompareTag("Weapon") && collision.transform.parent.GetComponent<PlayerHand>().isDefending))
        {
            DestroyImmediate();
        }
    }

    private void CheckDistanceTraveled()
    {
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            DestroyBullet(); // 최대 거리에 도달하면 총알 삭제
        }
    }

    private void DestroyImmediate()
    {
        Destroy(gameObject); // 총알 게임 오브젝트 즉시 삭제
    }

    private void DestroyBullet()
    {
        StartCoroutine(FadeOutThenDestroy()); // 점차 투명해지면서 삭제
    }

    IEnumerator FadeOutThenDestroy()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float duration = 0.5f; // 페이드 아웃 지속 시간
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // 총알 게임 오브젝트 최종 삭제
    }
}
