using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBPvPBattleModePlayerUI : MonoBehaviour
{
    [SerializeField] PBPvPPlayerNameUI playerNameUI;
    [SerializeField] TMP_Text overallScore;
    [SerializeField] SpriteSwitcher playerBackgroundFrame;
    [SerializeField] RawImage robotRenderRawImage;
    [SerializeField] private bool m_IsPlayerTeam;

    PlayerInfoVariable playerInfoVariable;

    public PlayerInfoVariable PlayerInfoVariable
    {
        get => playerInfoVariable;
        set
        {
            playerInfoVariable = value;
            UpdateView();
        }
    }

    public RenderTexture RobotRenderTexture
    {
        set
        {
            if (value == null)
            {
                robotRenderRawImage.gameObject.SetActive(false);
                return;
            }
            robotRenderRawImage.texture = value;
            robotRenderRawImage.gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        playerNameUI.PersonalInfo = null;
    }

    private void UpdateView()
    {
        playerNameUI.PersonalInfo = playerInfoVariable.value.personalInfo;
        overallScore.text = playerInfoVariable.value.Cast<PBPlayerInfo>().robotStatsSO.value.ToRoundedText();
        playerBackgroundFrame.ChangeSprite(m_IsPlayerTeam ? 1 : 2/*GetFrameIndex(playerInfoVariable.value.personalInfo)*/);
    }

    static int GetFrameIndex(PersonalInfo personalInfo)
    {
        if (personalInfo != null)
        {
            if (personalInfo.isLocal == true) return 1;
            else return 2;
        }
        else return 0;
    }
}
