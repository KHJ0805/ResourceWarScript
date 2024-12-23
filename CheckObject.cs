using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObject : MonoBehaviour
{
    void Start()
    {
        string objectName = gameObject.name;

        Vector3 worldPosition = transform.position;

        // ���� GameObject�� BoxCollider�� ������
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            // BoxCollider�� ���� ũ�⸦ ���
            Vector3 worldSize = Vector3.Scale(boxCollider.size, transform.lossyScale);

            // Collider�� center�� ���� ��ǥ�迡���� �߽��̹Ƿ�, �̸� ���� ��ǥ�� ��ȯ
            Vector3 worldCenter = transform.TransformPoint(boxCollider.center);

            // ���� �߽ɰ� ũ�⸦ �α׷� ���
            Debug.Log($"{objectName}�� {worldCenter}��ġ��, {worldSize} ũ��� �����մϴ�.");
        }
        else
        {
            Debug.Log($"{objectName}�� {worldPosition} ��ġ�� ������ BoxCollider�� �����ϴ�.");
        }
    }
}
