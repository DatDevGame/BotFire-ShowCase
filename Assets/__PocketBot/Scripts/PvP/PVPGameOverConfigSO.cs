using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PVPGameOverConfigSO", menuName = "PocketBots/PVP/PVPGameOverConfigSO")]
public class PVPGameOverConfigSO : SerializableScriptableObject
{
    [BoxGroup("Win Camera"), LabelText("Time Move Camera Win Cam")] public float TimeMoveWinCamDuration;
    [BoxGroup("Win Camera"), LabelText("Time Wait From WinCame -> Character")] public float TimeEndWaitWinCamera;

    [BoxGroup("Character"), LabelText("Time Wait Character")] public float TimeWaitCharacter;
}
