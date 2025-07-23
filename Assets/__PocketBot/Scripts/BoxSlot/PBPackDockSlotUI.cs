using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class PBPackDockSlotUI : PackDockSlotUI
{
    [SerializeField] protected EZAnimBase packThumbnailFillAnim;
    [SerializeField] protected EZAnimBase flashFillAnim;
    [SerializeField] protected List<GameObject> openNowTxtList;
    #region FTUE
    [SerializeField, BoxGroup("FTUE")] protected GameObject ftueHand;
    [SerializeField, BoxGroup("Box FTUE")] protected PPrefBoolVariable clickOpenBoxSlotFTUE;
    [SerializeField, BoxGroup("Box FTUE")] protected PPrefBoolVariable startUnlockBoxSlot;
    [SerializeField, BoxGroup("Box FTUE")] protected PPrefBoolVariable lootBoxSlotFTUE;
    [SerializeField, BoxGroup("Box PVP FTUE")] protected PPrefBoolVariable activeBoxSlotTheFirstTime;
    [SerializeField, BoxGroup("Box PVP FTUE")] protected PPrefBoolVariable clickOpenBoxSlotTheFirstTimeFTUE;
    [SerializeField, BoxGroup("Box PVP FTUE")] protected PPrefBoolVariable startUnlockBoxTheFirstTimeSlot;
    [SerializeField, BoxGroup("Box PVP FTUE")] protected PPrefBoolVariable lootBoxSlotTheFirstTimeFTUE;
    private IEnumerator DelayLootBoxFTUECR;
    private bool callOneTimeOpenBoxFTUE = false;
    private bool callOneTimeOpenBoxTheFirstTimeFTUE = false;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        GameEventHandler.AddActionEvent(GachaPackDockEventCode.OnGachaPackDockUpdated, OnGachaPackDockUpdated);
        OpenNowOfferManager.Instance.OnLimitedStateChanged += UpdateView;
        OpenNowOfferManager.Instance.OnPermanentStateChanged += UpdateView;
        packThumbnailFillAnim.gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEventHandler.RemoveActionEvent(GachaPackDockEventCode.OnGachaPackDockUpdated, OnGachaPackDockUpdated);
        if (OpenNowOfferManager.Instance != null)
        {
            OpenNowOfferManager.Instance.OnLimitedStateChanged -= UpdateView;
            OpenNowOfferManager.Instance.OnPermanentStateChanged -= UpdateView;
        }
    }

    public virtual void PlayFlashFX(float delay)
    {
        StartCoroutine(CommonCoroutine.Delay(delay, false, () =>
        {
            flashFillAnim.Play();
        }));
    }

    public override void Initialize(int gachaPackDockSlotIndex)
    {
        this.gachaPackDockSlotIndex = gachaPackDockSlotIndex;
        if (IsCanUpdate())
        {
            InitView();
            if (!gachaPackDockSlot.HasPlayedFillAnim)
            {
                gachaPackDockSlot.HasPlayedFillAnim = true;
                foreach (var item in stateContainers)
                {
                    if (item.Key == GachaPackDockSlotState.Empty)
                    {
                        item.Value.SetActive(true);
                    }
                    else
                    {
                        item.Value.SetActive(false);
                    }
                }
                packThumbnailFillAnim.gameObject.SetActive(true);
                packThumbnailFillAnim.Play(() => { UpdateView(); packThumbnailFillAnim.gameObject.SetActive(false); });
            }
            else
            {
                UpdateView();
            }
        }
    }

    public override void UpdateView()
    {
        base.UpdateView();
        bool isShowOpenNowTxt = OpenNowOfferManager.Instance.IsApplyingOpenNow;
        foreach (var item in openNowTxtList)
        {
            item.SetActive(isShowOpenNowTxt);
        }
        foreach (var item in unlockedDurationTxtList)
        {
            item.gameObject.SetActive(!isShowOpenNowTxt);
        }
    }

    protected override void InitView()
    {
        var gachaPack = gachaPackDockSlot.GachaPack;
        if (gachaPackDockSlot.State != GachaPackDockSlotState.Empty)
        {
            var thumbnailSprite = gachaPack.GetOriginalPackThumbnail();
            foreach (var item in thumbnailImgList)
            {
                item.sprite = thumbnailSprite;
            }
            var displayName = gachaPack.GetOriginalPackName().Split(" - ")[1];
            foreach (var item in namePackTxtList)
            {
                item.text = displayName;
            }
            var unlockedTimeSpan = TimeSpan.FromSeconds(gachaPack.UnlockedDuration);
            var unlockedTime = timeSpanFormat.Convert(unlockedTimeSpan);
            foreach (var item in unlockedDurationTxtList)
            {
                item.text = unlockedTime;
            }
        }

        StartCoroutine(LoaderFTUE());
    }

    protected virtual IEnumerator LoaderFTUE()
    {
        yield return new WaitForSeconds(0.05f);
        #region FTUE
        if (gachaPackDockSlot.State == GachaPackDockSlotState.WaitToUnlock)
        {
            if (!clickOpenBoxSlotFTUE.value)
            {
                ftueHand.SetActive(true);
                GameEventHandler.Invoke(LogFTUEEventCode.StartOpenBox1);
                GameEventHandler.Invoke(FTUEEventCode.OnOpenBoxSlotFTUE, this.gameObject);
            }

            if (clickOpenBoxSlotFTUE.value && !startUnlockBoxSlot.value)
            {
                ftueHand.SetActive(true);
                GameEventHandler.Invoke(FTUEEventCode.OnOpenBoxSlotFTUE, this.gameObject);
            }

            if (activeBoxSlotTheFirstTime.value && !clickOpenBoxSlotTheFirstTimeFTUE.value && transform.GetSiblingIndex() == 0)
            {
                ftueHand.SetActive(true);
                GameEventHandler.Invoke(LogFTUEEventCode.StartOpenBox2);
                GameEventHandler.Invoke(FTUEEventCode.OnClickBoxTheFisrtTime, this.gameObject);
            }

            if (activeBoxSlotTheFirstTime.value && clickOpenBoxSlotTheFirstTimeFTUE.value && transform.GetSiblingIndex() == 0 && !startUnlockBoxTheFirstTimeSlot.value)
            {
                ftueHand.SetActive(true);
                GameEventHandler.Invoke(FTUEEventCode.OnClickBoxTheFisrtTime, this.gameObject);
            }
        }

        if (clickOpenBoxSlotTheFirstTimeFTUE.value && !lootBoxSlotTheFirstTimeFTUE.value && gachaPackDockSlot.State == GachaPackDockSlotState.Unlocked && !callOneTimeOpenBoxTheFirstTimeFTUE && transform.GetSiblingIndex() == 0)
        {
            callOneTimeOpenBoxTheFirstTimeFTUE = true;
            ftueHand.SetActive(true);
            GameEventHandler.Invoke(FTUEEventCode.OnClickOpenBoxTheFirstTime, this.gameObject);
        }

        if (DelayLootBoxFTUECR != null)
            StopCoroutine(DelayLootBoxFTUECR);
        DelayLootBoxFTUECR = DelayLootBoxFTUE();
        StartCoroutine(DelayLootBoxFTUECR);
        #endregion
    }

    protected override void HandleBtnClicked()
    {
        bool ignoreClickSlot = false;

        bool ShouldIgnoreClickSlot()
        {
            return activeBoxSlotTheFirstTime.value &&
                   clickOpenBoxSlotTheFirstTimeFTUE.value &&
                   startUnlockBoxTheFirstTimeSlot.value &&
                   !lootBoxSlotTheFirstTimeFTUE.value &&
                   transform.GetSiblingIndex() == 0 &&
                   gachaPackDockSlot.State == GachaPackDockSlotState.Unlocking;
        }
        ignoreClickSlot = ShouldIgnoreClickSlot();

        if (ignoreClickSlot)
            return;

        if (gachaPackDockSlot.State == GachaPackDockSlotState.Unlocked)
        {
            #region Firebase Event
            if (gachaPackDockSlot.GachaPack != null)
            {
                string openType = "free";
                GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPackDockSlot.GachaPack, openType);
            }
            #endregion

            PackDockManager.Instance.OpenSlot(gachaPackDockSlot);
            GameEventHandler.Invoke(GachaPackDockEventCode.OnOpenByProgression, ((PBPackDockManager)PackDockManager.Instance).IsSemiStandardOpenStatus(gachaPackDockSlot));
        }
        GameEventHandler.Invoke(GachaPackDockEventCode.OnGachaPackDockSlotClicked, gachaPackDockSlot, this);

        #region FTUE
        ftueHand.SetActive(false);
        if (!clickOpenBoxSlotFTUE.value)
        {
            GameEventHandler.Invoke(FTUEEventCode.OnOpenBoxSlotFTUE);
        }

        if (clickOpenBoxSlotFTUE.value && !startUnlockBoxSlot.value)
        {
            GameEventHandler.Invoke(FTUEEventCode.OnOpenBoxSlotFTUE);
        }

        if (clickOpenBoxSlotFTUE.value && startUnlockBoxSlot.value && !lootBoxSlotFTUE.value)
        {
            GameEventHandler.Invoke(LogFTUEEventCode.EndOpenBox1);
            GameEventHandler.Invoke(LogFTUEEventCode.StartEquip_1);
            GameEventHandler.Invoke(FTUEEventCode.OnClickOpenBoxSlotFTUE);
        }

        if (activeBoxSlotTheFirstTime.value && !clickOpenBoxSlotTheFirstTimeFTUE.value && transform.GetSiblingIndex() == 0)
        {
            GameEventHandler.Invoke(FTUEEventCode.OnClickBoxTheFisrtTime);
        }

        if (activeBoxSlotTheFirstTime.value && clickOpenBoxSlotTheFirstTimeFTUE.value && !startUnlockBoxTheFirstTimeSlot)
        {
            GameEventHandler.Invoke(FTUEEventCode.OnClickBoxTheFisrtTime);
        }

        if (activeBoxSlotTheFirstTime.value && clickOpenBoxSlotTheFirstTimeFTUE.value && startUnlockBoxTheFirstTimeSlot.value && !lootBoxSlotTheFirstTimeFTUE.value)
        {
            GameEventHandler.Invoke(LogFTUEEventCode.EndOpenBox2);
            GameEventHandler.Invoke(FTUEEventCode.OnClickOpenBoxTheFirstTime);
            return;
        }

        #endregion
    }

    protected override void OnGachaPackDockUpdated()
    {
        base.OnGachaPackDockUpdated();
        #region FTUE
        if (clickOpenBoxSlotTheFirstTimeFTUE.value && !lootBoxSlotTheFirstTimeFTUE.value && gachaPackDockSlot.State == GachaPackDockSlotState.Unlocking && !callOneTimeOpenBoxTheFirstTimeFTUE && transform.GetSiblingIndex() == 0)
        {
            callOneTimeOpenBoxTheFirstTimeFTUE = true;
            GameEventHandler.Invoke(FTUEEventCode.OnClickOpenBoxTheFirstTime, this.gameObject);
        }

        if (clickOpenBoxSlotTheFirstTimeFTUE.value && !lootBoxSlotTheFirstTimeFTUE.value && gachaPackDockSlot.State == GachaPackDockSlotState.Unlocked && transform.GetSiblingIndex() == 0)
        {
            ftueHand.SetActive(true);
        }

        if (DelayLootBoxFTUECR != null)
            StopCoroutine(DelayLootBoxFTUECR);
        DelayLootBoxFTUECR = DelayLootBoxFTUE();
        StartCoroutine(DelayLootBoxFTUECR);
        #endregion
    }

    private IEnumerator DelayLootBoxFTUE()
    {
        if (clickOpenBoxSlotFTUE.value && !lootBoxSlotFTUE.value && gachaPackDockSlot.State == GachaPackDockSlotState.Unlocked && !callOneTimeOpenBoxFTUE)
        {
            yield return null;
            callOneTimeOpenBoxFTUE = true;
            ftueHand.SetActive(true);
            GameEventHandler.Invoke(FTUEEventCode.OnClickOpenBoxSlotFTUE, this.gameObject);
        }
    }
}
