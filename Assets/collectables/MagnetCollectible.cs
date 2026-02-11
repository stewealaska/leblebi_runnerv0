using UnityEngine;

public class MagnetCollectible : MonoBehaviour
{
    public float duration = 10f;
    public float radius = 10f;
    public float speed = 18f;

    private bool collected = false;

    public void Collect()
    {
        if (collected) return;
        collected = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            MagnetAttractor attractor = player.GetComponentInParent<MagnetAttractor>();
            if (attractor == null) attractor = player.GetComponent<MagnetAttractor>();

            if (attractor != null)
                attractor.ActivateMagnet(duration, radius, speed);
        }

        CollectiblePickupFX fx = GetComponent<CollectiblePickupFX>();
        if (fx != null) fx.PlayAndDestroy();
        else Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Trigger yolunda oyuncu colliderýndan çekmek daha güvenli
        MagnetAttractor attractor = other.GetComponentInParent<MagnetAttractor>();
        if (attractor != null)
            attractor.ActivateMagnet(duration, radius, speed);

        Collect();
    }
}
