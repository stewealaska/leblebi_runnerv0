using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ObstacleHitDetector : MonoBehaviour
{
    [Tooltip("normal.y bu deðerden büyükse 'üstten temas' sayýlýr (ölme yok).")]
    public float topHitThreshold = 0.5f;

    [Tooltip("Engellerin tag'i. Ýstersen Obstacle yap.")]
    public string obstacleTag = "Obstacle";

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Sadece engellere çarpýnca çalýþsýn
        if (!hit.collider.CompareTag(obstacleTag)) return;

        // Üstten temas: ölme (engel üstünde koþ)
        if (hit.normal.y > topHitThreshold) return;

        // Yan/ön temas: game over
        GameManager.Instance?.GameOver();
    }
}
