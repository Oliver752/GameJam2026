using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HappinessBar : MonoBehaviour
{
    [Header("Fill Image (Type = Filled)")]
    public Image fillImage;

    [Header("Optional text (e.g. 70/100)")]
    public TMP_Text valueText;

    private void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponent<Image>();
    }

    public void SetHappiness(int value)
    {
        value = Mathf.Clamp(value, 0, 100);

        // Fill
        if (fillImage != null)
            fillImage.fillAmount = value / 100f;

        // Text
        if (valueText != null)
            valueText.text = $"{value}/100";
    }
}
