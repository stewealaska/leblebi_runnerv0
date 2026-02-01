using UnityEngine;

public class HeartCollectible : MonoBehaviour
{
    [Tooltip("Kaç can eklesin? Genelde 1.")]
    public int healAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance?.AddLife(healAmount);

        Destroy(gameObject);
    }
}
