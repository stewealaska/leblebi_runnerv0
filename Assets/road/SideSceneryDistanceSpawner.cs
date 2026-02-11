using System.Collections.Generic;
using UnityEngine;

public class SideSceneryDistanceSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Tile'larýn root parent'ý. (Tile'larý EndlessRoad altýnda üretiyorsan onun Transform'u)")]
    public Transform roadTilesRoot;

    [Tooltip("Binalarýn parent'ý (sahnede düzen için).")]
    public Transform parentRoot;

    [Header("Tracking")]
    [Tooltip("Z takibini hangi referansla yapalým? Genelde Player Z mantýklý. X'e baðlanmaz.")]
    public Transform zReference;

    [Header("Prefabs")]
    public List<GameObject> buildingPrefabs = new List<GameObject>();

    [Header("Initial Fill")]
    public float startBehind = 30f;
    public float startAhead = 40f;

    [Header("Runtime Spawn")]
    public float spawnAhead = 120f;
    public float spacingZ = 6f;
    public float randomZJitter = 0f;

    [Header("Rotation")]
    public bool useSpawnRotation = true;
    public float extraYaw = 0f;

    [Header("Cleanup")]
    public float destroyBehindDistance = 80f;

    [Header("Debug")]
    public bool debugLogs = false;
    [Tooltip("Anchor taramasýný kaç saniyede bir yenilesin? (0.25-1.0 iyi)")]
    public float refreshInterval = 0.5f;

    private readonly List<RoadTileSceneryAnchors> anchors = new List<RoadTileSceneryAnchors>();
    private float nextZ;
    private bool initialized = false;
    private float nextRefreshTime = 0f;

    private void Start()
    {
        // Start'ta hemen dene ama tile'lar daha oluþmamýþ olabilir.
        TryRefreshAnchors();
        TryInitialize();
    }

    private void Update()
    {
        // Z ref yoksa hiçbir þey yapamayýz
        if (zReference == null) return;

        // Tile'lar recycle olduðu için anchor listesini periyodik yenile
        if (Time.unscaledTime >= nextRefreshTime)
        {
            nextRefreshTime = Time.unscaledTime + Mathf.Max(0.05f, refreshInterval);
            TryRefreshAnchors();
        }

        // Baþlatýlamadýysa, anchor geldiði anda tekrar baþlat
        if (!initialized)
        {
            TryInitialize();
            return;
        }

        float targetZ = zReference.position.z + spawnAhead;

        while (nextZ <= targetZ)
        {
            SpawnPairAtZ(nextZ);
            nextZ += spacingZ;
        }

        CleanupBehind();
    }

    private void TryInitialize()
    {
        if (initialized) return;

        if (roadTilesRoot == null)
        {
            if (debugLogs) Debug.LogWarning("SideSceneryDistanceSpawner: roadTilesRoot boþ.");
            return;
        }

        if (buildingPrefabs == null || buildingPrefabs.Count == 0)
        {
            if (debugLogs) Debug.LogWarning("SideSceneryDistanceSpawner: buildingPrefabs boþ.");
            return;
        }

        if (anchors.Count == 0)
        {
            if (debugLogs) Debug.LogWarning("SideSceneryDistanceSpawner: anchors bulunamadý (tile'lar daha oluþmamýþ olabilir).");
            return;
        }

        float startZ = zReference.position.z - startBehind;
        float endZ = zReference.position.z + startAhead;

        float z = startZ;
        while (z <= endZ)
        {
            SpawnPairAtZ(z);
            z += spacingZ;
        }

        nextZ = endZ + spacingZ;
        initialized = true;

        if (debugLogs) Debug.Log("SideSceneryDistanceSpawner: Initialize tamam.");
    }

    private void TryRefreshAnchors()
    {
        anchors.Clear();

        if (roadTilesRoot == null) return;

        RoadTileSceneryAnchors[] found = roadTilesRoot.GetComponentsInChildren<RoadTileSceneryAnchors>(true);
        if (found == null || found.Length == 0)
        {
            return;
        }

        for (int i = 0; i < found.Length; i++)
        {
            RoadTileSceneryAnchors a = found[i];
            if (a == null) continue;
            if (a.leftSpawn == null || a.rightSpawn == null) continue;

            anchors.Add(a);
        }

        anchors.Sort((a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

        if (debugLogs) Debug.Log("SideSceneryDistanceSpawner: anchors=" + anchors.Count);
    }

    private void SpawnPairAtZ(float z)
    {
        RoadTileSceneryAnchors a = FindAnchorForZ(z);
        if (a == null) return;

        float jitter = (randomZJitter > 0f) ? Random.Range(-randomZJitter, randomZJitter) : 0f;
        float finalZ = z + jitter;

        SpawnOne(a.leftSpawn, finalZ);
        SpawnOne(a.rightSpawn, finalZ);
    }

    private RoadTileSceneryAnchors FindAnchorForZ(float z)
    {
        if (anchors.Count == 0) return null;

        RoadTileSceneryAnchors best = null;

        // Z sýralý listede, z'den küçük/eþit en son anchor
        for (int i = 0; i < anchors.Count; i++)
        {
            RoadTileSceneryAnchors a = anchors[i];
            if (a == null) continue;

            float az = a.transform.position.z;
            if (az <= z) best = a;
            else break;
        }

        if (best == null) best = anchors[0];
        return best;
    }

    private void SpawnOne(Transform spawnPoint, float z)
    {
        if (spawnPoint == null) return;
        if (buildingPrefabs == null || buildingPrefabs.Count == 0) return;

        GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Count)];
        if (prefab == null) return;

        Vector3 pos = spawnPoint.position;
        pos.z = z;

        Quaternion rot = useSpawnRotation ? spawnPoint.rotation : Quaternion.identity;
        if (extraYaw != 0f) rot = rot * Quaternion.Euler(0f, extraYaw, 0f);

        GameObject obj = Instantiate(prefab, pos, rot);

        if (parentRoot != null)
            obj.transform.SetParent(parentRoot, true);
    }

    private void CleanupBehind()
    {
        if (parentRoot == null) return;

        float minZ = zReference.position.z - destroyBehindDistance;

        for (int i = parentRoot.childCount - 1; i >= 0; i--)
        {
            Transform ch = parentRoot.GetChild(i);
            if (ch != null && ch.position.z < minZ)
                Destroy(ch.gameObject);
        }
    }
}
