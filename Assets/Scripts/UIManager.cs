using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD Panel")]
    public GameObject hudPanel;

    [Header("HUD Elements")]
    public Image healthFill;
    public TextMeshProUGUI zombiesText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI waveCounter;

    [Header("Pause")]
    public GameObject pausePanel;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public GameObject BloodCanvas;
    public GameObject FPSCanvas;
    public TextMeshProUGUI goZombiesText;
    public TextMeshProUGUI goPointsText;

    int totalZombiesKilled = 0;
    int points = 0;
    public int zombiesKilledThisWave = 0;
    bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (hudPanel != null)
            hudPanel.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        SetZombieProgress(0);
        UpdatePointsText(0);
        SetHealth(100, 100);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && (gameOverPanel == null || !gameOverPanel.activeSelf))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        UpdatePointsText(points);
    }

    void UpdatePointsText(int points)
    {
        if (pointsText != null)
            pointsText.text = "Points: " + points;
    }

    public void SetHealth(float current, float max)
    {
        if (healthFill == null) return;

        float t = Mathf.Clamp01(current / max);
        healthFill.fillAmount = t;
    }

    public void AddZombieKill(int addPoints = 1)
    {
        points += addPoints;
        zombiesKilledThisWave++;
        totalZombiesKilled++;
    }

    public void SetZombieProgress(int max)
    {
        if (zombiesText != null)
            zombiesText.text = $"{max - zombiesKilledThisWave}/{max}";
    }

    public void SetWaveCounter(int wave)
    {
        if (waveCounter != null)
            waveCounter.text = $"{WaveToTallyMarks(wave)}";
    }

    string WaveToTallyMarks(int wave)
    {
        if (wave <= 0) return "";

        // Each group of 5 = "e", remainder uses a-e
        int fullGroups = (wave - 1) / 5;
        int remainder = ((wave - 1) % 5) + 1;

        string result = "";

        // Add full groups (each group = "e")
        for (int i = 0; i < fullGroups; i++)
        {
            result += "e";
        }

        // Add remainder as a-e
        char[] tallyChars = { 'a', 'b', 'c', 'd', 'e' };
        result += tallyChars[remainder - 1];

        return result;
    }

    void PauseGame()
    {
        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnClick_Pause()
    {
        PauseGame();
    }

    public void OnClick_Resume()
    {
        ResumeGame();
    }

    public void OnClick_Options()
    {
        Debug.Log("Abrir opciones de pausa");
    }

    public void OnClick_ReturnToMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    // ---------- GAME OVER ----------
    public void ShowGameOver()
    {
        // Desactivar HUD y pausa
        if (BloodCanvas != null)
            BloodCanvas.SetActive(false);

        if (FPSCanvas != null)
            FPSCanvas.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Activar Game Over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Congelar el tiempo
        Time.timeScale = 0f;

        // Mostrar stats
        if (goZombiesText != null)
            goZombiesText.text = $"Zombies killed: {totalZombiesKilled}";

        if (goPointsText != null)
            goPointsText.text = $"Score: {points}";

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClick_PlayAgain()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
