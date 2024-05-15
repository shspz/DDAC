using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImage : MonoBehaviour
{
    public SpriteRenderer bodySpriteRenderer; // 플레이어의 몸통 스프라이트 렌더러 참조
    public float afterImageLifetime = 0.5f; // 잔상의 수명
    public float afterImageDelay = 0.1f; // 잔상 생성 간격

    private float timeSinceLastAfterImage = 0f; // 마지막 잔상 생성 이후의 시간

    // 부스터 모드 확인을 위한 PlayerMovement 참조
    public PlayerMovement playerMovement;

    void Update()
    {
        if (playerMovement.IsBoosting() && playerMovement.playerHand.totalStamina > 0)
        {
            if (timeSinceLastAfterImage > afterImageDelay)
            {
                CreateAfterImage();
                timeSinceLastAfterImage = 0f;
            }
        }
        timeSinceLastAfterImage += Time.deltaTime;
    }

    void CreateAfterImage()
    {
        GameObject afterImage = new GameObject("AfterImage");
        SpriteRenderer afterImageRenderer = afterImage.AddComponent<SpriteRenderer>();
        afterImageRenderer.sprite = bodySpriteRenderer.sprite;
        afterImage.transform.position = transform.position;
        afterImage.transform.rotation = transform.rotation;
        afterImage.transform.localScale = transform.localScale;
        Destroy(afterImage, afterImageLifetime);

        // 잔상의 페이드 아웃 효과
        StartCoroutine(FadeOut(afterImageRenderer));
    }

    IEnumerator FadeOut(SpriteRenderer spriteRenderer)
    {
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime / afterImageLifetime;
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        Destroy(spriteRenderer.gameObject);
    }
}
