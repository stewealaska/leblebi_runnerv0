using UnityEngine;

public class MagnetCollectible : MonoBehaviour
{
    [Header("Magnet Power")]
    public float duration = 12f;
    public float radius = 10f;
    public float pullSpeed = 18f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        MagnetAttractor magnet = other.GetComponent<MagnetAttractor>();
        if (magnet == null)
            magnet = other.GetComponentInParent<MagnetAttractor>();

        if (magnet != null)
            magnet.ActivateMagnet(duration, radius, pullSpeed);

        Destroy(gameObject);
    }
}
