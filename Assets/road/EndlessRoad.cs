using System.Collections.Generic;
using UnityEngine;

public class EndlessRoad : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject tilePrefab;

    [Header("Settings")]
    public int tilesOnScreen = 8;
    public float tileLength = 75f;

    [Header("Failsafe")]
    public float recycleAheadDistance = 20f;

    private readonly Queue<GameObject> tiles = new Queue<GameObject>();
    private float nextSpawnZ = 0f;

    private void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Start()
    {
        if (tilePrefab == null) return;

        // Ýlk kurulum
        if (tiles.Count == 0)
        {
            BuildInitialTiles(0f);
        }
    }

    private void Update()
    {
        if (player == null) return;
        if (tiles.Count == 0) return;

        float needZ = player.position.z + recycleAheadDistance;

        while (nextSpawnZ < needZ + tileLength)
        {
            GameObject oldest = tiles.Dequeue();
            if (oldest == null) break;

            oldest.transform.localPosition = new Vector3(0f, 0f, nextSpawnZ);
            oldest.transform.localRotation = Quaternion.identity;
            nextSpawnZ += tileLength;

            RoadEndTrigger trig = oldest.GetComponentInChildren<RoadEndTrigger>(true);
            if (trig != null) trig.ResetTrigger();

            tiles.Enqueue(oldest);
        }
    }

    public void RecycleTile(GameObject tile)
    {
        if (tile == null) return;

        tile.transform.localPosition = new Vector3(0f, 0f, nextSpawnZ);
        tile.transform.localRotation = Quaternion.identity;
        nextSpawnZ += tileLength;

        if (tiles.Count > 0)
        {
            tiles.Dequeue();
            tiles.Enqueue(tile);
        }

        RoadEndTrigger trig = tile.GetComponentInChildren<RoadEndTrigger>(true);
        if (trig != null) trig.ResetTrigger();
    }

    // =========================
    // RESTART ÝÇÝN RESET
    // =========================
    public void ResetRoadToStart(float startLocalZ = 0f)
    {
        if (tiles.Count == 0)
        {
            // Hiç tile yoksa yeniden kur
            BuildInitialTiles(startLocalZ);
            return;
        }

        nextSpawnZ = startLocalZ;

        // Kuyruðu sýrayla alýp yeniden diziyoruz
        int count = tiles.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject t = tiles.Dequeue();
            if (t == null) continue;

            t.transform.localPosition = new Vector3(0f, 0f, nextSpawnZ);
            t.transform.localRotation = Quaternion.identity;
            nextSpawnZ += tileLength;

            RoadEndTrigger trig = t.GetComponentInChildren<RoadEndTrigger>(true);
            if (trig != null) trig.ResetTrigger();

            tiles.Enqueue(t);
        }
    }

    private void BuildInitialTiles(float startLocalZ)
    {
        nextSpawnZ = startLocalZ;

        for (int i = 0; i < tilesOnScreen; i++)
        {
            GameObject t = Instantiate(tilePrefab, transform);
            t.transform.localPosition = new Vector3(0f, 0f, nextSpawnZ);
            t.transform.localRotation = Quaternion.identity;

            RoadEndTrigger trig = t.GetComponentInChildren<RoadEndTrigger>(true);
            if (trig != null) trig.ResetTrigger();

            tiles.Enqueue(t);
            nextSpawnZ += tileLength;
        }
    }
}
