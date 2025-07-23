using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RobotStatsSO", menuName = "PocketBots/VariableSO/RobotStatsSO_Preview")]
public class PBRobotStatsSO_Preview : PBRobotStatsSO
{
    public override IPartStats stats
    {
        get
        {
            m_CombinationRobotStats = new CompoundRobotStats(statsOfRobot);
            return m_CombinationRobotStats;
        }
        set
        {
            m_CombinationRobotStats = value;
        }
    }
}
