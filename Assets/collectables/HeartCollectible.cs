using UnityEngine;

public class HeartCollectible : MonoBehaviour
{
    public int lifeAmount = 1;

    private bool collected = false;

    public void Collect()
    {
        if (collected) return;
        collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddLife(lifeAmount);

        CollectiblePickupFX fx = GetComponent<CollectiblePickupFX>();
        if (fx == null) fx = GetComponentInChildren<CollectiblePickupFX>(true);
        if (fx == null) fx = GetComponentInParent<CollectiblePickupFX>();

        if (fx != null) fx.PlayAndDestroy();
        else Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Collect();
    }
}
