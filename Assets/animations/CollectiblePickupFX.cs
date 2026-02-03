using UnityEngine;

public class CollectiblePickupFX : MonoBehaviour
{
    [Header("VFX / SFX")]
    public ParticleSystem vfxPrefab;
    public AudioClip sfx;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("Follow Target")]
    [Tooltip("Açýk olursa VFX CollectPoint'e baðlanýr ve oyuncuyla birlikte gider.")]
    public bool followPlayer = true;

    [Tooltip("Boþsa otomatik olarak Player -> CollectPoint bulunur.")]
    public Transform followTargetOverride;

    [Tooltip("VFX'in CollectPoint'e göre offset'i (ayaklardan kurtarmak için burayý yükselt/öne al).")]
    public Vector3 followLocalOffset = new Vector3(0f, 0.3f, 0.25f);

    [Tooltip("VFX'in CollectPoint'e göre rotasyonu.")]
    public Vector3 followLocalEuler = Vector3.zero;

    public bool IsPlaying { get; private set; }

    public void PlayAndDestroy()
    {
        PlayAtAndDestroy(transform.position);
    }

    public void PlayAtAndDestroy(Vector3 worldPosForSfx)
    {
        if (IsPlaying) return;
        IsPlaying = true;

        // 1) Tekrar tetiklenmesin
        var cols = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;

        // 2) Obje ANINDA görünmez olsun
        var rends = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++) rends[i].enabled = false;

        // 3) VFX
        if (vfxPrefab != null)
        {
            Transform followTarget = followTargetOverride;

            if (followTarget == null && followPlayer)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                {
                    // KyleRobot altýnda CollectPoint varsa onu al
                    Transform cp = p.transform.Find("CollectPoint");
                    followTarget = (cp != null) ? cp : p.transform;
                }
            }

            ParticleSystem fx;

            if (followPlayer && followTarget != null)
            {
                // Önce world’de doðru yere koy
                fx = Instantiate(vfxPrefab, followTarget.position, followTarget.rotation);

                // Sonra parent et, offset uygula
                fx.transform.SetParent(followTarget, true);
                fx.transform.localPosition = followLocalOffset;
                fx.transform.localRotation = Quaternion.Euler(followLocalEuler);
            }
            else
            {
                // Fallback: world'de sabit
                fx = Instantiate(vfxPrefab, worldPosForSfx, Quaternion.identity);
            }

            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax + 0.2f);
        }

        // 4) SFX
        if (sfx != null)
            AudioSource.PlayClipAtPoint(sfx, worldPosForSfx, sfxVolume);

        // 5) Collectible objesini anýnda sil
        Destroy(gameObject);
    }
}
