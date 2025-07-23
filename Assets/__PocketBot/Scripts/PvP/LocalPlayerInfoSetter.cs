using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerInfoSetter : MonoBehaviour
{
    [SerializeField]
    private PlayerDatabaseSO m_PlayerDatabaseSO;
    [SerializeField]
    private PBRobotStatsSO m_RobotStatsOfLocalPlayer;
    [SerializeField]
    private PlayerInfoVariable m_PlayerInfoOfLocalPlayer;

    private void Start()
    {
        m_PlayerInfoOfLocalPlayer.value = new PBPlayerInfo(m_PlayerDatabaseSO.localPlayerPersonalInfo, m_RobotStatsOfLocalPlayer);
    }
}