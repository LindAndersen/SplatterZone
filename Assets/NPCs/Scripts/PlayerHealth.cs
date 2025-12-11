using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI / Effects")]
    [Tooltip("Optional: Referenz auf den BloodPulse (Blut-Canvas). Wenn leer, wird beim Start danach gesucht.")]
    public BloodPulse bloodPulse;

    void Awake()
    {
        currentHealth = maxHealth;

        // Falls nicht im Inspector gesetzt: einmal im Scene-Hierarchy suchen
        if (bloodPulse == null)
            bloodPulse = FindFirstObjectByType<BloodPulse>();
    }

    /// <summary>
    /// Zieht dem Spieler Leben ab und löst den Blut-Canvas aus.
    /// </summary>
    public void LooseLife(int amount)
    {
        if (amount <= 0)
            return;

        // Blut-Overlay auslösen
        if (bloodPulse != null)
            bloodPulse.Pulse();

        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;

        //Debug.Log($"[PlayerHealth] LooseLife({amount}) → currentHealth={currentHealth}");

        if (currentHealth == 0)
            Die();

        UIManager.Instance.SetHealth(currentHealth, maxHealth);
    }

    /// <summary>
    /// Heilt den Spieler um einen bestimmten Betrag.
    /// </summary>
    public void GainHealth(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        //Debug.Log($"[PlayerHealth] GainHealth({amount}) → currentHealth={currentHealth}");

        UIManager.Instance.SetHealth(currentHealth, maxHealth);
    }

    private void Die()
    {
        //Debug.Log("[PlayerHealth] Player died");
        // TODO: Game Over / Respawn etc.
        UIManager.Instance.ShowGameOver();
    }
}
