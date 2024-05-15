using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    public Image staminaBarFill;
    public Image attackStaminaBarFill;
    public Image cooldownBarFill;
    public Image attackStaminaBorderFill; // 공격 스테미너 바와 동일한 위치에 있는 보더

    // 새로 추가된 보더 참조
    public RectTransform attackStaminaBorderA;
    public RectTransform attackStaminaBorderB;

    public PlayerHand playerHand;
    private bool wasAttackingLastFrame; // 이전 프레임에서 공격 중인지 여부

    void Start()
    {
        AdjustAttackStaminaBarWidth();
        wasAttackingLastFrame = false; // 초기화
    }

    void Update()
    {
        if (playerHand != null)
        {
            staminaBarFill.fillAmount = playerHand.totalStamina / playerHand.maxTotalStamina;
            attackStaminaBarFill.fillAmount = playerHand.attackStamina / playerHand.maxAttackStamina;
            cooldownBarFill.fillAmount = 1.0f - (playerHand.cooldownTimer / playerHand.attackCooldown);

            // 플레이어의 공격 상태가 변경될 때 마다 공격 관련 UI 업데이트
            if (!wasAttackingLastFrame && playerHand.isAttacking)
            {
                AlignAttackBar();
            }
            else if (wasAttackingLastFrame && !playerHand.isAttacking)
            {
                HideAttackStaminaBars();
            }

            wasAttackingLastFrame = playerHand.isAttacking; // 현재 프레임의 공격 상태 저장
        }
    }

    private void HideAttackStaminaBars()
    {
        // 공격 스테미나 바와 관련된 UI 요소 숨기기
        attackStaminaBarFill.gameObject.SetActive(false);
        attackStaminaBorderFill.gameObject.SetActive(false);
        attackStaminaBorderA.gameObject.SetActive(false);
        attackStaminaBorderB.gameObject.SetActive(false);
    }
    private void AdjustAttackStaminaBarWidth()
    {
        if (playerHand == null) return;

        float totalStaminaBarWidth = staminaBarFill.rectTransform.rect.width;
        float attackStaminaBarWidth = totalStaminaBarWidth * (playerHand.maxAttackStamina / playerHand.maxTotalStamina);
        RectTransform attackBarRect = attackStaminaBarFill.rectTransform;
        attackBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, attackStaminaBarWidth);
        attackStaminaBorderFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, attackStaminaBarWidth);
    }

    private void AlignAttackBar()
    {
        // 공격 스테미나 바와 관련된 UI 요소 보이기
        attackStaminaBarFill.gameObject.SetActive(true);
        attackStaminaBorderFill.gameObject.SetActive(true);
        attackStaminaBorderA.gameObject.SetActive(true);
        attackStaminaBorderB.gameObject.SetActive(true);

        float totalWidth = staminaBarFill.rectTransform.rect.width;
        float attackWidth = attackStaminaBarFill.rectTransform.rect.width;

        float staminaRatio = playerHand.totalStamina / playerHand.maxTotalStamina;
        float divisor = Mathf.Lerp(1f, 0f, staminaRatio);

        float maxOffset = (totalWidth - attackWidth) / 2 - divisor * totalWidth;
        float currentOffset = maxOffset;

        RectTransform attackBarRect = attackStaminaBarFill.rectTransform;
        attackBarRect.anchoredPosition = new Vector2(staminaBarFill.rectTransform.anchoredPosition.x + currentOffset, attackBarRect.anchoredPosition.y);
        attackStaminaBorderFill.rectTransform.anchoredPosition = new Vector2(staminaBarFill.rectTransform.anchoredPosition.x + currentOffset, attackBarRect.anchoredPosition.y);

        // 새로 추가된 테두리의 위치 설정
        attackStaminaBorderA.anchoredPosition = new Vector2(attackBarRect.anchoredPosition.x - attackWidth / 2, attackBarRect.anchoredPosition.y);
        attackStaminaBorderB.anchoredPosition = new Vector2(attackBarRect.anchoredPosition.x + attackWidth / 2, attackBarRect.anchoredPosition.y);
    }
}
