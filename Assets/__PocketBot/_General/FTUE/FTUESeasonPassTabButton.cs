using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using UnityEngine.UI;
using System;

public class FTUESeasonPassTabButton : MonoBehaviour
{
    public static Action OnEndFTUE;

    [SerializeField] GameObject ftueHand;
    [SerializeField] PPrefBoolVariable FTUE_SeasonPassTab;
    [SerializeField] Button tabButton;
    [SerializeField] CustomizeDockerButton dockerButton;
    [SerializeField] Sprite lockIcon;
    [SerializeField] Sprite preludeIcon;
    [SerializeField] Sprite seasonIcon;
    SeasonPassState state => SeasonPassManager.Instance.seasonPassSO.data.state;
    bool hasClickedTabBtn;
    private void Awake()
    {
        tabButton.onClick.AddListener(OnTabButtonClicked);
    }

    private void OnDestroy()
    {
        tabButton.onClick.RemoveListener(OnTabButtonClicked);
    }

    private void Start()
    {
        StartCoroutine(CR_FTUE());
    }

    IEnumerator CR_FTUE()
    {
        if (!FTUE_SeasonPassTab.value)
        {
            dockerButton.SetOverrideIcon(lockIcon);
            tabButton.interactable = false;
            yield return new WaitUntil(() => SeasonPassManager.Instance.isUnlockSeasonPass);
            dockerButton.SetOverrideIcon(state == SeasonPassState.PreludeSeason || state == SeasonPassState.None ? preludeIcon : seasonIcon);
            tabButton.interactable = true;

            //FTUE
            GameEventHandler.Invoke(LogFTUEEventCode.StartPreludeSeason);
            //
            if (SeasonPassManager.Instance.seasonPassSO.data.isNewUser)
            {
                #region FTUE Event
                GameEventHandler.Invoke(LogFTUEEventCode.StartPreludeSeason_Explore);
                #endregion

                GameEventHandler.Invoke(FTUEEventCode.OnPreludeSeasonUnlocked_FTUE, this.gameObject);
            }
            ftueHand.SetActive(true);
            yield return new WaitUntil(() => hasClickedTabBtn);

            //FTUE
            GameEventHandler.Invoke(LogFTUEEventCode.EndPreludeSeason);
            //
            if (SeasonPassManager.Instance.seasonPassSO.data.isNewUser)
            {
                #region FTUE Event
                GameEventHandler.Invoke(LogFTUEEventCode.EndPreludeSeason_Explore);
                #endregion

                GameEventHandler.Invoke(FTUEEventCode.OnPreludeSeasonUnlocked_FTUE);
            }
            ftueHand.SetActive(false);
            OnEndFTUE?.Invoke();
        }
        else
        {
            dockerButton.SetOverrideIcon(state == SeasonPassState.PreludeSeason || state == SeasonPassState.None ? preludeIcon : seasonIcon);
            tabButton.interactable = true;
        }

        if (state == SeasonPassState.PreludeSeason || state == SeasonPassState.None)
        {
            while (state == SeasonPassState.PreludeSeason || state == SeasonPassState.None)
            {
                yield return null;
            }
            yield return new WaitUntil(() => state != SeasonPassState.PreludeSeason && state != SeasonPassState.None);
            dockerButton.SetOverrideIcon(seasonIcon);
            tabButton.interactable = true;
        }
    }

    void OnTabButtonClicked()
    {
        hasClickedTabBtn = true;
    }
}
