using System.Collections.Generic;
using UnityEngine;

public class CollectibleCatcher : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Sadece collectible layer'larýný seç. (Coin, Shield, Magnet vb.)")]
    public LayerMask collectibleMask;

    [Tooltip("Toplama yarýçapý. Hýz yüksekse 0.6 - 1.2 arasý iyi.")]
    public float radius = 0.9f;

    [Tooltip("Merkez offset. CharacterController merkezine göre biraz yukarý almak iyi olur.")]
    public Vector3 centerOffset = new Vector3(0f, 1.0f, 0f);

    [Header("Performance")]
    [Tooltip("Bir karede en fazla kaç collider taransýn.")]
    public int maxHits = 32;

    private Collider[] hits;
    private readonly HashSet<int> consumedIds = new HashSet<int>(256);

    private void Awake()
    {
        hits = new Collider[Mathf.Max(8, maxHits)];
    }

    private void FixedUpdate()
    {
        Vector3 center = transform.position + centerOffset;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            radius,
            hits,
            collectibleMask,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            Collider c = hits[i];
            if (c == null) continue;

            int id = c.GetInstanceID();
            if (consumedIds.Contains(id)) continue;

            // Collectible objesinin root'u veya kendisi olabilir
            GameObject go = c.attachedRigidbody != null ? c.attachedRigidbody.gameObject : c.gameObject;

            // Birden fazla collider varsa tekrar tetiklenmesin
            consumedIds.Add(id);

            // En güvenlisi: Collect() varsa çaðýr
            // Yoksa mevcut scriptlerin OnTriggerEnter'ýna güvenmek yerine, SendMessage ile tetikle.
            // Collect fonksiyonun yoksa aþaðýdaki satýr boþa gider (hata vermez).
            go.SendMessage("Collect", SendMessageOptions.DontRequireReceiver);

            // Eðer sende coin/shield/magnet scriptleri direkt Destroy yapýyorsa ve Collect yoksa,
            // burada da güvenlik olarak objeyi yok edebilirsin. Ama önce Collect yapýsý daha temiz.
            // Destroy(go);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + centerOffset;
        Gizmos.DrawWireSphere(center, radius);
    }
}
