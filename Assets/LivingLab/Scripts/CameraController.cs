using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    public float distance = 5.0f;
    public float height = 3.0f;
    public float rotationDamping = 2.0f;
    public float xOffsetAngle = 30.0f; // X eksenindeki rotasyon a��s�

    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;

            // Kameran�n yeni pozisyonunu ve rotasyonunu hesapla
            Vector3 desiredPosition = targetPosition - (target.forward * distance) + (Vector3.up * height);
            Quaternion desiredRotation = Quaternion.LookRotation(targetPosition - transform.position, target.up);

            // X eksenindeki rotasyonu ekleyerek kameran�n bak�� a��s�n� de�i�tir
            desiredRotation *= Quaternion.Euler(xOffsetAngle, 0, 0);

            // Kamera pozisyonunu ve rotasyonunu yumu�ak bir �ekilde g�ncelle
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationDamping);
        }
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

}
