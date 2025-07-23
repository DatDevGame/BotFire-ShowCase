using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NoAdsOffersPopup : Singleton<NoAdsOffersPopup>
{
    [SerializeField] HighestAchievedPPrefFloatTracker highestTrophyAchieved;
    [SerializeField] PPrefIntVariable interstitialCount;
    [SerializeField] PPrefBoolVariable removeAdsPPref;
    [SerializeField] PPrefBoolVariable hasShowedFirstTime;
    [SerializeField] Button closeBtn;
    [SerializeField] CanvasGroupVisibility visibility;
    [SerializeField] int interstitialAmountPerShowTime = 4;
    [SerializeField] int discountedTrophyThreshold = 840;
    [SerializeField] Transform pivot;
    [SerializeField] Canvas rootCanvas;

    public PPrefBoolVariable HasShowedFirstTime => hasShowedFirstTime;
    public PPrefBoolVariable RemoveAdsPPref => removeAdsPPref;
    public Canvas RootCanvas => rootCanvas;
    public bool hasRemovedAds => removeAdsPPref.value;
    public bool isEnoughInterstitialToShow => interstitialCount.value >= interstitialAmountPerShowTime;
    public bool isEnoughTrophyToDiscount => highestTrophyAchieved.value >= discountedTrophyThreshold;

    Vector3 originalPivotPos;
    Transform noAdsOpenButton;
    bool isShowing = false;
    Coroutine ShowCoroutine;

    protected override void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        base.Awake();
        closeBtn.onClick.AddListener(OnCloseBtnClick);
        originalPivotPos = pivot.position;
        ObjectFindCache<NoAdsOffersPopup>.Add(this);
    }

    private void OnDestroy()
    {
        closeBtn.onClick.RemoveListener(OnCloseBtnClick);
        ObjectFindCache<NoAdsOffersPopup>.Remove(this);
    }

    void OnCloseBtnClick()
    {
        if (noAdsOpenButton != null)
        {
            HideFromOpenButtonMainScene();
        }
        else
        {
            visibility.Hide();
            isShowing = false;
        }
    }

    public void Show()
    {
        pivot.transform.localScale = Vector3.one;
        pivot.transform.position = originalPivotPos;
        visibility.Show();
        isShowing = true;
    }

    public void ShowBeforeAds(Action callback)
    {
        bool hasCalled = false;
        void CallBack()
        {
            if (!hasCalled)
            {
                hasCalled = true;
                callback?.Invoke();
            }
        }
        try
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            if (ShowCoroutine != null)
            {
                StopCoroutine(ShowCoroutine);
            }
            ShowCoroutine = StartCoroutine(CR_ShowBeforeAds(CallBack));
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            CallBack();
        }
    }

    IEnumerator CR_ShowBeforeAds(Action callback)
    {
        if (hasRemovedAds)
        {
            callback?.Invoke();
            yield break;
        }

        if (hasShowedFirstTime.value && !isEnoughInterstitialToShow)
        {
            interstitialCount.value++;
            if (isEnoughInterstitialToShow)
            {
                Show();
                yield return new WaitUntil(() => !isShowing);
                interstitialCount.value = 0;
            }

            callback?.Invoke();
            yield break;
        }
        else
        {
            interstitialCount.value = 0;
        }

        if (FTUEMainScene.Instance != null && FTUEMainScene.Instance.IsShowingFTUE)
        {
            GameEventHandler.AddActionEvent(SceneManagementEventCode.OnStartLoadScene, HandleStartLoadScene);
            yield return new WaitUntil(() => FTUEMainScene.Instance != null && !FTUEMainScene.Instance.IsShowingFTUE);
        }

        hasShowedFirstTime.value = true;
        Show();
        yield return new WaitUntil(() => !isShowing);
        callback?.Invoke();
    }

    private void HandleStartLoadScene(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        GameEventHandler.RemoveActionEvent(SceneManagementEventCode.OnStartLoadScene, HandleStartLoadScene);
        if (parameters[0] is string destinationScene && destinationScene != SceneName.MainScene.ToString())
        {
            if (ShowCoroutine != null)
            {
                StopCoroutine(ShowCoroutine);
            }
            ShowCoroutine = null;
        }
    }

    void HideFromOpenButtonMainScene()
    {
        visibility.Hide();
        pivot.transform.DOKill();
        pivot.transform.DOScale(0, AnimationDuration.TINY).SetEase(Ease.OutSine);
        pivot.transform.DOMove(noAdsOpenButton.position, AnimationDuration.TINY).SetEase(Ease.OutSine);
        noAdsOpenButton = null;
    }

    public void ShowFromOpenButtonMainScene(Transform btn)
    {
        if (removeAdsPPref.value)
        {
            return;
        }
        noAdsOpenButton = btn;

        visibility.Show();
        pivot.transform.DOKill();
        pivot.transform.localScale = Vector3.zero;
        pivot.transform.position = noAdsOpenButton.position;
        pivot.transform.DOScale(1, AnimationDuration.TINY).SetEase(Ease.InSine);
        pivot.transform.DOMove(originalPivotPos, AnimationDuration.TINY).SetEase(Ease.InSine);
    }
}
