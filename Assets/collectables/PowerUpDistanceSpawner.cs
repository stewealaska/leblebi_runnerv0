using UnityEngine;

public class PowerUpDistanceSpawner : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform player;

    [Header("Genel Spawn Ayarlarý")]
    public float laneDistance = 2.5f;
    public float spawnY = 1.0f;

    [Tooltip("Ýlk power-up ne kadar ilerden baþlasýn?")]
    public float startAhead = 60f;

    [Tooltip("Oyuncunun önünde bu mesafeye kadar üret")]
    public float keepAhead = 200f;

    [Header("Engellerle çakýþma kontrolü")]
    public LayerMask obstacleMask;
    public Vector3 checkHalfExtents = new Vector3(0.7f, 1f, 0.7f);

    // ===== MAGNET =====
    [Header("Magnet")]
    public GameObject magnetPrefab;

    [Tooltip("Magnet spawn denemesi aralýðý (metre).")]
    public Vector2 magnetSpacingRange = new Vector2(80f, 140f);

    [Tooltip("Her magnet denemesinde çýkma olasýlýðý.")]
    [Range(0f, 1f)] public float magnetChance = 0.08f;

    // ===== HEART =====
    [Header("Heart")]
    public GameObject heartPrefab;

    [Tooltip("Heart spawn denemesi aralýðý (metre).")]
    public Vector2 heartSpacingRange = new Vector2(120f, 200f);

    [Tooltip("Her heart denemesinde çýkma olasýlýðý.")]
    [Range(0f, 1f)] public float heartChance = 0.05f;

    // Internal spawn trackers
    private float nextMagnetZ;
    private float nextHeartZ;

    private static readonly Collider[] hits = new Collider[16];

    private void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            nextMagnetZ = player.position.z + startAhead;
            nextHeartZ = player.position.z + startAhead;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float targetZ = player.position.z + keepAhead;

        // --- Magnet spawn loop ---
        if (magnetPrefab != null)
        {
            while (nextMagnetZ < targetZ)
            {
                TrySpawn(magnetPrefab, magnetChance, nextMagnetZ);
                nextMagnetZ += Random.Range(magnetSpacingRange.x, magnetSpacingRange.y);
            }
        }

        // --- Heart spawn loop ---
        if (heartPrefab != null)
        {
            while (nextHeartZ < targetZ)
            {
                TrySpawn(heartPrefab, heartChance, nextHeartZ);
                nextHeartZ += Random.Range(heartSpacingRange.x, heartSpacingRange.y);
            }
        }
    }

    private void TrySpawn(GameObject prefab, float chance, float zPos)
    {
        if (prefab == null) return;

        // Olasýlýk kontrolü
        if (Random.value > chance) return;

        // Lane seç (0 sol, 1 orta, 2 sað)
        int lane = Random.Range(0, 3);
        float x = (lane - 1) * laneDistance;

        Vector3 pos = new Vector3(x, spawnY, zPos);

        // Engel çakýþmasý varsa koyma
        if (IsBlocked(pos)) return;

        Instantiate(prefab, pos, Quaternion.identity);
    }

    private bool IsBlocked(Vector3 pos)
    {
        int count = Physics.OverlapBoxNonAlloc(
            pos,
            checkHalfExtents,
            hits,
            Quaternion.identity,
            obstacleMask,
            QueryTriggerInteraction.Collide
        );

        return count > 0;
    }
}
