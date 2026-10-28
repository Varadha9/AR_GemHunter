using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f; // 60 seconds round

    [Header("UI Reference")]
    [SerializeField] private UIManager uiManager;

    [Header("Spawner Reference")]
    [SerializeField] private ARSpawner arSpawner;

    private int score = 0;
    private float timeRemaining;
    private bool isGameActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-find references if missing
        if (uiManager == null) uiManager = GetComponent<UIManager>() ?? FindObjectOfType<UIManager>();
        if (arSpawner == null) arSpawner = GetComponent<ARSpawner>() ?? FindObjectOfType<ARSpawner>();

        // Lock mobile target frame rate to 60 FPS
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        score = 0;
        timeRemaining = gameDuration;
        isGameActive = true;

        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
            uiManager.UpdateTimer(timeRemaining);
            uiManager.HideGameOver();
        }

        if (arSpawner != null)
        {
            arSpawner.StartSpawning();
        }
    }

    private void Update()
    {
        if (!isGameActive) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndGame();
        }

        if (uiManager != null)
        {
            uiManager.UpdateTimer(timeRemaining);
        }
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;

        score += points;
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
        }
    }

    private void EndGame()
    {
        isGameActive = false;

        if (arSpawner != null)
        {
            arSpawner.StopSpawning();
        }

        if (uiManager != null)
        {
            uiManager.ShowGameOver(score);
        }
    }

    public void RestartGame()
    {
        StartGame();
    }

    public bool IsGameActive() => isGameActive;
}
