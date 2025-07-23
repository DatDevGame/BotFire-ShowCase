using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FTUEPreludeSeason : MonoBehaviour
{
    public static Action<GameObject> ShowDoMissionFTUE;

    [SerializeField] GameObject ftueHand;
    [SerializeField] PPrefBoolVariable FTUE_SeasonPassTab;
    [SerializeField] Button button;
    [SerializeField] SeasonPreludeUI seasonPreludeUI;

    private void Awake()
    {
        button.onClick.AddListener(OnClaimButtonClicked);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClaimButtonClicked);
    }

    private void Start()
    {
        StartCoroutine(CR_FTUE());
    }

    IEnumerator CR_FTUE()
    {
        if (!FTUE_SeasonPassTab.value && SeasonPassManager.Instance.seasonPassSO.data.isNewUser)
        {
            var hasEndedPreludeTabFTUE = false;
            FTUESeasonPassTabButton.OnEndFTUE += OnEndFTUE;
            yield return new WaitUntil(() => hasEndedPreludeTabFTUE);

            #region FTUE Event
            GameEventHandler.Invoke(LogFTUEEventCode.StartPreludeSeason_ClaimRewards);
            #endregion

            GameEventHandler.Invoke(BlockBackGround.LockWhiteHasObject, button.gameObject);
            ftueHand.SetActive(true);
            yield return new WaitUntil(() => FTUE_SeasonPassTab.value);

            #region FTUE Event
            GameEventHandler.Invoke(LogFTUEEventCode.EndPreludeSeason_ClaimRewards);
            #endregion

            ftueHand.SetActive(false);
            GameEventHandler.Invoke(BlockBackGround.LockWhiteHasObject);
            GameEventHandler.Invoke(BlockBackGround.LockWhiteHasObject, ftueHand.gameObject);

            //Fix: Claim button not hide.
            button.gameObject.SetActive(false);
            yield return null;
            button.gameObject.SetActive(true);

            seasonPreludeUI.OnCompletedClaiming += OnCompleteClaiming;

            void OnEndFTUE()
            {
                hasEndedPreludeTabFTUE = true;
                FTUESeasonPassTabButton.OnEndFTUE -= OnEndFTUE;
            }

            void OnCompleteClaiming()
            {
                GameEventHandler.Invoke(BlockBackGround.LockWhiteHasObject);
                seasonPreludeUI.OnCompletedClaiming -= OnCompleteClaiming;
                ShowDoMissionFTUE?.Invoke(seasonPreludeUI.MissionCells[0].gameObject);
            }
        }
    }

    void OnClaimButtonClicked()
    {
        if (!FTUE_SeasonPassTab.value)
        {
            FTUE_SeasonPassTab.value = true;
        }
    }
}
