using UnityEngine;
using TMPro;

public class KillFeedEntry : MonoBehaviour
{
    public Color localPlayerColor, allyColor, opponentColor;
    [Header("UI References")]
    public TMP_Text killerNameText;
    public TMP_Text victimNameText;

    private Color GetColor(PBRobot robot)
    {
        if (robot.PersonalInfo.isLocal)
            return localPlayerColor;
        if (robot.TeamId == 1)
            return allyColor;
        return opponentColor;
    }

    public void SetData(PBRobot killer, PBRobot victim)
    {
        killerNameText.text = killer.PersonalInfo.name;
        killerNameText.color = GetColor(killer);
        victimNameText.text = victim.PersonalInfo.name;
        victimNameText.color = GetColor(victim);
    }
}