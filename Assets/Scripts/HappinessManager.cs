using UnityEngine;

public class HappinessManager : MonoBehaviour
{
    public static HappinessManager Instance;

    [Range(0, 100)]
    public int happiness = 100;   // ✅ štart 100

    public HappinessBar happinessBar; // sem potiahni objekt so scriptom HappinessBar

    private void Awake()
    {
        Instance = this;
        happiness = 100;
    }

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        // ✅ TEST klávesy (môžeš neskôr zmazať)
        if (Input.GetKeyDown(KeyCode.Alpha1)) AddHappiness(-10);
        if (Input.GetKeyDown(KeyCode.Alpha2)) AddHappiness(+10);
    }

    public void AddHappiness(int delta)
    {
        happiness = Mathf.Clamp(happiness + delta, 0, 100);
        RefreshUI();
        Debug.Log($"Happiness: {happiness}");
    }

    private void RefreshUI()
    {
        if (happinessBar != null)
            happinessBar.SetHappiness(happiness);
        else
            Debug.LogWarning("[HappinessManager] happinessBar is not assigned.");
    }
}
