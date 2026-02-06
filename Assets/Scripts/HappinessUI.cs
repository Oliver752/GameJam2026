using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HappinessUI : MonoBehaviour
{
    [Header("UI")]
    public Image fillImage;          
    public TMP_Text valueText;       

    [Header("Values")]
    public int maxHappiness = 100;
    public int currentHappiness = 70;

    void Start()
    {
        UpdateUI();
    }

    public void SetHappiness(int value)
    {
        currentHappiness = Mathf.Clamp(value, 0, maxHappiness);
        UpdateUI();
    }

    public void AddHappiness(int delta)
    {
        SetHappiness(currentHappiness + delta);
    }

    void UpdateUI()
    {
        float t = (float)currentHappiness / maxHappiness;
        fillImage.fillAmount = t;
        valueText.text = $"{currentHappiness}/{maxHappiness}";
    }
}
