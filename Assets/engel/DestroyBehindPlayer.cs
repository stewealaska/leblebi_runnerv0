using UnityEngine;

public class DestroyBehindPlayer : MonoBehaviour
{
    [Tooltip("Boþsa Tag: Player'dan otomatik bulur.")]
    public Transform player;

    [Tooltip("Oyuncunun kaç metre arkasýna düþünce silinsin")]
    public float destroyBehindDistance = 30f;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Engelin Z'si oyuncudan yeterince geride kaldýysa sil
        if (transform.position.z < player.position.z - destroyBehindDistance)
        {
            Destroy(gameObject);
        }
    }
}
