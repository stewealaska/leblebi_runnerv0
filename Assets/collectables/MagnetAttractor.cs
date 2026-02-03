using UnityEngine;

public class MagnetAttractor : MonoBehaviour
{
    [SerializeField] private bool magnetActive = false;
    [SerializeField] private float magnetEndTime = 0f;

    public float attractRadius = 14f;
    public float attractSpeed = 45f;

    public float autoCollectDistance = 3.2f;

    public Transform pullTarget;
    public LayerMask collectibleMask;

    private static readonly Collider[] hits = new Collider[128];

    void Awake()
    {
        // Pull target verilmezse root yerine kendisini alsýn
        if (pullTarget == null) pullTarget = transform;
    }

    void Update()
    {
        if (!magnetActive) return;

        float remaining = magnetEndTime - Time.time;
        if (remaining <= 0f)
        {
            magnetActive = false;
            GameManager.Instance?.SetMagnetTimer(0f);
            return;
        }

        GameManager.Instance?.SetMagnetTimer(remaining);

        Vector3 center = pullTarget.position;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            attractRadius,
            hits,
            collectibleMask,
            QueryTriggerInteraction.Collide
        );

        float step = attractSpeed * Time.deltaTime;

        for (int i = 0; i < count; i++)
        {
            Collider c = hits[i];
            if (c == null) continue;

            BoxCollectible box = c.GetComponentInParent<BoxCollectible>();
            if (box == null) continue;
            if (box.IsCollected) continue;

            CollectiblePickupFX fx = box.GetComponent<CollectiblePickupFX>();
            if (fx != null && fx.IsPlaying) continue;

            Transform t = box.transform;

            // FIX: Y eksenini yok say (XZ mesafe)
            Vector3 flat = t.position - center;
            flat.y = 0f;
            float dist = flat.magnitude;

            if (dist <= autoCollectDistance)
            {
                box.CollectAt(center);
                continue;
            }

            t.position = Vector3.MoveTowards(t.position, center, step);
        }
    }

    public void ActivateMagnet(float duration, float radius, float speed)
    {
        attractRadius = radius;
        attractSpeed = speed;

        magnetActive = true;
        magnetEndTime = Time.time + duration;

        GameManager.Instance?.SetMagnetTimer(duration);
    }
}
