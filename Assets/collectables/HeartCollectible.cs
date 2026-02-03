using UnityEngine;

public class HeartCollectible : MonoBehaviour
{
    public int lifeAmount = 1;

    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.AddLife(lifeAmount);

        CollectiblePickupFX fx = GetComponent<CollectiblePickupFX>();
        if (fx == null) fx = GetComponentInChildren<CollectiblePickupFX>(true);
        if (fx == null) fx = GetComponentInParent<CollectiblePickupFX>();

        if (fx != null) fx.PlayAndDestroy();
        else Destroy(gameObject);
    }
}
