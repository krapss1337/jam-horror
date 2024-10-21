using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthTxt = default;
    [SerializeField] private TextMeshProUGUI staminaTxt = default;

    private void OnEnable()
    {
        PlayerController.OnDamage += UpdateHealthUI;
        PlayerController.OnHeal += UpdateHealthUI;
        PlayerController.OnStaminaChange += UpdateStaminaUI;
    }

    private void OnDisable()
    {
        PlayerController.OnDamage -= UpdateHealthUI;
        PlayerController.OnHeal -= UpdateHealthUI;
        PlayerController.OnStaminaChange += UpdateStaminaUI;
    }

    private void Start()
    {
        UpdateHealthUI(100);
        UpdateStaminaUI(100);
    }

    private void UpdateHealthUI(float currentHealth)
    {
        healthTxt.text = currentHealth.ToString("00");
    }

    private void UpdateStaminaUI(float currentStamina)
    {
        staminaTxt.text = currentStamina.ToString("00");
    }
}
