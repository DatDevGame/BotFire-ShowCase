using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBPlayerInfo : PlayerInfo
{
    public PBPlayerInfo(PersonalInfo personalInfo, PBRobotStatsSO robotStatsSO)
    {
        this.personalInfo = personalInfo;
        this.robotStatsSO = robotStatsSO;
    }

    public virtual PBRobotStatsSO robotStatsSO { get; protected set; }

    public override void ClearIngameData()
    {
        //Do nothing
    }
}

public class PBBotInfo : PBPlayerInfo
{
    public PBBotInfo(PersonalInfo personalInfo, PBRobotStatsSO chassisSO, PB_AIProfile aiProfile) : base(personalInfo, chassisSO)
    {
        this.aiProfile = aiProfile;
    }

    public PB_AIProfile aiProfile { get; set; }

}
