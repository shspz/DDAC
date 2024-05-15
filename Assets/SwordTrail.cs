using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTrail : MonoBehaviour
{
    public Transform swordTip; // 검 끝 오브젝트의 Transform
    public PlayerHand playerHand; // PlayerHand 컴포넌트 참조

    private LineRenderer lineRenderer;
    private List<Vector3> positions = new List<Vector3>(); // 경로를 저장할 리스트
    private int maxPositions = 20; // 최대 저장 위치 수

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false; // 기본적으로 궤적을 비활성화
    }

    void Update()
    {
        if (playerHand.isAttacking)
        {
            lineRenderer.enabled = true; // 공격 상태일 때 궤적 활성화

            // 검 끝의 위치를 리스트에 추가
            if (positions.Count > maxPositions)
            {
                positions.RemoveAt(0); // 리스트가 최대 크기를 초과하면 가장 오래된 위치 제거
            }
            positions.Add(swordTip.position); // 현재 위치 추가

            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray()); // LineRenderer에 위치 설정

            // 검의 궤적을 부드럽게 보이도록 조정
            SmoothPositions();
        }
        else
        {
            if (lineRenderer.enabled)
            {
                lineRenderer.enabled = false; // 공격 상태가 아니면 궤적 비활성화
                positions.Clear(); // 리스트를 비워 다음 공격을 준비
                lineRenderer.positionCount = 0; // LineRenderer 포인트 제거
            }
        }
    }

    void SmoothPositions()
    {
        Vector3[] smoothedPositions = new Vector3[positions.Count];
        for (int i = 0; i < positions.Count; i++)
        {
            if (i == 0 || i == positions.Count - 1)
            {
                smoothedPositions[i] = positions[i];
            }
            else
            {
                smoothedPositions[i] = (positions[i - 1] + positions[i] + positions[i + 1]) / 3;
            }
        }
        lineRenderer.SetPositions(smoothedPositions); // 부드러운 위치로 업데이트
    }
}
