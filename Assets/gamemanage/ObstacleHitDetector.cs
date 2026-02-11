using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ObstacleHitDetector : MonoBehaviour
{
    public float topHitThreshold = 0.5f;
    public string obstacleTag = "Obstacle";
    public int damage = 1;
    public bool destroyRootObject = true;

    [Header("Çarpýþma Sesi")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.CompareTag(obstacleTag)) return;
        if (hit.normal.y > topHitThreshold) return;
        if (GameManager.Instance == null) return;

        GameObject obstacleObj = destroyRootObject ? hit.collider.transform.root.gameObject : hit.collider.gameObject;

        // Önce sesi çal (engel yok olsa bile ses gelir)
        PlayHitSound(hitClip);

        GameManager.Instance.TryHandleObstacleHit(damage, obstacleObj);
    }

    private void PlayHitSound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, volume);
    }
}
