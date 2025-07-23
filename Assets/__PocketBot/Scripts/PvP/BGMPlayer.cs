using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Template;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    private void Awake()
    {
        GameEventHandler.AddActionEvent(SceneManagementEventCode.OnLoadSceneCompleted, HandleLoadSceneCompleted);
    }

    private void HandleLoadSceneCompleted(params object[] objs)
    {
        if (objs[0] is not string destinationSceneName) return;
        if (destinationSceneName == SceneName.MainScene.ToString())
        {
            SoundManager.Instance.PlayLoopSFX(SFX.BGM_MainScene, 0.75f, true, false, this.gameObject);
        }
        else if (destinationSceneName == SceneName.PvP.ToString() || destinationSceneName == SceneName.FTUE_PocketShooter.ToString())
        {
            SoundManager.Instance.PlayLoopSFX(SFX.BGM_BattleScene, 0.75f, true, false, this.gameObject);
        }
    }
}
