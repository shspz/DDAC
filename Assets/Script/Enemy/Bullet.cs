using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float maxDistance = 50f; // �ִ� �̵� �Ÿ�
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // �ʱ� ��ġ ����
    }

    void Update()
    {
        CheckDistanceTraveled();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �÷��̾ ����� �� �Ǵ� ��(����)�� ��Ҵµ� �÷��̾ ��� ���� ��� �ٷ� ����
        if (collision.CompareTag("Player") || (collision.CompareTag("Weapon") && collision.transform.parent.GetComponent<PlayerHand>().isDefending))
        {
            DestroyImmediate();
        }
    }

    private void CheckDistanceTraveled()
    {
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            DestroyBullet(); // �ִ� �Ÿ��� �����ϸ� �Ѿ� ����
        }
    }

    private void DestroyImmediate()
    {
        Destroy(gameObject); // �Ѿ� ���� ������Ʈ ��� ����
    }

    private void DestroyBullet()
    {
        StartCoroutine(FadeOutThenDestroy()); // ���� ���������鼭 ����
    }

    IEnumerator FadeOutThenDestroy()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float duration = 0.5f; // ���̵� �ƿ� ���� �ð�
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // �Ѿ� ���� ������Ʈ ���� ����
    }
}
