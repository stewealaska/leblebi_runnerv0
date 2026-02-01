using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ObstacleHitDetector : MonoBehaviour
{
    [Tooltip("normal.y bu deðerden büyükse 'üstten temas' sayýlýr (can gitmez).")]
    public float topHitThreshold = 0.5f;

    [Tooltip("Engellerin tag'i. Ýstersen Obstacle yap.")]
    public string obstacleTag = "Obstacle";

    [Tooltip("Çarpýnca kaç can gitsin?")]
    public int damage = 1;

    [Header("Destroy On Damage")]
    [Tooltip("Hasar verdiðin engeli yok etsin mi?")]
    public bool destroyObstacleOnDamage = true;

    [Tooltip("Engeli yok etme gecikmesi (anim/VFX için).")]
    public float destroyDelay = 0f;

    [Tooltip("True: collider'ýn ROOT objesini siler (genelde prefab komple gider). False: sadece collider objesini siler.")]
    public bool destroyRootObject = true;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.CompareTag(obstacleTag)) return;

        // Üstten temas: can gitmesin
        if (hit.normal.y > topHitThreshold) return;

        if (GameManager.Instance == null) return;

        // Hasar gerçekten uygulandý mý?
        bool applied = GameManager.Instance.TryTakeDamage(damage);

        // Hasar uygulandýysa engeli kaldýr
        if (applied && destroyObstacleOnDamage)
        {
            GameObject target = destroyRootObject ? hit.collider.transform.root.gameObject : hit.collider.gameObject;
            Destroy(target, destroyDelay);
        }
    }
}
