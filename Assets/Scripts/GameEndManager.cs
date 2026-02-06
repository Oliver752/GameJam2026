using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance;

    [Header("Panel Controller")]
    public PanelController panelController;

    private void Awake()
    {
        Instance = this;
    }

    public void TriggerWin()
    {
        if (House.gameEnded) return;
        House.gameEnded = true;
        panelController?.ShowWinPanel();
    }

    public void TriggerLose()
    {
        if (House.gameEnded) return;
        House.gameEnded = true;
        panelController?.ShowLosePanel();
    }
}