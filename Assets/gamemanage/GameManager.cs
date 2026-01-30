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
    public GameObject startPanel;
    public GameObject gameOverPanel;

    [Header("Speed & Multiplier")]
    public float startSpeed = 8f;
    public float maxSpeed = 18f;
    public float secondsToMax = 90f; // süre uzadýkça daha yumuþak hýzlanýr
    public float maxMultiplier = 5f;

    public bool GameStarted { get; private set; } = false;
    public float CurrentSpeed { get; private set; }

    private int score = 0;
    private bool isGameOver = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        score = 0;
        isGameOver = false;

        CurrentSpeed = startSpeed;
        UpdateUI();

        if (Restarted)
        {
            Restarted = false;

            GameStarted = true;
            if (startPanel != null) startPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            Time.timeScale = 1f;
            return;
        }

        GameStarted = false;
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        Time.timeScale = 0f;
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

        float t = Mathf.InverseLerp(startSpeed, maxSpeed, CurrentSpeed); // 0..1
        float m = Mathf.Lerp(1f, maxMultiplier, t);

        // UI daha stabil görünsün diye 0.1 hassasiyete yuvarla
        m = Mathf.Round(m * 10f) / 10f;

        // garanti sýnýr
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

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private void RestartGame()
    {
        Restarted = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();

        UpdateMultiplierUI();
    }

    private void UpdateMultiplierUI()
    {
        if (multiplierText != null)
            multiplierText.text = "x" + GetMultiplier().ToString("0.0");
    }
}
