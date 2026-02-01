using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool Restarted = false;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text multiplierText;

    [Tooltip("Can göstergesi (örn: 3/5). Boþ kalabilir.")]
    public TMP_Text livesText;

    [Tooltip("Magnet kalan süre (örn: 2.7s). Boþ kalabilir.")]
    public TMP_Text magnetTimerText;

    public GameObject startPanel;
    public GameObject gameOverPanel;

    [Header("Speed & Multiplier")]
    public float startSpeed = 8f;
    public float maxSpeed = 18f;
    public float secondsToMax = 90f;
    public float maxMultiplier = 5f;

    [Header("Lives")]
    public int startLives = 3;
    public int maxLives = 5;

    [Tooltip("Engelle temas ettiðinde ayný anda art arda can gitmesin diye koruma süresi.")]
    public float damageCooldown = 0.8f;

    [Header("Hit Invulnerability")]
    public float invulnDuration = 1.2f;

    [Tooltip("Çarpýnca hýzdan düþülsün mü? (m/s gibi düþün)")]
    public float hitSpeedPenalty = 4f;

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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // --- HARD RESET (Restart sonrasý bozulmayý engeller) ---
        score = 0;
        isGameOver = false;

        invulnerable = false;
        invulnEndTime = 0f;
        nextDamageTime = 0f;

        playerLayer = LayerMask.NameToLayer("Player");
        obstacleLayer = LayerMask.NameToLayer("Obstacle");

        if (playerLayer != -1 && obstacleLayer != -1)
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, false);

        RunnerPlayer rp = FindFirstObjectByType<RunnerPlayer>();
        if (rp != null)
        {
            rp.SetCollisionEnabled(true);
            rp.StopInvulnerabilityBlink(); // restart sonrasý görünür kalsýn
        }

        Lives = Mathf.Clamp(startLives, 0, maxLives);
        CurrentSpeed = startSpeed;

        UpdateUI();
        SetMagnetTimer(0f);

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
        if (!GameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
            return;
        }

        if (isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
            return;
        }

        if (!GameStarted || isGameOver) return;

        UpdateSpeedAndMultiplier();
        UpdateMultiplierUI();

        // i-frame bittiðinde çarpýþmalarý geri aç + blink kapat
        if (invulnerable && Time.time >= invulnEndTime)
        {
            invulnerable = false;

            if (playerLayer != -1 && obstacleLayer != -1)
                Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, false);

            RunnerPlayer rp = FindFirstObjectByType<RunnerPlayer>();
            if (rp != null)
            {
                rp.SetCollisionEnabled(true);
                rp.StopInvulnerabilityBlink();
            }
        }
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
        UpdateUI();
    }

    public void AddLife(int amount)
    {
        if (isGameOver) return;
        if (amount <= 0) return;

        Lives = Mathf.Clamp(Lives + amount, 0, maxLives);
        UpdateLivesUI();
    }

    // Hasar uygulanýrsa true döner; i-frame vs yüzünden uygulanmazsa false.
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
            rp.SetCollisionEnabled(false);
            rp.NudgeForward(2.0f, 0.2f);

            //  Blink baþlat
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

        // Oyun bitince blink varsa kapatýp karakteri görünür býrak
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

    private void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = Lives.ToString() + "/" + maxLives.ToString();
    }
}
