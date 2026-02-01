using UnityEngine;

public class CoinDistanceSpawner : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform player;

    [Tooltip("Sýra önemli: 0=Mavi, 1=Turuncu, 2=Neon")]
    public GameObject[] boxPrefabs;

    [Header("Çýkma Sýklýðý (Weighted %)")]
    [Range(0f, 100f)] public float blueChance = 70f;    // Mavi
    [Range(0f, 100f)] public float orangeChance = 25f;  // Turuncu
    [Range(0f, 100f)] public float neonChance = 5f;     // Neon

    [Header("Þeritler")]
    public float laneDistance = 2.5f;
    public float spawnY = 1.0f;

    [Header("Önde Üretim")]
    public float startAhead = 25f;
    public float keepAhead = 200f;

    [Header("Trail (3 tane art arda)")]
    public int trailSize = 3;
    public float gap = 2.0f;

    [Header("Trail'ler arasý mesafe")]
    public Vector2 spacingRange = new Vector2(15f, 35f);
    [Range(0f, 1f)] public float spawnChance = 0.9f;

    [Header("Engellerle çakýþmasýn")]
    public LayerMask obstacleMask;
    public Vector3 checkHalfExtents = new Vector3(0.7f, 1.2f, 0.7f);
    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    private float nextSpawnZ;
    private static readonly Collider[] hits = new Collider[16];

    private void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
            nextSpawnZ = player.position.z + startAhead;
    }

    private void Update()
    {
        if (player == null) return;
        if (boxPrefabs == null || boxPrefabs.Length < 3) return;

        float targetZ = player.position.z + keepAhead;

        while (nextSpawnZ < targetZ)
        {
            if (Random.value <= spawnChance)
            {
                int lane = Random.Range(0, 3);
                float x = (lane - 1) * laneDistance;

                for (int i = 0; i < trailSize; i++)
                {
                    float z = nextSpawnZ + i * gap;
                    Vector3 pos = new Vector3(x, spawnY, z);

                    if (!IsBlocked(pos))
                    {
                        GameObject prefab = GetWeightedBox();
                        if (prefab != null)
                            Instantiate(prefab, pos, Quaternion.identity);
                    }
                }
            }

            nextSpawnZ += Random.Range(spacingRange.x, spacingRange.y);
        }
    }

    private GameObject GetWeightedBox()
    {
        // Güvenlik: oranlar 0 olursa veya toplam 0 olursa maviye düþ
        float total = Mathf.Max(0f, blueChance) + Mathf.Max(0f, orangeChance) + Mathf.Max(0f, neonChance);
        if (total <= 0.0001f) return boxPrefabs[0];

        float r = Random.value * total;

        if (r < blueChance) return boxPrefabs[0];
        r -= blueChance;

        if (r < orangeChance) return boxPrefabs[1];

        return boxPrefabs[2];
    }

    private bool IsBlocked(Vector3 pos)
    {
        int count = Physics.OverlapBoxNonAlloc(
            pos,
            checkHalfExtents,
            hits,
            Quaternion.identity,
            obstacleMask,
            triggerInteraction
        );

        return count > 0;
    }
}
