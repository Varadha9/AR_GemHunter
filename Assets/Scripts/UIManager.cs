using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("In-Game HUD Overlays")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Game Over Overlay")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private void Awake()
    {
        // Auto-link references if unassigned
        if (scoreText == null)
        {
            GameObject obj = GameObject.Find("ScoreText");
            if (obj != null) scoreText = obj.GetComponent<TextMeshProUGUI>();
        }

        if (timerText == null)
        {
            GameObject obj = GameObject.Find("TimerText");
            if (obj != null) timerText = obj.GetComponent<TextMeshProUGUI>();
        }

        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOverPanel");
        }

        if (finalScoreText == null && gameOverPanel != null)
        {
            Transform t = gameOverPanel.transform.Find("FinalScoreText");
            if (t != null) finalScoreText = t.GetComponent<TextMeshProUGUI>();
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = $"Time: {seconds}s";
        }
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}
