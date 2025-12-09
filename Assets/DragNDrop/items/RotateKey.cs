using UnityEngine;

public class RotateKey : MonoBehaviour
{
    public float rotationSpeed = 180f;

    void Update()
    {
        if (transform == null) return;
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }
}
