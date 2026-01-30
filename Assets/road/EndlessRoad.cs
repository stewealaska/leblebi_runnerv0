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
        if (tilePrefab == null)
        {
            Debug.LogError("EndlessRoad: tilePrefab boþ!");
            enabled = false;
            return;
        }

        // Baþtan temiz baþla
        tiles.Clear();
        nextSpawnZ = 0f;

        for (int i = 0; i < tilesOnScreen; i++)
            SpawnTile();
    }

    private void SpawnTile()
    {
        // Local eksende diziyoruz
        Vector3 localPos = new Vector3(0f, 0f, nextSpawnZ);

        GameObject t = Instantiate(tilePrefab, transform);
        t.transform.localPosition = localPos;
        t.transform.localRotation = Quaternion.identity;

        tiles.Enqueue(t);
        nextSpawnZ += tileLength;
    }

    public void RecycleTile(GameObject tile)
    {
        if (tile == null) return;

        // Local eksende arkaya taþý
        tile.transform.localPosition = new Vector3(0f, 0f, nextSpawnZ);
        tile.transform.localRotation = Quaternion.identity;
        nextSpawnZ += tileLength;

        if (tiles.Count > 0)
        {
            tiles.Dequeue();
            tiles.Enqueue(tile);
        }

        var trig = tile.GetComponentInChildren<RoadEndTrigger>(true);
        if (trig != null) trig.ResetTrigger();
    }
}
