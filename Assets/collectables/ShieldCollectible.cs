using UnityEngine;

public class ShieldCollectible : MonoBehaviour
{
    public float duration = 8f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("SHIELD PICKED UP");

        if (GameManager.Instance != null)
        {
            Debug.Log("GameManager.Instance OK, calling ActivateShield");
            GameManager.Instance.ActivateShield(duration);
        }
        else
        {
            Debug.LogError("GameManager.Instance is NULL");
        }

        CollectiblePickupFX fx = GetComponent<CollectiblePickupFX>();
        if (fx != null) fx.PlayAndDestroy();
        else Destroy(gameObject);
    }
}
