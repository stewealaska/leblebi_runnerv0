using UnityEngine;

public class ObstacleGameOver : MonoBehaviour
{
    [Tooltip("Player engelin üstüne düþtüyse ölmesin. Bu deðerin üstü 'üstten temas' sayýlýr.")]
    public float topHitThreshold = 0.5f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.gameObject.CompareTag("Player")) return;

        // Normal, çarpýþma yönünü verir.
        // hit.normal.y yüksekse, player engelin üst yüzeyine temas ediyor demektir.
        // Önden/yanlardan çarpýnca normal.y genelde 0'a yakýn olur.
        if (hit.normal.y > topHitThreshold)
        {
            // Üstten temas: ölme
            return;
        }

        // Önden/yanlardan temas: Game Over
        GameManager.Instance?.GameOver();
    }
}
