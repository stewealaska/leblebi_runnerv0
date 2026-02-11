using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool Restarted = false;

    public bool IsShieldActive { get { return shieldActive; } }

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text multiplierText;
    public TMP_Text livesText; // Artık kullanılmayacak (istersen boş bırak / UI'da kapat)
    public TMP_Text magnetTimerText;
    public TMP_Text shieldTimerText;

    public GameObject startPanel;
    public GameObject gameOverPanel;

    [Header("High Score UI")]
    public TMP_Text highScoreText; // Sağda göstereceğin TMP_Text

    [Header("Lives Icons (Mario Style)")]
    public Transform livesContainer;       // Canvas altındaki LivesContainer
    public GameObject lifeIconPrefab;      // Robot ikon prefabı (UI Image)
    private readonly List<GameObject> lifeIcons = new List<GameObject>();

    [Header("Speed & Multiplier")]
    public float startSpeed = 8f;
    public float maxSpeed = 18f;
    public float secondsToMax = 90f;
    public float maxMultiplier = 5f;

    [Header("Lives")]
    public int startLives = 3;
    public int maxLives = 5;

    [Tooltip("Engelle temas ettiğinde aynı anda art arda can gitmesin diye koruma süresi.")]
    public float damageCooldown = 0.8f;

    [Header("Hit Invulnerability")]
    public float invulnDuration = 1.2f;
    public float hitSpeedPenalty = 4f;

    [Header("Shield")]
    [Tooltip("Shield engel çarpışmasını yedikten sonra aynı frame'de ikinci hit gelmesini önlemek için kısa koruma.")]
    public float shieldConsumeInvuln = 0.45f;

    public bool GameStarted { get; private set; } = false;
    public float CurrentSpeed { get; private set; }
    public int Lives { get; private set; }

    private int score = 0;
    private bool isGameOver = false;
    private float nextDamageTime = 0f;

    private bool invulnerable = false;
    private float invulnEndTime = 0f;

    private int playerLayer = -1;
    private int obstacleLayer = -1;

    private bool shieldActive = false;
    private float shieldEndTime = 0f;

    // ===============================
    //   SWIPE UP DETECTION
    // ===============================
    [Header("Swipe Settings")]
    [Tooltip("Yukarı kaydırmanın sayılabilmesi için gereken minimum dikey mesafe (piksel).")]
    public float swipeUpMinDistance = 80f;

    [Tooltip("Yukarı kaydırmada izin verilen maksimum yatay sapma (piksel).")]
    public float swipeMaxHorizontalDrift = 160f;

    private Vector2 touchStartPos;
    private bool isSwiping = false;

    // ===============================
    //   HIGH SCORE (PlayerPrefs)
    // ===============================
    private const string HIGH_SCORE_KEY = "HIGH_SCORE";
    private int highScore = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        score = 0;
        isGameOver = false;

        invulnerable = false;
        invulnEndTime = 0f;
        nextDamageTime = 0f;

        shieldActive = false;
        shieldEndTime = 0f;

        playerLayer = LayerMask.NameToLayer("Player");
        obstacleLayer = LayerMask.NameToLayer("Obstacle");

        if (playerLayer != -1 && obstacleLayer != -1)
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, false);

        RunnerPlayer rp = FindFirstObjectByType<RunnerPlayer>();
        if (rp != null)
        {
            rp.StopInvulnerabilityBlink();
        }

        Lives = Mathf.Clamp(startLives, 0, maxLives);
        CurrentSpeed = startSpeed;

        // High score load
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

        BuildLivesIconPool();
        UpdateUI();
        UpdateHighScoreUI();
        SetMagnetTimer(0f);
        SetShieldTimer(0f);

        if (Restarted)
        {
            Restarted = false;

            GameStarted = true;
            if (startPanel != null) startPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            Time.timeScale = 1f;
        }
        else
        {
            GameStarted = false;
            if (startPanel != null) startPanel.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            Time.timeScale = 0f;
        }
    }

    private void Update()
    {
        if (!GameStarted)
        {
            bool startPressed = Input.GetKeyDown(KeyCode.Space) || IsSwipeUp();
            if (startPressed)
            {
                StartGame();
                return;
            }
        }

        if (isGameOver)
        {
            bool restartPressed = Input.GetKeyDown(KeyCode.Space) || IsSwipeUp();
            if (restartPressed)
            {
                RestartGame();
                return;
            }
        }

        if (!GameStarted || isGameOver) return;

        UpdateSpeedAndMultiplier();
        UpdateMultiplierUI();

        if (shieldActive)
        {
            float remaining = shieldEndTime - Time.time;
            if (remaining <= 0f)
            {
                shieldActive = false;
                SetShieldTimer(0f);
            }
            else
            {
                SetShieldTimer(remaining);
            }
        }

        if (invulnerable && Time.time >= invulnEndTime)
        {
            invulnerable = false;

            if (playerLayer != -1 && obstacleLayer != -1)
                Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, false);

            RunnerPlayer rp = FindFirstObjectByType<RunnerPlayer>();
            if (rp != null)
            {
                rp.StopInvulnerabilityBlink();
            }
        }
    }

    private bool IsSwipeUp()
    {
        // Mobil dokunmatik
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isSwiping = true;
                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended && isSwiping)
            {
                isSwiping = false;
                Vector2 delta = touch.position - touchStartPos;

                if (delta.y >= swipeUpMinDistance && Mathf.Abs(delta.x) <= swipeMaxHorizontalDrift)
                    return true;
            }

            return false;
        }

        // PC testi için mouse ile "yukarı sürükle"
        if (Input.GetMouseButtonDown(0))
        {
            isSwiping = true;
            touchStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            isSwiping = false;
            Vector2 endPos = Input.mousePosition;
            Vector2 delta = endPos - touchStartPos;

            if (delta.y >= swipeUpMinDistance && Mathf.Abs(delta.x) <= swipeMaxHorizontalDrift)
                return true;
        }

        return false;
    }

    private void StartGame()
    {
        GameStarted = true;
        Time.timeScale = 1f;
        if (startPanel != null) startPanel.SetActive(false);
    }

    private void UpdateSpeedAndMultiplier()
    {
        if (secondsToMax <= 0.01f) secondsToMax = 0.01f;

        float accel = (maxSpeed - startSpeed) / secondsToMax;
        CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + accel * Time.deltaTime);
    }

    public float GetMultiplier()
    {
        if (maxSpeed <= startSpeed) return 1f;

        float t = Mathf.InverseLerp(startSpeed, maxSpeed, CurrentSpeed);
        float m = Mathf.Lerp(1f, maxMultiplier, t);

        m = Mathf.Round(m * 10f) / 10f;
        return Mathf.Clamp(m, 1f, maxMultiplier);
    }

    public void AddCoinScore(int baseValue)
    {
        if (isGameOver) return;

        float mult = GetMultiplier();
        int add = Mathf.RoundToInt(baseValue * mult);

        score += add;

        // High score check (score değiştiği an)
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }

        UpdateUI();
    }

    public void AddLife(int amount)
    {
        if (isGameOver) return;
        if (amount <= 0) return;

        Lives = Mathf.Clamp(Lives + amount, 0, maxLives);
        UpdateLivesUI();
    }

    public void ActivateShield(float duration)
    {
        if (isGameOver) return;

        shieldActive = true;
        shieldEndTime = Time.time + duration;
        SetShieldTimer(duration);
    }

    public bool TryHandleObstacleHit(int damageAmount, GameObject obstacleRoot)
    {
        if (isGameOver) return false;
        if (!GameStarted) return false;

        if (invulnerable) return false;

        if (shieldActive)
        {
            float remaining = shieldEndTime - Time.time;
            if (remaining > 0f)
            {
                shieldActive = false;
                SetShieldTimer(0f);

                if (obstacleRoot != null)
                    Destroy(obstacleRoot);

                invulnerable = true;
                invulnEndTime = Time.time + shieldConsumeInvuln;
                nextDamageTime = Time.time + shieldConsumeInvuln;

                if (playerLayer != -1 && obstacleLayer != -1)
                    Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, true);

                RunnerPlayer rp = FindFirstObjectByType<RunnerPlayer>();
                if (rp != null)
                {
                    rp.NudgeForward(2.5f, 0.2f);
                    rp.StartInvulnerabilityBlink(shieldConsumeInvuln);
                }

                return true;
            }

            shieldActive = false;
            SetShieldTimer(0f);
        }

        bool tookDamage = TryTakeDamage(damageAmount);

        if (tookDamage && obstacleRoot != null)
            Destroy(obstacleRoot);

        return tookDamage;
    }

    public bool TryTakeDamage(int amount)
    {
        if (isGameOver) return false;
        if (!GameStarted) return false;
        if (amount <= 0) return false;

        if (invulnerable) return false;

        if (Time.time < nextDamageTime) return false;
        nextDamageTime = Time.time + damageCooldown;

        Lives = Mathf.Clamp(Lives - amount, 0, maxLives);
        UpdateLivesUI();

        CurrentSpeed = Mathf.Max(startSpeed, CurrentSpeed - hitSpeedPenalty);

        invulnerable = true;
        invulnEndTime = Time.time + invulnDuration;

        if (playerLayer != -1 && obstacleLayer != -1)
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, true);

        RunnerPlayer rp = FindFirstObjectByType<RunnerPlayer>();
        if (rp != null)
        {
            rp.NudgeForward(2.0f, 0.2f);
            rp.StartInvulnerabilityBlink(invulnDuration);
        }

        if (Lives <= 0)
            GameOver();

        return true;
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        SetMagnetTimer(0f);
        SetShieldTimer(0f);

        // Güvenlik: game over anında da kaydet
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }

        RunnerPlayer rp = FindFirstObjectByType<RunnerPlayer>();
        if (rp != null) rp.StopInvulnerabilityBlink();
    }

    private void RestartGame()
    {
        Restarted = true;

        if (playerLayer != -1 && obstacleLayer != -1)
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, false);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetMagnetTimer(float remaining)
    {
        if (magnetTimerText == null) return;

        if (remaining <= 0f)
        {
            magnetTimerText.text = "";
            return;
        }

        magnetTimerText.text = remaining.ToString("0.0") + "s";
    }

    public void SetShieldTimer(float remaining)
    {
        if (shieldTimerText == null) return;

        if (remaining <= 0f)
        {
            shieldTimerText.text = "";
            return;
        }

        shieldTimerText.text = remaining.ToString("0.0") + "s";
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();

        UpdateMultiplierUI();
        UpdateLivesUI();
    }

    private void UpdateMultiplierUI()
    {
        if (multiplierText != null)
            multiplierText.text = "x" + GetMultiplier().ToString("0.0");
    }

    // ==========================
    //  LIVES ICONS (MARIO STYLE)
    // ==========================

    private void BuildLivesIconPool()
    {
        if (livesContainer == null || lifeIconPrefab == null)
            return;

        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] != null)
                Destroy(lifeIcons[i]);
        }
        lifeIcons.Clear();

        for (int i = 0; i < maxLives; i++)
        {
            GameObject icon = Instantiate(lifeIconPrefab, livesContainer);
            icon.SetActive(false);
            lifeIcons.Add(icon);
        }

        RefreshLivesIcons();
    }

    private void UpdateLivesUI()
    {
        RefreshLivesIcons();
    }

    private void RefreshLivesIcons()
    {
        if (livesContainer == null || lifeIconPrefab == null) return;

        if (lifeIcons.Count != maxLives)
        {
            BuildLivesIconPool();
            return;
        }

        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] == null) continue;
            lifeIcons[i].SetActive(i < Lives);
        }
    }

    // ==========================
    //  HIGH SCORE UI
    // ==========================

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = "HS " + highScore.ToString();
    }
}
