using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostBar : MonoBehaviour
{
    public PlayerMovement playerMovement; // PlayerMovement 스크립트 참조
    private Image boostBarImage; // Image 컴포넌트 참조

    void Start()
    {
        boostBarImage = GetComponent<Image>(); // Image 컴포넌트 가져오기
    }

    void Update()
    {
        if (playerMovement != null && boostBarImage != null)
        {
            boostBarImage.fillAmount = playerMovement.boostStamina / playerMovement.maxBoostStamina; // 게이지 업데이트
        }
    }
}
