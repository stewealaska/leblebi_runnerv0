using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    public int value = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.AddCoinScore(value);

        Destroy(gameObject);
    }
}
