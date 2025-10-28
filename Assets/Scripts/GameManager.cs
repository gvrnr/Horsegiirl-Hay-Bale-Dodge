using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    [Header("UI – In-game")]
    [SerializeField] private TMP_Text scoreText;                 // Canvas/ScoreText
    [SerializeField] private List<Image> heartImages = new();    // 5 heartss in order (left–right)

    [Header("UI – Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverTitleText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text highScoreText;     
    [SerializeField] private Button restartButton;              

    [Header("References to stop on Game Over (optional)")]
    [SerializeField] private List<MonoBehaviour> thingsToDisableOnGameOver;

    [Header("Gameplay")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int pointsPerTick = 10;     // +10 pts
    [SerializeField] private float tickSeconds = 5f;     // every 5s
    [SerializeField] private bool freezeWithTimeScale = true; // freeze gameplay on game over

    private int currentLives;
    private int score;
    private Coroutine scoreTickerCo;
    private bool gameIsOver;

    private const string HIGH_SCORE_KEY = "HIGH_SCORE";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        // hide Game Over UI at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        gameIsOver = false;
        currentLives = Mathf.Max(1, maxLives);
        score = 0;

        UpdateHearts();
        UpdateScoreUI();

        scoreTickerCo = StartCoroutine(ScoreTicker());
    }

    public void AddScore(int amount)
    {
        if (gameIsOver) return;
        score += amount;
        UpdateScoreUI();
    }

    public void LoseLife(int amount = 1)
    {
        if (gameIsOver) return;

        currentLives = Mathf.Max(0, currentLives - amount);
        UpdateHearts();

        if (currentLives <= 0)
        {
            OnGameOver();
        }
    }

    public int CurrentLives => currentLives;
    public int Score => score;

    private IEnumerator ScoreTicker()
    {
        while (!gameIsOver)
        {
            yield return new WaitForSeconds(tickSeconds);
            AddScore(pointsPerTick);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Points: {score}";
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] != null)
                heartImages[i].enabled = (i < currentLives);
        }
    }

    private void OnGameOver()
    {
        gameIsOver = true;

        // stop the score ticker
        if (scoreTickerCo != null)
        {
            StopCoroutine(scoreTickerCo);
            scoreTickerCo = null;
        }

        // disable gameplay scripts
        if (thingsToDisableOnGameOver != null)
        {
            foreach (var m in thingsToDisableOnGameOver)
                if (m != null) m.enabled = false;
        }

        // optionally freeze gameplay
        if (freezeWithTimeScale) Time.timeScale = 0f;

        // save & show scores
        int oldHigh = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (score > oldHigh)
        {
            oldHigh = score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, oldHigh);
            PlayerPrefs.Save();
        }

        if (gameOverTitleText != null) gameOverTitleText.text = "Game Over";
        if (finalScoreText != null) finalScoreText.text = $"Score: {score}";
        if (highScoreText != null) highScoreText.text = $"High Score: {oldHigh}";

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;
        gameIsOver = false;

        score = 0;
        currentLives = maxLives;
        UpdateScoreUI();
        UpdateHearts();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // re-enable scripts
        if (thingsToDisableOnGameOver != null)
        {
            foreach (var m in thingsToDisableOnGameOver)
                if (m != null) m.enabled = true;
        }

        if (scoreTickerCo == null)
            scoreTickerCo = StartCoroutine(ScoreTicker());
    }

    public void GainLife(int amount = 1)
    {
        if (gameIsOver) return;
        currentLives = Mathf.Min(currentLives + amount, maxLives);
        UpdateHearts();
    }
}
