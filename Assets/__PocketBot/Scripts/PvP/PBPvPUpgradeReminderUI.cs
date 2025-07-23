using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GachaSystem.Core;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class PBPvPUpgradeReminderUI : MonoBehaviour
{
    [SerializeField] DOTweenAnimation pulseAnimation;
    [SerializeField] GameObject hasButtonGroup, noButtonGroup;
    [SerializeField] List<PBPartManagerSO> partsManagers;
    [SerializeField] Button upgradeNowBtn;
    [SerializeField] PPrefIntVariable firstArenaScriptedGachaPacksIndex;
    [SerializeField] LayoutElement layoutElement;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        upgradeNowBtn.onClick.AddListener(OnUpgradeNowBtnClicked);
        layoutElement.ignoreLayout = true;
        hasButtonGroup.SetActive(false);
        noButtonGroup.SetActive(false);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        upgradeNowBtn.onClick.RemoveListener(OnUpgradeNowBtnClicked);
    }

    private void OnUpgradeNowBtnClicked()
    {
        #region Firebase Event
        GameEventHandler.Invoke(LogFirebaseEventCode.UpgradeNowClicked);
        #endregion

        ObjectFindCache<PBPvPStateGameController>.Get().LeaveGame();
        MainSceneRemindUpgrade.HasClickedUpgradeBtn = true;
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (firstArenaScriptedGachaPacksIndex.value <= 0)
            return;
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (!matchOfPlayer.isAbleToComplete)
            return;
        var isVictory = matchOfPlayer.isVictory;
        if (isVictory)
            return;
        //layoutElement.ignoreLayout = false;
        hasButtonGroup.SetActive(IsAtLeastOnePartUpgradeable());
        noButtonGroup.SetActive(!IsAtLeastOnePartUpgradeable());
        StartCoroutine(CommonCoroutine.WaitUntil(() => transform.localScale == Vector3.one, () =>
        {
            pulseAnimation.DOPlay();

            #region Firebase Event
            GameEventHandler.Invoke(LogFirebaseEventCode.UpgradeNowShown);
            #endregion
        }));
    }

    private bool IsAtLeastOnePartUpgradeable()
    {
        foreach (var partManager in partsManagers)
        {
            foreach (var part in partManager.Parts)
            {
                if (part.IsEnoughCardToUpgrade())
                {
                    return true;
                }
            }
        }
        return false;
    }
}
