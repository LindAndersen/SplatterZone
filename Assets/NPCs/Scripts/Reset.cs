using UnityEngine;
using UnityEngine.SceneManagement;

public class Reset : MonoBehaviour
{
    public KeyCode restartKey = KeyCode.R;

    void Update()
    {
        if (Input.GetKeyDown(restartKey))
            RestartGame();
    }

    public void RestartGame()
    {
        // Falls du irgendwo Time.timeScale verändert hast (Pause etc.)
        Time.timeScale = 1f;

        // Aktuelle Szene neu laden
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
