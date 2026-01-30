using UnityEngine;

public class SpinCoin : MonoBehaviour
{
    public float rotateSpeed = 90f; // saniyede 90 derece, istersen 60 yap 120 yap

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
    }
}
