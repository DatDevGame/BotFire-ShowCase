using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvPCheckBotImmobilized_FTUE : PvPCheckBotImmobilized
{
    protected override void HandleCompetitorJoined(object[] parameters)
    {
        if (parameters[0] is not Competitor competitor) return;
        var robot = competitor as PBRobot;
        if (robots.Contains(robot)) return;
        if (robot.PersonalInfo.isLocal == false) return;
        robots.Add(robot);
    }
}
