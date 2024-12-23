using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObject : MonoBehaviour
{
    void Start()
    {
        string objectName = gameObject.name;

        Vector3 worldPosition = transform.position;

        // 현재 GameObject의 BoxCollider를 가져옴
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            // BoxCollider의 월드 크기를 계산
            Vector3 worldSize = Vector3.Scale(boxCollider.size, transform.lossyScale);

            // Collider의 center는 로컬 좌표계에서의 중심이므로, 이를 월드 좌표로 변환
            Vector3 worldCenter = transform.TransformPoint(boxCollider.center);

            // 계산된 중심과 크기를 로그로 출력
            Debug.Log($"{objectName}는 {worldCenter}위치에, {worldSize} 크기로 존재합니다.");
        }
        else
        {
            Debug.Log($"{objectName}은 {worldPosition} 위치에 있지만 BoxCollider가 없습니다.");
        }
    }
}
