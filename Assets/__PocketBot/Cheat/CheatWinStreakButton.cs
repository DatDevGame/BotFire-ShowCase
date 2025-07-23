using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatWinStreakButton : MonoBehaviour
{
    public Button Button;
    public Button Button_2;
    public WinStreakManagerSO WinStreakManagerSO;
    public PPrefIntVariable WinStreak;

    private void Awake()
    {
        Button.onClick.AddListener(() =>
        {
            WinStreakManagerSO.ResetNow();
        });

        Button_2.onClick.AddListener(() => 
        {
            WinStreak.value++;
        });
    }
}
