using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("ตัวละครที่ต้องการให้ตาม")]
    [SerializeField] private Transform target;

    [Header("ความสมูทในการตาม (ค่ายิ่งน้อยยิ่งหน่วง)")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("ระยะห่างของกล้อง")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private void LateUpdate()
    {
        // ป้องกัน Error หากไม่ได้ใส่ Target
        if (target == null) return;

        // คำนวณตำแหน่งที่กล้องควรจะไปอยู่
        Vector3 desiredPosition = target.position + offset;

        // ใช้ Lerp เพื่อค่อยๆ เลื่อนกล้องไปหาเป้าหมายอย่างนุ่มนวล
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}