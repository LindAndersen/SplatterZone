using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD Panel")]
    public GameObject hudPanel;
    public AudioClip changeWaveSound;
    public AudioClip backgroundMusic;
    public Animator waveCounterAnimator;

    [Header("Audio")]
    [Range(0.05f, 2f)] public float musicFadeDuration = 0.35f;

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

    private string waveTallyMarks = "";
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioSource stingerAudioSource; // dedicated for wave change sound
    private float backgroundVolume = 0.1f;
    private float stingerTargetVolume = 0.8f;
    private Coroutine waveAudioRoutine;

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

        SetupPlayerAudioSource();
        StartBackgroundMusic();
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
        {
            waveTallyMarks = WaveToTallyMarks(wave);

            if (waveCounterAnimator != null)
            {
                waveCounterAnimator.SetTrigger("WaveChange");
            }

            if (changeWaveSound != null && playerAudioSource != null)
            {
                if (waveAudioRoutine != null)
                    StopCoroutine(waveAudioRoutine);

                waveAudioRoutine = StartCoroutine(PlayChangeWaveSoundWithFade());
            }
        }
    }

    void SetupPlayerAudioSource()
    {
        if (playerAudioSource == null)
        {
            Debug.LogWarning("UIManager: assign a Player AudioSource in the inspector.");
            return;
        }

        playerAudioSource.playOnAwake = false;
        playerAudioSource.loop = true;

        if (playerAudioSource.volume > 0f)
            backgroundVolume = playerAudioSource.volume;

        // Set up stinger source if present
        if (stingerAudioSource != null)
        {
            stingerAudioSource.playOnAwake = false;
            stingerAudioSource.loop = false;
            stingerAudioSource.volume = 0f; // keep silent until needed
        }
    }

    void StartBackgroundMusic()
    {
        if (playerAudioSource == null || backgroundMusic == null)
            return;

        playerAudioSource.clip = backgroundMusic;
        playerAudioSource.volume = backgroundVolume;
        playerAudioSource.loop = true;
        playerAudioSource.Play();
    }

    IEnumerator PlayChangeWaveSoundWithFade()
    {
        if (playerAudioSource == null)
            yield break;
        // If we have a dedicated stinger source, crossfade between the two sources
        if (stingerAudioSource != null && changeWaveSound != null)
        {
            // Ensure background music is playing
            if (playerAudioSource.clip != backgroundMusic && backgroundMusic != null)
            {
                playerAudioSource.clip = backgroundMusic;
                playerAudioSource.loop = true;
                playerAudioSource.volume = backgroundVolume;
                playerAudioSource.Play();
            }

            // Start crossfade: background down, stinger up
            stingerAudioSource.clip = changeWaveSound;
            stingerAudioSource.volume = 0f;
            stingerAudioSource.loop = false;
            stingerAudioSource.Play();

            // Run both fades in parallel over musicFadeDuration
            yield return StartCoroutine(CrossfadeSources(playerAudioSource, stingerAudioSource, backgroundVolume, stingerTargetVolume, musicFadeDuration));

            // Wait until stinger ends
            yield return new WaitForSeconds(changeWaveSound.length - musicFadeDuration);

            // Fade stinger out and background back in
            yield return StartCoroutine(CrossfadeSources(stingerAudioSource, playerAudioSource, stingerTargetVolume, backgroundVolume, musicFadeDuration));
            stingerAudioSource.Stop();
        }
        else
        {
            // Fallback: single source behavior
            if (playerAudioSource.clip == backgroundMusic && playerAudioSource.isPlaying)
                yield return FadeToVolume(0f, musicFadeDuration);

            playerAudioSource.loop = false;
            float preOneShotVolume = playerAudioSource.volume; // likely 0 after fade
            float oneShotVolume = Mathf.Max(0.2f, backgroundVolume);
            playerAudioSource.volume = oneShotVolume;
            playerAudioSource.PlayOneShot(changeWaveSound);
            yield return new WaitForSeconds(changeWaveSound.length);
            playerAudioSource.volume = preOneShotVolume;

            if (backgroundMusic != null)
            {
                if (playerAudioSource.clip != backgroundMusic)
                {
                    playerAudioSource.clip = backgroundMusic;
                    playerAudioSource.Play();
                }
                playerAudioSource.loop = true;
                yield return FadeToVolume(backgroundVolume, musicFadeDuration);
            }
        }
    }

    IEnumerator CrossfadeSources(AudioSource downSource, AudioSource upSource, float downStartOrCurrent, float upTarget, float duration)
    {
        float startDown = downSource != null ? downSource.volume : 0f;
        float startUp = upSource != null ? upSource.volume : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            if (downSource != null)
                downSource.volume = Mathf.Lerp(startDown, 0f, t);
            if (upSource != null)
                upSource.volume = Mathf.Lerp(startUp, upTarget, t);
            yield return null;
        }
        if (downSource != null) downSource.volume = 0f;
        if (upSource != null) upSource.volume = upTarget;
    }

    IEnumerator FadeToVolume(float targetVolume, float duration)
    {
        if (playerAudioSource == null)
            yield break;

        float startVolume = playerAudioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            playerAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        playerAudioSource.volume = targetVolume;
    }

    public void AnimationKeyFrameSetWaveCounter()
    {
        if (waveCounter != null)
            waveCounter.text = waveTallyMarks;
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
