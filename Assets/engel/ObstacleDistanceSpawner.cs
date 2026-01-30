using UnityEngine;

public class ObstacleDistanceSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject obstaclePrefab;

    [Header("Lanes")]
    public float laneDistance = 2.5f; // RunnerPlayer ile ayný
    public float obstacleY = 0.5f;

    [Header("Spawn Ahead")]
    public float startAhead = 50f;   // ilk engel ne kadar önde baþlasýn
    public float keepAhead = 250f;   // oyuncunun önünde bu mesafeye kadar üret

    [Header("Spacing")]
    public Vector2 spacingRange = new Vector2(30f, 70f);
    [Range(0f, 1f)] public float spawnChance = 0.7f;

    private float nextSpawnZ;

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
        if (player == null || obstaclePrefab == null) return;

        float targetZ = player.position.z + keepAhead;

        while (nextSpawnZ < targetZ)
        {
            if (Random.value <= spawnChance)
            {
                int lane = Random.Range(0, 3); // 0 sol, 1 orta, 2 sað
                float x = (lane - 1) * laneDistance;

                Vector3 pos = new Vector3(x, obstacleY, nextSpawnZ);
                Instantiate(obstaclePrefab, pos, Quaternion.identity);
            }

            nextSpawnZ += Random.Range(spacingRange.x, spacingRange.y);
        }
    }
}
