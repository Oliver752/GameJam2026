using UnityEngine;

public class HappinessManager : MonoBehaviour
{
    public static HappinessManager Instance;

    [Header("Settings")]
    public int maxHappiness = 100;
    private int currentHappiness;

    [Header("UI")]
    public HappinessBar happinessBar;

    private void Awake()
    {
        Instance = this;
        currentHappiness = maxHappiness;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddHappiness(int amount)
    {
        if (House.gameEnded) return; // Don't change if game ended

        currentHappiness += amount;
        currentHappiness = Mathf.Clamp(currentHappiness, 0, maxHappiness);
        
        UpdateUI();

        // Check lose condition
        if (currentHappiness <= 0)
        {
            GameEndManager.Instance?.TriggerLose();
        }
    }

    public int GetHappiness() => currentHappiness;

    private void UpdateUI()
    {
        if (happinessBar != null)
            happinessBar.SetHappiness(currentHappiness);
    }
}