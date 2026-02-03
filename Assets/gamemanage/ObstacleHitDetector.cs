using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ObstacleHitDetector : MonoBehaviour
{
    public float topHitThreshold = 0.5f;
    public string obstacleTag = "Obstacle";
    public int damage = 1;

    [Tooltip("Collider'ýn ROOT objesini yok eder (genelde tüm prefab).")]
    public bool destroyRootObject = true;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.CompareTag(obstacleTag)) return;

        // Üstten temas: can gitmesin
        if (hit.normal.y > topHitThreshold) return;

        if (GameManager.Instance == null) return;

        GameObject obstacleObj = destroyRootObject ? hit.collider.transform.root.gameObject : hit.collider.gameObject;

        // Kalkan varsa can gitmez + engel yok olur
        GameManager.Instance.TryHandleObstacleHit(damage, obstacleObj);
    }
}
