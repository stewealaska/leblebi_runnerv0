using UnityEngine;

public class ShieldVisualController : MonoBehaviour
{
    public GameObject shieldVfxPrefab;

    public Transform attachTargetOverride;

    public Vector3 localOffset = new Vector3(0f, 0.9f, 0f);
    public Vector3 localEuler = Vector3.zero;

    private GameObject shieldInstance;
    private GameManager gm;
    private bool lastActive = false;

    private void Start()
    {
        gm = GameManager.Instance;
        Debug.Log("ShieldVisualController Start. GM is " + (gm != null ? "OK" : "NULL"));
    }

    private void Update()
    {
        if (gm == null)
        {
            gm = GameManager.Instance;
            if (gm == null) return;
        }

        bool activeNow = gm.IsShieldActive;

        if (activeNow == lastActive) return;
        lastActive = activeNow;

        if (activeNow) ShowShield();
        else HideShield();
    }

    private void ShowShield()
    {
        Debug.Log("ShieldVisualController ShowShield");

        if (shieldVfxPrefab == null)
        {
            Debug.LogError("shieldVfxPrefab is NULL");
            return;
        }

        Transform target = attachTargetOverride != null ? attachTargetOverride : transform;

        if (shieldInstance == null)
        {
            shieldInstance = Instantiate(shieldVfxPrefab, target);
        }
        else
        {
            shieldInstance.transform.SetParent(target, false);
            shieldInstance.SetActive(true);
        }

        shieldInstance.transform.localPosition = localOffset;
        shieldInstance.transform.localRotation = Quaternion.Euler(localEuler);

        ParticleSystem ps = shieldInstance.GetComponentInChildren<ParticleSystem>(true);
        if (ps != null)
        {
            ps.Clear(true);
            ps.Play(true); 
        }
    }

    private void HideShield()
    {
        Debug.Log("ShieldVisualController HideShield");

        if (shieldInstance == null) return;

        ParticleSystem ps = shieldInstance.GetComponentInChildren<ParticleSystem>(true);
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        shieldInstance.SetActive(false);
    }
}
