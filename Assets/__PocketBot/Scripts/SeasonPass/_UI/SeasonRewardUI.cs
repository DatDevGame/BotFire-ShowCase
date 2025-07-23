using System.Collections.Generic;
using LatteGames;
using LatteGames.Tab;
using UnityEngine;
using UnityEngine.UI;

public class SeasonRewardUI : MonoBehaviour
{
    [SerializeField] SeasonPassSO seasonPassSO;
    [SerializeField] SeasonRewardCell seasonRewardCellPrefab;
    [SerializeField] RectTransform zeroMilestone;
    [SerializeField] Transform scrollViewContent;
    [SerializeField] Slider claimableZoneSlider;
    [SerializeField] Slider progressBarCoreImg;
    [SerializeField] TabContent tabContent;
    [SerializeField] float updateProgressAnimDuration;
    [SerializeField] AnimationCurve updateProgressAnimCurve;
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private RecycleCellUI recycleCellUI;

    List<SeasonRewardCell> seasonRewardCells;
    bool isInitialized = false;
    int previousMilestoneIndex = -1;
    Coroutine updateProgressCoroutine;
    int currentMilestoneIndex = 0;
    SeasonRewardMilestoneImg firstMilestone;

    public void InitOrRefresh()
    {
        if (!isInitialized)
        {
            Init();
        }
        else
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (!tabContent.isActive)
        {
            return;
        }
        if (seasonPassSO.seasonCurrency.value != seasonPassSO.data.seasonTokenUI)
        {
            if (updateProgressCoroutine != null)
            {
                StopCoroutine(updateProgressCoroutine);
            }
            var startProgress = seasonPassSO.data.seasonTokenUI;
            updateProgressCoroutine = StartCoroutine(CommonCoroutine.LerpFactor(updateProgressAnimDuration, t =>
            {
                UpdateProgress(Mathf.Lerp(startProgress, seasonPassSO.seasonCurrency.value, updateProgressAnimCurve.Evaluate(t)));
            }));
        }
    }

    public void Init()
    {
        isInitialized = true;

        firstMilestone = zeroMilestone.GetComponentInChildren<SeasonRewardMilestoneImg>();
        firstMilestone.Init(0);
        var insertedCells = new List<RecycleCellUI.InsertedCell>()
        {
            new RecycleCellUI.InsertedCell(){
                cell = zeroMilestone,
                index = 0
            }
        };
        recycleCellUI.OnUpdateCell += OnUpdateCell;
        recycleCellUI.Init(seasonRewardCellPrefab.gameObject, seasonPassSO.milestones.Count, insertedCells);

        seasonRewardCells = new List<SeasonRewardCell>(scrollViewContent.GetComponentsInChildren<SeasonRewardCell>());

        UpdateProgress(seasonPassSO.data.seasonTokenUI);
        tabContent.OnSetActive += OnSetActive;
    }

    void OnUpdateCell(RectTransform cell, int index)
    {
        SeasonRewardCell seasonRewardCell = cell.gameObject.GetComponent<SeasonRewardCell>();
        seasonRewardCell.Init(seasonPassSO.milestones[index], index + 1);
        UpdateMilestone(seasonRewardCell);
    }

    public void Reset()
    {
        var insertedCells = new List<RecycleCellUI.InsertedCell>()
        {
            new RecycleCellUI.InsertedCell(){
                cell = zeroMilestone,
                index = 0
            }
        };
        recycleCellUI.Init(seasonRewardCellPrefab.gameObject, seasonPassSO.milestones.Count, insertedCells);
        seasonRewardCells = new List<SeasonRewardCell>(scrollViewContent.GetComponentsInChildren<SeasonRewardCell>());

        UpdateProgress(seasonPassSO.data.seasonTokenUI);
    }

    void OnSetActive(bool isActive)
    {
        if (isActive)
        {
            Refresh();
            FocusCurrentMilestone();
        }
    }

    public void FocusCurrentMilestone()
    {
        int currentMilestoneIndex = 0;
        bool isOutMilestone = true;
        for (var i = 0; i < seasonPassSO.milestones.Count; i++)
        {
            var mileStone = seasonPassSO.milestones[i];
            if (seasonPassSO.seasonCurrency.value < mileStone.requiredAmount)
            {
                isOutMilestone = false;
                currentMilestoneIndex = i;
                break;
            }
            else if (seasonPassSO.seasonCurrency.value == mileStone.requiredAmount)
            {
                isOutMilestone = false;
                currentMilestoneIndex = i + 1;
                break;
            }
        }

        if (isOutMilestone)
        {
            currentMilestoneIndex = seasonPassSO.milestones.Count;
        }
        StartCoroutine(CommonCoroutine.Delay(0, false, () =>
        {
            m_ScrollRect.FocusAtPoint(recycleCellUI.GetCellPos(currentMilestoneIndex));
        }));
    }

    public void UpdateProgress(float amount)
    {
        seasonPassSO.data.seasonTokenUI = amount;
        var progressDelta = 1f / seasonPassSO.milestones.Count;
        var remainderProgress = 0f;
        bool isOutMilestone = true;

        for (var i = 0; i < seasonPassSO.milestones.Count; i++)
        {
            var mileStone = seasonPassSO.milestones[i];
            if (seasonPassSO.data.seasonTokenUI < mileStone.requiredAmount)
            {
                isOutMilestone = false;
                currentMilestoneIndex = i;
                var previousSeasonCurrency = i > 0 ? seasonPassSO.milestones[i - 1].requiredAmount : 0;
                remainderProgress = (seasonPassSO.data.seasonTokenUI - previousSeasonCurrency) / (mileStone.requiredAmount - previousSeasonCurrency);
                break;
            }
            else if (seasonPassSO.data.seasonTokenUI == mileStone.requiredAmount)
            {
                isOutMilestone = false;
                currentMilestoneIndex = i + 1;
                remainderProgress = 0;
                break;
            }
        }

        if (isOutMilestone)
        {
            currentMilestoneIndex = seasonPassSO.milestones.Count;
            remainderProgress = 1;
        }

        float currentMilestoneProgress = (float)currentMilestoneIndex / seasonPassSO.milestones.Count;
        float currentProgress = currentMilestoneProgress + remainderProgress * progressDelta;

        // Update UI
        progressBarCoreImg.value = currentProgress;

        if (previousMilestoneIndex != currentMilestoneIndex)
        {
            previousMilestoneIndex = currentMilestoneIndex;
            claimableZoneSlider.value = 1 - currentMilestoneProgress;

            for (var i = 0; i < seasonRewardCells.Count; i++)
            {
                var seasonRewardCell = seasonRewardCells[i];
                UpdateMilestone(seasonRewardCell);
            }
        }
    }

    void UpdateMilestone(SeasonRewardCell seasonRewardCell)
    {
        var mileStoneState = MileStoneState.Unreached;
        if (seasonRewardCell.MilestoneImg.Index < currentMilestoneIndex)
        {
            mileStoneState = MileStoneState.Reached;
            if (seasonRewardCell.MilestoneImg.Index > 0)
            {
                seasonRewardCell.TryToUnlock();
            }
        }
        else if (seasonRewardCell.MilestoneImg.Index == currentMilestoneIndex)
        {
            mileStoneState = MileStoneState.Current;
            if (seasonRewardCell.MilestoneImg.Index > 0)
            {
                seasonRewardCell.TryToUnlock();
            }
        }
        seasonRewardCell.MilestoneImg.SetState(mileStoneState);
        if (currentMilestoneIndex > 0)
        {
            firstMilestone.SetState(MileStoneState.Reached);
        }
        else
        {
            firstMilestone.SetState(MileStoneState.Current);
        }
    }
}
