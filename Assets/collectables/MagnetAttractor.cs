using UnityEngine;

public class MagnetAttractor : MonoBehaviour
{
    [Header("Magnet State (Read Only)")]
    [SerializeField] private bool magnetActive = false;
    [SerializeField] private float magnetEndTime = 0f;

    [Header("Attract Settings")]
    public float attractRadius = 10f;
    public float attractSpeed = 18f;

    [Tooltip("Sadece bu layer'daki collectible'larý çeker (Collectible layer önerilir).")]
    public LayerMask collectibleMask;

    [Tooltip("Kutular oyuncunun bu noktasýna çekilir. Boþsa kendi transform'u kullanýlýr.")]
    public Transform pullTarget;

    private static readonly Collider[] hits = new Collider[64];

    private void Awake()
    {
        if (pullTarget == null) pullTarget = transform;
    }

    private void Update()
    {
        if (!magnetActive) return;

        float remaining = magnetEndTime - Time.time;

        // Süre bitti
        if (remaining <= 0f)
        {
            magnetActive = false;
            GameManager.Instance?.SetMagnetTimer(0f);
            return;
        }

        // UI güncelle
        GameManager.Instance?.SetMagnetTimer(remaining);

        // Yakýndaki collectible collider'larýný bul
        Vector3 center = pullTarget.position;
        int count = Physics.OverlapSphereNonAlloc(
            center,
            attractRadius,
            hits,
            collectibleMask,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            Collider c = hits[i];
            if (c == null) continue;

            // Collider child objede olabilir -> parent'ta collectible scriptini ara
            var box = c.GetComponentInParent<BoxCollectible>();
            if (box != null)
            {
                Transform t = box.transform;
                t.position = Vector3.MoveTowards(t.position, center, attractSpeed * Time.deltaTime);
                continue;
            }

            // Eski coin sistemi varsa destekle
            var coin = c.GetComponentInParent<CoinCollectible>();
            if (coin != null)
            {
                Transform t = coin.transform;
                t.position = Vector3.MoveTowards(t.position, center, attractSpeed * Time.deltaTime);
                continue;
            }
        }
    }

    public void ActivateMagnet(float duration, float radius, float speed)
    {
        attractRadius = radius;
        attractSpeed = speed;

        magnetActive = true;
        magnetEndTime = Time.time + duration;

        // Daha ilk frame'de UI gözüksün
        GameManager.Instance?.SetMagnetTimer(duration);
    }

    private void OnDrawGizmosSelected()
    {
        Transform t = pullTarget != null ? pullTarget : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(t.position, attractRadius);
    }
}
