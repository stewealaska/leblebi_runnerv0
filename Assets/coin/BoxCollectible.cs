using UnityEngine;

public class BoxCollectible : MonoBehaviour
{
    public int scoreValue = 1;
    public bool IsCollected { get; private set; }

    public void CollectAt(Vector3 worldPos)
    {
        if (IsCollected) return;
        IsCollected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddCoinScore(scoreValue);

        CollectiblePickupFX fx = GetComponent<CollectiblePickupFX>();
        if (fx != null)
        {
            // worldPos sadece SFX için de kullanýlýyor, VFX artýk player'ý takip edecek
            fx.PlayAtAndDestroy(worldPos);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Kutunun kendi pozisyonu (temiz)
        CollectAt(transform.position);
    }
}
