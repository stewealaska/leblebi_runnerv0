using UnityEngine;

public class FootstepReceiver : MonoBehaviour
{
    [Header("Footstep Clips")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Landing Clips (OnLand)")]
    [SerializeField] private AudioClip[] landClips;

    [Header("Ayarlar")]
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.35f;
    [SerializeField, Range(0f, 1f)] private float landVolume = 0.45f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 0 = 2D, istersen 3D yaparýz
    }

    //  Run animasyonundaki event bunu çaðýrýyor
    public void OnFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, footstepVolume);
    }

    //  JumpLand animasyonundaki event bunu çaðýrýyor (hata bunun yüzünden)
    public void OnLand()
    {
        // Eðer özel iniþ sesi yoksa, footstep'ten bir tane kullanýp yine de hatayý yok eder
        AudioClip clip = null;

        if (landClips != null && landClips.Length > 0)
            clip = landClips[Random.Range(0, landClips.Length)];
        else if (footstepClips != null && footstepClips.Length > 0)
            clip = footstepClips[Random.Range(0, footstepClips.Length)];

        if (clip == null) return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, landVolume);
    }
}
