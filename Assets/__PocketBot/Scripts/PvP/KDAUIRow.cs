using DG.Tweening;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KDAUIRow : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] private Color m_PlayerLocalColor;

    [SerializeField, BoxGroup("Ref")] private Image m_BackGround;
    [SerializeField, BoxGroup("Ref")] private Image m_Avatar;
    [SerializeField, BoxGroup("Ref")] private Image m_Flag;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_PlayerName;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_TrophyRankText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_KillsText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_DeathsText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_AssistText;
    [SerializeField, BoxGroup("Ref")] private EZAnimVisibility m_EZAnimVisibility;
    [SerializeField, BoxGroup("Ref")] private Sprite m_BlueTeamImage;
    [SerializeField, BoxGroup("Ref")] private Sprite m_RedTeamImage;

    [SerializeField, BoxGroup("Resource")] private Material m_FontDefault;

    public void Init(PBRobot robot, PBPvPMatch pbPvPMatch)
    {
        string name = robot.PlayerInfoVariable.value.personalInfo.name;
        PlayerKDA playerKDA = robot.PlayerKDA;
        m_BackGround.sprite = robot.TeamId == 1 ? m_BlueTeamImage : m_RedTeamImage;
        m_Avatar.sprite = robot.PlayerInfoVariable.value.personalInfo.avatar;
        m_Flag.sprite = robot.PlayerInfoVariable.value.personalInfo.nationalFlag;
        m_PlayerName.SetText($"{name}");
        if (robot.PersonalInfo.isLocal)
            m_PlayerName.color = m_PlayerLocalColor;
        else
            m_PlayerName.fontMaterial = m_FontDefault;

        m_TrophyRankText.SetText(robot.PersonalInfo.isLocal
                ? robot.PersonalInfo.GetTotalNumOfPoints().ToRoundedText()
                : (pbPvPMatch.GetLocalPlayerInfo().personalInfo.GetTotalNumOfPoints() + Random.Range(-0.1f, 0.1f) * pbPvPMatch.GetLocalPlayerInfo().personalInfo.GetTotalNumOfPoints()).ToRoundedText());
        m_KillsText.SetText($"{playerKDA.Kills}");
        m_DeathsText.SetText($"{playerKDA.Deaths}");
        m_AssistText.SetText($"{playerKDA.Assists}");
    }
}
