using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class RunnerPlayer : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Movement")]
    public float forwardSpeed = 8f;
    public float laneDistance = 2.5f;
    public float laneChangeSpeed = 12f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.4f;
    public float gravity = -25f;
    public float groundStickForce = -2f;

    [Header("Ground Check")]
    public LayerMask groundMask = ~0;

    [Header("Invulnerability Visual (Blink)")]
    [Tooltip("Yanýp sönme aralýðý (saniye). 0.08–0.15 iyi aralýk.")]
    public float blinkInterval = 0.10f;

    private CharacterController cc;

    private int targetLane = 1;  // 0 = sol, 1 = orta, 2 = sað
    private float verticalVelocity;

    // Blink internals
    private Renderer[] cachedRenderers;
    private Coroutine blinkRoutine;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        cachedRenderers = GetComponentsInChildren<Renderer>(true);
    }

    void Start()
    {
        targetLane = 1;
        verticalVelocity = 0f;

        if (animator != null)
            animator.SetBool("isJumping", false);

        // Güvenlik: sahne baþýnda görünür olsun
        SetRenderersVisible(true);
    }

    void Update()
    {
        // Oyun baþlamadýysa karakter hiçbir þey yapmasýn
        if (GameManager.Instance != null && !GameManager.Instance.GameStarted)
            return;

        // === INPUT ===
        bool leftPressed = Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame;
        bool rightPressed = Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame;
        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (leftPressed) targetLane = Mathf.Max(0, targetLane - 1);
        if (rightPressed) targetLane = Mathf.Min(2, targetLane + 1);

        // === GROUND CHECK ===
        bool grounded = IsGroundedReliable();

        if (grounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundStickForce;

            if (animator != null)
                animator.SetBool("isJumping", false);
        }

        // === JUMP ===
        if (grounded && jumpPressed)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetBool("isJumping", true);
        }

        // === GRAVITY ===
        verticalVelocity += gravity * Time.deltaTime;

        // === LANE MOVEMENT ===
        float targetX = (targetLane - 1) * laneDistance;
        float newX = Mathf.Lerp(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        float xMove = newX - transform.position.x;

        // === FORWARD ===
        float speed = (GameManager.Instance != null) ? GameManager.Instance.CurrentSpeed : forwardSpeed;
        float zMove = speed * Time.deltaTime;

        // === FINAL MOVE ===
        Vector3 motion = new Vector3(xMove, verticalVelocity * Time.deltaTime, zMove);
        cc.Move(motion);
    }

    private bool IsGroundedReliable()
    {
        Vector3 origin = cc.bounds.center;
        float radius = cc.radius * 0.9f;
        float distance = cc.bounds.extents.y + 0.2f;

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out _,
            distance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    void OnDrawGizmosSelected()
    {
        if (cc == null) return;

        Gizmos.color = Color.yellow;
        Vector3 origin = cc.bounds.center;
        float radius = cc.radius * 0.9f;
        float distance = cc.bounds.extents.y + 0.2f;
        Gizmos.DrawWireSphere(origin + Vector3.down * distance, radius);
    }

    // ===== TAKILMA ÇÖZÜM ARAÇLARI =====

    public void SetCollisionEnabled(bool enabled)
    {
        if (cc == null) cc = GetComponent<CharacterController>();
        cc.detectCollisions = enabled;
    }

    public void NudgeForward(float forward = 2.0f, float up = 0.2f)
    {
        if (cc == null) cc = GetComponent<CharacterController>();
        cc.Move(new Vector3(0f, up, forward));
    }

    // ===== BLINK (INVULNERABILITY VISUAL) =====

    public void StartInvulnerabilityBlink(float duration)
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkCoroutine(duration));
    }

    public void StopInvulnerabilityBlink()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        // Bitince kesin görünür býrak
        SetRenderersVisible(true);
    }

    private IEnumerator BlinkCoroutine(float duration)
    {
        float end = Time.time + duration;
        bool visible = true;

        // Süre boyunca toggle
        while (Time.time < end)
        {
            visible = !visible;
            SetRenderersVisible(visible);
            yield return new WaitForSeconds(blinkInterval);
        }

        // Final: görünür
        SetRenderersVisible(true);
        blinkRoutine = null;
    }

    private void SetRenderersVisible(bool visible)
    {
        if (cachedRenderers == null || cachedRenderers.Length == 0)
            cachedRenderers = GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
                cachedRenderers[i].enabled = visible;
        }
    }
}
