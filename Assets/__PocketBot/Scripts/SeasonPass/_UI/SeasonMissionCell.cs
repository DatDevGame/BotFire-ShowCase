using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonMissionCell : MonoBehaviour
{
    public Action OnInverseUpdated;
    public Action<SeasonMissionCell> OnSkipAction;

    [SerializeField] Animator animator;
    [SerializeField] Image iconImg;
    [SerializeField] Image bgImg;
    [SerializeField] GameObject tickImg;
    [SerializeField] TMP_Text descriptionTxt;
    [SerializeField] Slider progressSlider;
    [SerializeField] TMP_Text progressTxt;
    [SerializeField] Sprite completedSprite;
    [SerializeField] Sprite normalSprite;
    [SerializeField, BoxGroup("Replace")] bool canReplace;
    [SerializeField, BoxGroup("Replace")] EZAnimBase showAnim;
    [SerializeField, BoxGroup("Replace")] Button replaceBtn;
    [SerializeField, BoxGroup("Replace")] Button keepBtn;
    [SerializeField, BoxGroup("Replace")] RVButtonBehavior skipBtn;
    [SerializeField, BoxGroup("Reward")] Image rewardIconImg;
    [SerializeField, BoxGroup("Reward")] GameObject rewardGroup;
    [SerializeField, BoxGroup("Reward")] GameObject shineFX;
    [SerializeField, BoxGroup("Reward")] TMP_Text rewardAmountTxt;
    [SerializeField, BoxGroup("Reward")] EZAnimSequence showDoubleTxt;
    [SerializeField, BoxGroup("Reward")] EZAnimBase hideRewardTxt;
    [SerializeField, BoxGroup("Reward")] EZAnimBase showTickTxt;
    [SerializeField, BoxGroup("Helper")] Button helperBtn;
    [SerializeField, BoxGroup("Helper")] TMP_Text helperBtnTxt;


    bool isShowHelperBtn = true;
    Action<SeasonMissionCell> OnWatchedSkipAds;
    MissionData _mission;
    Coroutine updateProgressCoroutine;
    State _state = State.None;

    public Func<bool> isShowing = null;
    public MissionData mission => _mission;
    public Image RewardIconImg => rewardIconImg;
    public State state
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                if (_state == State.RewardClaimed)
                {
                    rewardGroup.SetActive(false);
                    hideRewardTxt.SetToEnd();
                    showDoubleTxt.SetToEnd();

                    tickImg.SetActive(true);
                    showTickTxt.SetToEnd();
                    bgImg.sprite = normalSprite;

                    replaceBtn.gameObject.SetActive(false);
                    helperBtn.gameObject.SetActive(false);
                }
                else if (_state == State.Completed)
                {
                    rewardGroup.SetActive(true);
                    shineFX.SetActive(true);
                    animator.SetBool("Breath", true);
                    hideRewardTxt.SetToStart();
                    showDoubleTxt.SetToStart();

                    tickImg.SetActive(false);
                    showTickTxt.SetToStart();
                    bgImg.sprite = completedSprite;

                    replaceBtn.gameObject.SetActive(false);
                    helperBtn.gameObject.SetActive(false);
                }
                else if (_state == State.Uncompleted)
                {
                    rewardGroup.SetActive(true);
                    shineFX.SetActive(false);
                    animator.SetBool("Breath", false);
                    hideRewardTxt.SetToStart();
                    showDoubleTxt.SetToStart();

                    tickImg.SetActive(false);
                    showTickTxt.SetToStart();
                    bgImg.sprite = normalSprite;

                    replaceBtn.gameObject.SetActive(canReplace);
                }
            }
        }
    }


    private void Awake()
    {
        helperBtn.onClick.AddListener(OnClickedHelperBtn);
        replaceBtn.onClick.AddListener(OnClickedReplaceBtn);
        keepBtn.onClick.AddListener(OnClickedKeepBtn);
        skipBtn.OnRewardGranted += OnWatchedSkipBtnAds;
    }

    private void OnDestroy()
    {
        helperBtn.onClick.RemoveListener(OnClickedHelperBtn);
        replaceBtn.onClick.RemoveListener(OnClickedReplaceBtn);
        keepBtn.onClick.RemoveListener(OnClickedKeepBtn);
        skipBtn.OnRewardGranted -= OnWatchedSkipBtnAds;
        if (mission != null)
        {
            mission.onMissionProgressUpdated -= MissionProgressUpdated;
        }
    }

    void OnClickedHelperBtn()
    {
        MissionHelper.Instance.GoToPartInfoUI(mission);
    }

    void OnClickedReplaceBtn()
    {
        showAnim.Play();

        #region Firebase Event
        try
        {
            int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
            string missionType = mission.scope switch
            {
                MissionScope.Daily => "today",
                MissionScope.Weekly => "weekly",
                MissionScope.Season => "season",
                _ => "null"
            };
            string missionName = mission.description;
            GameEventHandler.Invoke(LogFirebaseEventCode.RefreshMissionClicked, seasonID, missionType, missionName);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    void OnClickedKeepBtn()
    {
        showAnim.InversePlay();
    }

    void OnWatchedSkipBtnAds(RVButtonBehavior.RewardGrantedEventData data)
    {
        #region MonetizationEventCode
        try
        {
            string missionType = _mission.scope switch
            {
                MissionScope.Daily => "Today",
                MissionScope.Weekly => "Weekly",
                MissionScope.Season => "Season",
                _ => "Null"
            };
            string missionID = _mission.targetType.ToString();
            GameEventHandler.Invoke(MonetizationEventCode.SkipMission, missionType, missionID);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
        OnSkipAction?.Invoke(this);
        showAnim.InversePlay();
        OnWatchedSkipAds?.Invoke(this);
    }

    public void Init(MissionData mission, CurrencyType rewardCurrencyType, bool canReplace = true, Action<SeasonMissionCell> OnWatchedSkipAds = null, bool isShowHelperBtn = true)
    {
        if (this._mission != null)
        {
            mission.onMissionProgressUpdated -= MissionProgressUpdated;
        }
        this.OnWatchedSkipAds = OnWatchedSkipAds;
        this._mission = mission;
        this.canReplace = canReplace;
        this.isShowHelperBtn = isShowHelperBtn;
        state = State.None;
        helperBtnTxt.text = MissionHelper.Instance.GetHelperBtnText(mission);
        iconImg.sprite = MissionManager.Instance.GetMissionIcon(mission.targetType);
        descriptionTxt.text = mission.description;
        replaceBtn.gameObject.SetActive(canReplace);
        rewardIconImg.sprite = CurrencyManager.Instance.GetCurrencySO(rewardCurrencyType).icon;
        rewardAmountTxt.text = mission.currencyRewardAmount.ToRoundedText();
        mission.onMissionProgressUpdated += MissionProgressUpdated;
        UpdateView();
    }

    void MissionProgressUpdated()
    {
        if (isShowing != null && isShowing())
        {
            UpdateProgress(AnimationDuration.SSHORT, () => OnInverseUpdated?.Invoke());
        }
    }

    public void UpdateViewManually(float progress)
    {
        progressSlider.value = progress / mission.targetValue;
        progressTxt.text = progress.ToRoundedText();

        if (progressSlider.value >= 1)
        {
            state = State.Completed;
        }
        else
        {
            state = State.Uncompleted;
        }
    }

    public void UpdateView()
    {
        if (mission.isRewardClaimed)
        {
            state = State.RewardClaimed;
        }
        else if (mission.isCompletedUI)
        {
            state = State.Completed;
        }
        else
        {
            state = State.Uncompleted;
            helperBtn.gameObject.SetActive(isShowHelperBtn && MissionHelper.Instance.IsShowHelperBtn(mission));
        }

        progressSlider.value = mission.progressUI / mission.targetValue;
        progressTxt.text = mission.progressUI.ToRoundedText();
    }

    public void UpdateProgress(float duration = AnimationDuration.TINY, Action callback = null)
    {
        if (updateProgressCoroutine != null)
        {
            StopCoroutine(updateProgressCoroutine);
        }
        updateProgressCoroutine = StartCoroutine(CR_UpdateProgress(duration, callback));
    }

    IEnumerator CR_UpdateProgress(float duration, Action callback = null)
    {
        if (mission.progressUI != mission.progress)
        {
            var t = 0f;
            var startProgress = mission.progressUI;
            while (t <= 1)
            {
                t += Time.deltaTime / duration;
                mission.progressUI = Mathf.Lerp(startProgress, mission.progress, t);
                UpdateView();
                yield return null;
            }
        }
        else
        {
            UpdateView();
        }
        callback?.Invoke();
    }

    public void ShowDoubleRewardText(Action callback)
    {
        replaceBtn.gameObject.SetActive(false);
        showDoubleTxt.Play(callback);
    }

    public void HideRewardAndShowTick(Action callback)
    {
        replaceBtn.gameObject.SetActive(false);
        tickImg.SetActive(true);
        hideRewardTxt.Play(() =>
        {
            showTickTxt.Play(callback);
        });
    }

    public enum State : byte
    {
        None,
        RewardClaimed,
        Completed,
        Uncompleted
    }
}
