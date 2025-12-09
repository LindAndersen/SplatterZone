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

    [Header("Pause")]
    public GameObject pausePanel;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI goZombiesText;
    public TextMeshProUGUI goPointsText;

    [Header("Config")]
    public int maxZombies = 10;

    int zombiesKilled = 0;
    int points = 0;
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

        UpdateZombiesText();
        UpdatePointsText();
        SetHealth(100, 100);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // No permitir pausar si ya está en Game Over
        if (Input.GetKeyDown(KeyCode.Escape) && (gameOverPanel == null || !gameOverPanel.activeSelf))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ---------- VIDA ----------
    public void SetHealth(float current, float max)
    {
        if (healthFill == null) return;

        float t = Mathf.Clamp01(current / max);
        healthFill.fillAmount = t;
    }

    // ---------- ZOMBIS / PUNTOS ----------
    public void AddZombieKill(int addPoints = 1)
    {
        zombiesKilled++;
        points += addPoints;
        UpdateZombiesText();
        UpdatePointsText();
    }

    public void SetZombieProgress(int killed, int max)
    {
        zombiesKilled = killed;
        maxZombies = max;
        UpdateZombiesText();
    }

    void UpdateZombiesText()
    {
        if (zombiesText != null)
            zombiesText.text = zombiesKilled + "/" + maxZombies;
    }

    public void AddPoints(int extra)
    {
        points += extra;
        UpdatePointsText();
    }

    public void SetPoints(int value)
    {
        points = value;
        UpdatePointsText();
    }

    void UpdatePointsText()
    {
        if (pointsText != null)
            pointsText.text = "Puntos: " + points;
    }

    // ---------- PAUSA ----------
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
        if (hudPanel != null)
            hudPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Activar Game Over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Congelar el tiempo
        Time.timeScale = 0f;

        // Mostrar stats
        if (goZombiesText != null)
            goZombiesText.text = zombiesKilled + "/" + maxZombies;

        if (goPointsText != null)
            goPointsText.text = points.ToString();

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
