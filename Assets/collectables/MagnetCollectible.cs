using UnityEngine;

public class MagnetCollectible : MonoBehaviour
{
    public float duration = 10f;
    public float radius = 10f;
    public float speed = 18f;

    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        MagnetAttractor attractor = other.GetComponentInParent<MagnetAttractor>();
        if (attractor != null)
            attractor.ActivateMagnet(duration, radius, speed);

        CollectiblePickupFX fx = GetComponent<CollectiblePickupFX>();
        if (fx != null) fx.PlayAndDestroy();
        else Destroy(gameObject);
    }
}
