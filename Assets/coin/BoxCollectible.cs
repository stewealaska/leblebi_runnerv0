using UnityEngine;

public class BoxCollectible : MonoBehaviour
{
    public int scoreValue = 1;
    public bool IsCollected { get; private set; }

    public void Collect()
    {
        // Catcher uyumu: worldPos olmadan toplama
        CollectAt(transform.position);
    }

    public void CollectAt(Vector3 worldPos)
    {
        if (IsCollected) return;
        IsCollected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddCoinScore(scoreValue);

        CollectiblePickupFX fx = GetComponent<CollectiblePickupFX>();
        if (fx != null)
        {
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
        CollectAt(transform.position);
    }
}
