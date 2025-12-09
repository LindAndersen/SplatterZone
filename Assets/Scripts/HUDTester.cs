using UnityEngine;

public class HUDTester : MonoBehaviour
{
    float currentHealth = 100f;
    float maxHealth = 100f;

    void Update()
    {
        // Bajar vida con tecla H
        if (Input.GetKeyDown(KeyCode.H))
        {
            currentHealth -= 10f;
            if (currentHealth < 0) currentHealth = 0;
            UIManager.Instance.SetHealth(currentHealth, maxHealth);
        }

        // Simular zombie muerto con tecla K
        if (Input.GetKeyDown(KeyCode.K))
        {
            UIManager.Instance.AddZombieKill();
        }

        // Añadir puntos con tecla P
        if (Input.GetKeyDown(KeyCode.P))
        {
            UIManager.Instance.AddPoints(50);
        }
    }
}
