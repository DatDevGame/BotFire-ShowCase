using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames;
using HyrphusQ.SerializedDataStructure;
using System.Linq;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "BossFightStreak", menuName = "PocketBots/BossFight/BossFightStreak")]
public class BossFightStreakHandle : PPrefIntVariable
{
    public BossMapSO bossMapSO;

    private string GetKey(int bossIndex, string chapterName) => $"BossFightStreak{GetHashCode()}-{bossIndex}-{chapterName}";

    public virtual void ResetAllStreak()
    {
        for (int i = 0; i < bossMapSO.chapterList.Count; i++)
        {
            BossChapterSO bossChapterSO = bossMapSO.chapterList[i];
            for (int x = 0; x < bossChapterSO.bossList.Count; x++)
            {
                PlayerPrefs.SetInt(GetKey(x, bossMapSO.chapterList[i].chapterName), 0);
            }
        }
    }

    public virtual int GetStreakBoss(BossChapterSO bossChapterSO)
    {
        int bossID = bossChapterSO.bossIndex.value;
        string key = GetKey(bossID, bossChapterSO.chapterName);

        if (value != bossID)
        {
            ResetAllStreak();
            value = bossID;
        }

        int streakIndex = PlayerPrefs.GetInt(key) + 1;
        PlayerPrefs.SetInt(key, streakIndex);
        return streakIndex;
    }

    public override void Clear()
    {
        base.Clear();
        ResetAllStreak();
    }
}
