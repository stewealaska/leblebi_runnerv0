using UnityEngine;

public class BoxCollectible : MonoBehaviour
{
    [Tooltip("Mavi=1, Turuncu=3, Neon=5")]
    public int value = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.AddCoinScore(value);

        Destroy(gameObject);
    }
}
