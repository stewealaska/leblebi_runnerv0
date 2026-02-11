using UnityEngine;

public class RoadTileSceneryAnchors : MonoBehaviour
{
    [Header("Assign in prefab")]
    public Transform leftSpawn;
    public Transform rightSpawn;

    public float TileStartZWorld
    {
        get { return transform.position.z; }
    }
}
