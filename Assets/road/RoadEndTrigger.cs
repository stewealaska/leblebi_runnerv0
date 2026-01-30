using UnityEngine;

public class RoadEndTrigger : MonoBehaviour
{
    private EndlessRoad road;
    private Transform tileRoot;

    private bool triggered = false;

    private void Awake()
    {
        road = Object.FindFirstObjectByType<EndlessRoad>();
        tileRoot = transform.root;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        if (road == null)
        {
            Debug.LogError("RoadEndTrigger: EndlessRoad bulunamadý! Sahnede RoadManager objesinde EndlessRoad var mý?");
            return;
        }

        triggered = true;
        road.RecycleTile(tileRoot.gameObject);
    }

    // Tile recycle edilince tekrar kullanýlabilsin diye çaðýracaðýz
    public void ResetTrigger()
    {
        triggered = false;
    }
}
