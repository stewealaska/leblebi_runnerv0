using UnityEngine;

public class CoinDistanceSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject coinPrefab;

    [Header("Lanes")]
    public float laneDistance = 2.5f;
    public float coinY = 1.0f;

    [Header("Spawn Ahead")]
    public float startAhead = 25f;
    public float keepAhead = 200f;

    [Header("Coin Trail (3 in a row)")]
    public int trailSize = 3;
    public float coinGap = 2.0f;

    [Header("Spacing (between trails)")]
    public Vector2 spacingRange = new Vector2(15f, 35f);
    [Range(0f, 1f)] public float spawnChance = 0.9f;

    [Header("No overlap with obstacles")]
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
        if (player == null || coinPrefab == null) return;

        float targetZ = player.position.z + keepAhead;

        while (nextSpawnZ < targetZ)
        {
            if (Random.value <= spawnChance)
            {
                // Trail için bir lane seç (istersen her coin ayrý lane de seçilebilir)
                int lane = Random.Range(0, 3);
                float x = (lane - 1) * laneDistance;

                for (int i = 0; i < trailSize; i++)
                {
                    float z = nextSpawnZ + i * coinGap;
                    Vector3 pos = new Vector3(x, coinY, z);

                    // Her coin ayrý ayrý kontrol
                    if (!IsBlocked(pos))
                    {
                        Instantiate(coinPrefab, pos, Quaternion.Euler(0f, 90f, 90f));
                    }
                    // engel varsa bu coin'i atla
                }
            }

            nextSpawnZ += Random.Range(spacingRange.x, spacingRange.y);
        }
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
