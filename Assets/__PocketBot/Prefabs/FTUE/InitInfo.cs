using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitInfo : MonoBehaviour
{
    [SerializeField] PlayerInfoVariable enemyInfo;
    [SerializeField] PlayerDatabaseSO personalInfos;
    [SerializeField] PBRobotStatsSO enemyStatSO;
    [SerializeField] PB_AIProfile profile;

    private void Awake()
    {
        var personalInfo = GetRandomPersonalInfo();
        var infoOfOpponent = new PBBotInfo(personalInfo, enemyStatSO, profile);

        enemyInfo.value = infoOfOpponent;
    }

    protected virtual PersonalInfo GetRandomPersonalInfo()
    {
        return personalInfos.GetRandomAnonymousBotInfo();
    }
}