using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using Sirenix.OdinInspector;

public class BossFightManager : Singleton<BossFightManager>
{
    public BossMapSO bossMapSO;
    public ItemSOVariable bossChassisInUse;
    public PBRobotStatsSO bossRobotStatsSO;
    public PlayerInfoVariable infoOfBoss;
    public ModeVariable currentChosenModeVariable;
    public BossSO bossSOSelected;

    [SerializeField, BoxGroup("Select Boss Lock")] public ItemSOVariable select_bossChassisLock;
    [SerializeField, BoxGroup("Select Boss Lock")] public PBRobotStatsSO select_PBRobotStatsSO;
    [SerializeField, BoxGroup("Select Boss Lock")] public PlayerInfoVariable select_InfoOfBossSelect;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable pPrefBoolBattleBossFightingUI;

    private Dictionary<string, BossSO> _bossSOByKeyDic;

    public int selectingChapterIndex;

    protected override void Awake()
    {
        if (!pPrefBoolBattleBossFightingUI.value)
        {
            if (bossMapSO.chapterIndex > 0 || bossMapSO.currentChapterSO.bossIndex > 0)
            {
                pPrefBoolBattleBossFightingUI.value = true;
            }
        }
        GameEventHandler.AddActionEvent(BossFightEventCode.OnSelectBossLockInventory, SetInfoBossSelect);
        if (bossChassisInUse.value == null)
        {
            UpdateInfoOfBoss();
        }
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnSelectBossLockInventory, SetInfoBossSelect);
    }

    public void LogBossFightOutcome(bool isVictory)
    {
        if (isVictory)
        {
            GameEventHandler.Invoke(BossEventCode.CompleteBossFight, bossMapSO);
            return;
        }
        GameEventHandler.Invoke(BossEventCode.FailBossFight, bossMapSO);
    }

    public void DefeatCurrentBoss()
    {
        var currentChapterSO = bossMapSO.currentChapterSO;
        if (currentChapterSO.bossIndex >= currentChapterSO.bossCount - 1)
        {
            if (bossMapSO.chapterIndex < bossMapSO.chapterCount - 1)
            {
                bossMapSO.chapterIndex.value++;
                currentChapterSO.bossIndex.value++;
            }
        }
        else
        {
            currentChapterSO.bossIndex.value++;
        }
    }

    public void OnSelectCurrentBoss()
    {
        UpdateInfoOfBoss();
        GameEventHandler.Invoke(BossEventCode.StartBossFight, bossMapSO);
    }

    public void UpdateInfoOfBoss()
    {
        var bossSO = bossMapSO.currentBossSO;
        bossChassisInUse.value = bossSO.chassisSO;
        bossRobotStatsSO.ForceUpdateStats();
        infoOfBoss.value = new PBBotInfo(bossSO.botInfo, bossRobotStatsSO, bossSO.AIProfile);
    }

    private void SetInfoBossSelect(params object[] obj)
    {
        if (obj.Length <= 0 || obj[0] == null) return;
        if (obj[0] is not PBChassisSO) return;

        PBChassisSO pBChassisSO = obj[0] as PBChassisSO;
        string displayNameBoss = pBChassisSO.GetModule<NameItemModule>().displayName;

        BossSO bossSO = null;
        for (int i = 0; i < bossMapSO.chapterList.Count; i++)
        {
            bossSO = bossMapSO.chapterList[i].bossList.Find(v => v.chassisSO == pBChassisSO);
            if (bossSO != null)
                break;
        }
        bossSOSelected = bossSO;

        select_bossChassisLock.value = pBChassisSO;

        if (bossSO != null)
        {
            select_InfoOfBossSelect.value = new PBBotInfo(bossSO.botInfo, select_PBRobotStatsSO, bossSO.AIProfile);
        }
    }

    public BossSO GetBossSOByKey(string key)
    {
        if (_bossSOByKeyDic == null)
        {
            _bossSOByKeyDic = new Dictionary<string, BossSO>();
            foreach(BossChapterSO bossChapterSO in bossMapSO.chapterList)
            {
                foreach(BossSO bossSO1 in bossChapterSO.bossList)
                {
                    _bossSOByKeyDic.Add(bossSO1.Key, bossSO1);
                }
            }
        }
        if (_bossSOByKeyDic.TryGetValue(key, out BossSO bossSO)) return bossSO;
        return null;
    }
}
