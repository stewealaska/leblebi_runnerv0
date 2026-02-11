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
    public float laneDistance = 2f;
    public float laneChangeSpeed = 16f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.4f;
    public float gravity = -25f;
    public float groundStickForce = -2f;

    [Header("Ground Check")]
    public LayerMask groundMask = ~0;

    [Header("Invulnerability Visual (Blink)")]
    [Tooltip("Yanýp sönme aralýðý (saniye). 0.08–0.15 iyi aralýk.")]
    public float blinkInterval = 0.10f;

    [Header("Swipe Settings")]
    [Tooltip("Swipe algýlama eþiði (piksel). 60–120 arasý iyi.")]
    public float swipeThresholdPixels = 90f;

    [Tooltip("Çapraz swipe yanlýþ algýlanmasýn diye eksen baskýnlýk oraný. 1.2 iyi.")]
    public float dominantAxisRatio = 1.2f;

    [Tooltip("Editörde mouse ile swipe testini aç/kapat.")]
    public bool enableMouseSwipeInEditor = true;

    private CharacterController cc;

    private int targetLane = 1;  // 0 = sol, 1 = orta, 2 = sað
    private float verticalVelocity;

    // Blink internals
    private Renderer[] cachedRenderers;
    private Coroutine blinkRoutine;

    // Swipe internals
    private Vector2 swipeStartPos;
    private bool swipeTracking;
    private bool swipeConsumed; // Bu dokunuþta swipe tetiklendi mi?

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

        SetRenderersVisible(true);
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.GameStarted)
            return;

        bool leftPressed = Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame;
        bool rightPressed = Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame;
        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        HandleSwipeInput(ref leftPressed, ref rightPressed, ref jumpPressed);

        if (leftPressed) targetLane = Mathf.Max(0, targetLane - 1);
        if (rightPressed) targetLane = Mathf.Min(2, targetLane + 1);

        bool grounded = IsGroundedReliable();

        if (grounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundStickForce;

            if (animator != null)
                animator.SetBool("isJumping", false);
        }

        if (grounded && jumpPressed)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetBool("isJumping", true);
        }

        verticalVelocity += gravity * Time.deltaTime;

        float targetX = (targetLane - 1) * laneDistance;
        float newX = Mathf.Lerp(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        float xMove = newX - transform.position.x;

        float speed = (GameManager.Instance != null) ? GameManager.Instance.CurrentSpeed : forwardSpeed;
        float zMove = speed * Time.deltaTime;

        Vector3 motion = new Vector3(xMove, verticalVelocity * Time.deltaTime, zMove);
        cc.Move(motion);
    }

    private void HandleSwipeInput(ref bool leftPressed, ref bool rightPressed, ref bool jumpPressed)
    {
        // 1) Mobil touch varsa: Moved sýrasýnda anýnda tetikle
        if (Input.touchCount > 0)
        {
            UnityEngine.Touch t = Input.GetTouch(0);

            if (t.phase == UnityEngine.TouchPhase.Began)
            {
                swipeTracking = true;
                swipeConsumed = false;
                swipeStartPos = t.position;
            }

            if (swipeTracking && !swipeConsumed &&
                (t.phase == UnityEngine.TouchPhase.Moved || t.phase == UnityEngine.TouchPhase.Stationary))
            {
                Vector2 currentPos = t.position;
                if (TryConsumeSwipe(swipeStartPos, currentPos, ref leftPressed, ref rightPressed, ref jumpPressed))
                {
                    swipeConsumed = true;
                    swipeTracking = false;
                }
            }

            // Güvenlik: eðer Moved sýrasýnda tetiklenmediyse, Ended'da da dene
            if (swipeTracking && !swipeConsumed &&
                (t.phase == UnityEngine.TouchPhase.Ended || t.phase == UnityEngine.TouchPhase.Canceled))
            {
                Vector2 endPos = t.position;
                TryConsumeSwipe(swipeStartPos, endPos, ref leftPressed, ref rightPressed, ref jumpPressed);

                swipeTracking = false;
                swipeConsumed = false;
            }

            if (t.phase == UnityEngine.TouchPhase.Ended || t.phase == UnityEngine.TouchPhase.Canceled)
            {
                swipeTracking = false;
                swipeConsumed = false;
            }

            return;
        }

        // 2) Touch yoksa: Editor/PC mouse swipe (býrakýnca deðerlendir)
        if (!enableMouseSwipeInEditor) return;

        if (Input.GetMouseButtonDown(0))
        {
            swipeTracking = true;
            swipeConsumed = false;
            swipeStartPos = (Vector2)Input.mousePosition;
        }

        if (swipeTracking && Input.GetMouseButtonUp(0))
        {
            Vector2 endPos = (Vector2)Input.mousePosition;
            TryConsumeSwipe(swipeStartPos, endPos, ref leftPressed, ref rightPressed, ref jumpPressed);

            swipeTracking = false;
            swipeConsumed = false;
        }
    }

    private bool TryConsumeSwipe(Vector2 start, Vector2 end, ref bool leftPressed, ref bool rightPressed, ref bool jumpPressed)
    {
        Vector2 delta = end - start;

        if (delta.magnitude < swipeThresholdPixels)
            return false;

        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);

        if (absX > absY * dominantAxisRatio)
        {
            if (delta.x > 0f) rightPressed = true;
            else leftPressed = true;
            return true;
        }

        if (absY > absX * dominantAxisRatio)
        {
            if (delta.y > 0f) jumpPressed = true;
            return true;
        }

        return false;
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

        SetRenderersVisible(true);
    }

    private IEnumerator BlinkCoroutine(float duration)
    {
        float end = Time.time + duration;
        bool visible = true;

        while (Time.time < end)
        {
            visible = !visible;
            SetRenderersVisible(visible);
            yield return new WaitForSeconds(blinkInterval);
        }

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
