using UnityEngine;

public class Player : MonoBehaviour
{
    public float maxStamina = 100;
    public float currentStamina;

    public StaminaBar staminaBar;


    void Start()
    {
        currentStamina = maxStamina;
        staminaBar.SetMaxStamina(maxStamina);
    }

    void Update()
    {
        // Example of stamina usage
        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D))
        {
            UseStamina(0.1f);
        }
        // Example of stamina regeneration
        if (!Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D))
        {
            RegenerateStamina(0.1f);
        }
    }

    public void UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            Debug.Log("Used " + amount + " stamina. Current stamina: " + currentStamina);
        }
        else
        {
          
        }

        staminaBar.SetStamina(currentStamina);
    }

    public void RegenerateStamina(float amount)
    {
        if (currentStamina + amount <= maxStamina)
        {
            currentStamina += amount;
            Debug.Log("Regenerated " + amount + " stamina. Current stamina: " + currentStamina);
        }
        else
        {
            currentStamina = maxStamina;
        }
        staminaBar.SetStamina(currentStamina);
    }
}
