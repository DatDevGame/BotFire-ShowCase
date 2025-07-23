using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GachaSystem.Core;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Monetization;
using LatteGames.UnpackAnimation;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using LatteGames.Template;

public class UnlockBossScene : MonoBehaviour
{
    public static Action OnSkipClicked;

    public static bool IsAbleToClaimBossByRV = true;
    private const string premiumIcon = "<size=70><voffset=-7><sprite name=Premium></voffset></size>";

    [SerializeField] private RVButtonBehavior claimRVButton;
    [SerializeField] private BossClaimByRVSO bossClaimByRVSO;
    [SerializeField] GameObject container, modelContainer;
    [SerializeField] Button nextBtn, claimByRVBtn, claimByPremium, skipBtn, claimNotAdsBtn;
    [SerializeField] TextMeshProUGUI claimByRVAmountTxt, claimByPremiumAmountTxt;
    [SerializeField] ScaleGridLayoutGroup scaleGridLayoutGroup;
    [SerializeField] PBGachaBoxModel specialBotModel;
    [SerializeField] Camera camera;
    [SerializeField] float dropDuration;
    [SerializeField] AnimationCurve bagDroppingCurve, camFollowingCurve;
    [SerializeField] PBModelRenderer opponentModelPrefab, bossSelectLock;
    [SerializeField] PlayerInfoVariable infoOfBoss, infoBossLockSelect;
    [SerializeField] LayerMask openPackLayer;
    [SerializeField] CanvasGroupVisibility nonInteractiveVisibility, interactiveVisibility;
    [SerializeField] List<AbstractCardController> cardControllers;
    [SerializeField] AnimationCurve cellScaleAnimationCurve;
    [SerializeField] ResourceLocationProvider resourceLocationProvider;
    [SerializeField] private GameObject iconLoadingAds;
    [SerializeField] LockedChainController lockedChainControllerPrefab;

    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility cardGridCanvasGroup;

    [BoxGroup("Unlock Effect")] private GameObject unlockedGlowingObject;
    [SerializeField, BoxGroup("Unlock Effect")] private GameObject backGroundUnlocked;
    [SerializeField, BoxGroup("Unlock Effect")] private TMP_Text unlockedtxt;
    [SerializeField, BoxGroup("Unlock Effect")] private ParticleSystem hightLightVFX;
    [SerializeField, BoxGroup("Unlock Effect")] private SpiralShapeParticleEmitter unlockBossVFX;
    [SerializeField, BoxGroup("Unlock Effect")] private Image darkenImage;
    [SerializeField, BoxGroup("Unlock Effect")] private CanvasGroupVisibility nextButtonCanvasGroup;

    LockedChainController lockedChainController;
    private GameObject objectPreview;
    Sequence sequence;
    private BossSO bossSO => isSelectCharacterUI ? BossFightManager.Instance.bossSOSelected : BossFightManager.Instance.bossMapSO.previousBossSO;
    private float originalScale;
    private Vector2 originalSpacing;
    public RectTransform rect => GetComponent<RectTransform>();
    private bool isSelectCharacterUI = false;
    private GameObject modelPreview;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(BossFightEventCode.OnUnlockBoss, OnUnlockBoss);
        nextBtn.onClick.AddListener(OnNextButton);
        skipBtn.onClick.AddListener(OnSkipButton);
        claimByPremium.onClick.AddListener(OnClaimByPremiumButton);
        claimNotAdsBtn.onClick.AddListener(ActionClaimBoss);
        claimRVButton.OnRewardGranted += OnRewardGrantedClaim;
        container.SetActive(false);
        originalScale = scaleGridLayoutGroup.cellScale;
        originalSpacing = scaleGridLayoutGroup.spacing;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnUnlockBoss, OnUnlockBoss);
        nextBtn.onClick.RemoveListener(OnNextButton);
        skipBtn.onClick.RemoveListener(OnSkipButton);
        claimByPremium.onClick.RemoveListener(OnClaimByPremiumButton);
        claimNotAdsBtn.onClick.RemoveListener(ActionClaimBoss);
        claimRVButton.OnRewardGranted -= OnRewardGrantedClaim;
    }

    void OnUnlockBoss(params object[] obj)
    {
        #region Firebase
        PlayerPrefs.SetInt("Firebase_BossFightMenu", 0);
        #endregion

        bool isUnlockInCharacterUI = false;
        if (obj.Length > 0)
            isUnlockInCharacterUI = true;



        if (bossSO.chassisSO.IsIgnoreClaimRV && !isUnlockInCharacterUI)
        {
            bossSO.chassisSO.TryUnlockItem();
            OnNextButton();
            return;
        }

        cardGridCanvasGroup.Hide();
        nextButtonCanvasGroup.Hide();
        unlockedtxt.DOFade(0, 0);
        backGroundUnlocked.SetActive(false);

        //v1.5.1: Disable claim Boss
        //claimBtn.gameObject.SetActive(true);

        isSelectCharacterUI = isUnlockInCharacterUI;

        claimRVButton.Location = isUnlockInCharacterUI ? AdsLocation.RV_Claim_Boss_Inventory_UI : AdsLocation.RV_Claim_Boss_Win_Boss_UI;

        // claimRVButton.interactable = AdsManager.Instance.IsReadyRewarded;
        //iconLoadingAds.SetActive(!claimRVButton.interactable);

        container.SetActive(true);

        SpawnBossModelRenderer(isUnlockInCharacterUI ? infoBossLockSelect : infoOfBoss, isUnlockInCharacterUI ? bossSelectLock : opponentModelPrefab);
        DropBox(() =>
        {
            nonInteractiveVisibility.Show();
            interactiveVisibility.Show();
            SoundManager.Instance.PlayLoopSFX(GeneralSFX.UIDropBox, 0.5f, false, false, gameObject);
        });
        GetAndShowCards();
        if (lockedChainController != null)
        {
            DestroyImmediate(lockedChainController.gameObject);
        }
        lockedChainController = Instantiate(lockedChainControllerPrefab, container.transform);

        //v1.5.1: Disable claim Boss
        claimNotAdsBtn.gameObject.SetActive(!IsAbleToClaimBossByRV);
        claimByRVBtn.gameObject.SetActive(IsAbleToClaimBossByRV);
        claimByPremium.gameObject.SetActive(IsAbleToClaimBossByRV);
        skipBtn.gameObject.SetActive(IsAbleToClaimBossByRV);

        claimByRVAmountTxt.SetText($"{bossClaimByRVSO.data[bossSO.ClaimByRVAmountKey]}/{bossSO.claimByRVAmount}");
        claimByPremiumAmountTxt.SetText($"{premiumIcon}{CalculateRVToGemExchange(bossSO.claimByRVAmount - bossClaimByRVSO.data[bossSO.ClaimByRVAmountKey])}");

        #region Design Events
        int bossID = GetBossId(bossSO);
        string rvName = $"ClaimBoss_Boss{bossID}";
        string location = isSelectCharacterUI ? "BuildUI" : "WinBossUI";
        GameEventHandler.Invoke(DesignEvent.RVShow);
        #endregion
    }

    void OnSkipButton()
    {
        OnNextButton();
        OnSkipClicked?.Invoke();
    }

    void OnNextButton()
    {
        BossFightManager.Instance.UpdateInfoOfBoss();
        PBPvPStateGameController pBPvPStateGameController = ObjectFindCache<PBPvPStateGameController>.Get();
        if (pBPvPStateGameController != null)
            pBPvPStateGameController.LeaveGame();

        cardGridCanvasGroup.Hide();
        container.SetActive(false);

        GameEventHandler.Invoke(BossFightEventCode.OnDisableUnlockBoss);
    }
    void OnClaimByPremiumButton()
    {
        float gemAmount = CalculateRVToGemExchange(bossSO.claimByRVAmount - bossClaimByRVSO.data[bossSO.ClaimByRVAmountKey]);
        int bossID = GetBossId(bossSO);
        string itemID = $"ClaimBoss_{bossID}";
        if (!CurrencyManager.Instance.IsAffordable(CurrencyType.Premium, gemAmount))
        {
            // Show popup gem IAP
            IAPGemPackPopup.Instance?.Show();
        }
        else if (CurrencyManager.Instance.Spend(CurrencyType.Premium, gemAmount, ResourceLocation.BossFight, itemID))
        {
            #region Progression Event
            TrackClaimBoss(bossSO, ClaimType.Gem);
            #endregion
            ActionClaimBoss();
        }
    }
    void OnRewardGrantedClaim(RVButtonBehavior.RewardGrantedEventData data)
    {
        #region Progression Event
        TrackClaimBoss(bossSO, ClaimType.RV);
        #endregion

        int watchCount = bossClaimByRVSO.data[bossSO.ClaimByRVAmountKey] + 1;
        bossClaimByRVSO.data[bossSO.ClaimByRVAmountKey] = watchCount;
        claimByRVAmountTxt.SetText($"{watchCount}/{bossSO.claimByRVAmount}");
        claimByPremiumAmountTxt.SetText($"{premiumIcon}{CalculateRVToGemExchange(bossSO.claimByRVAmount - bossClaimByRVSO.data[bossSO.ClaimByRVAmountKey])}");
        if (watchCount >= bossSO.claimByRVAmount)
            ActionClaimBoss();

        #region MonetizationEventCode
        AdsLocation adsLocationEnum = isSelectCharacterUI ? AdsLocation.RV_Claim_Boss_Inventory_UI : AdsLocation.RV_Claim_Boss_Win_Boss_UI;
        MonetizationEventCode claimBossMonetization = isSelectCharacterUI ? MonetizationEventCode.ClaimBoss_InventoryUI : MonetizationEventCode.ClaimBoss_WinBossUI;
        string adsLocation = adsLocationEnum == AdsLocation.RV_Claim_Boss_Win_Boss_UI ? "WinBossUI" : "BuildUI";
        int bossID = GetBossId(bossSO);
        GameEventHandler.Invoke(claimBossMonetization, adsLocation, bossID, watchCount);
        #endregion
    }

    void DropBox(Action complete)
    {
        specialBotModel.packGameObject.SetActive(true);
        specialBotModel.packTransform.position = specialBotModel.startDropPoint.transform.position;
        camera.transform.rotation = specialBotModel.startCamDropRot.transform.rotation;
        sequence.Kill();
        sequence = DOTween.Sequence();
        sequence
            .Join(camera.transform.DORotateQuaternion(specialBotModel.packCenterCamRot.transform.rotation, dropDuration).SetEase(camFollowingCurve))
            .Join(specialBotModel.packTransform.DOMove(specialBotModel.endDropPoint.transform.position, dropDuration, false).SetEase(bagDroppingCurve))
            .Play().OnComplete(() =>
            {
                specialBotModel.packDropOnGroundFX.PlayAnim();
                complete?.Invoke();
            });
    }

    public void SpawnBossModelRenderer(PlayerInfoVariable opponentInfoVariable, PBModelRenderer pBModelRenderer)
    {
        var instance = SpawnModelRenderer(pBModelRenderer);
        // instance.gameObject.SetLayer(openPackLayer, true);
        instance.transform.SetParent(modelContainer.transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        if (opponentInfoVariable != null) instance.SetInfo(opponentInfoVariable);
        instance.BuildRobot(false); //True or false is not important just a placeholder
        instance.ChassisInstance.RobotBaseBody.isKinematic = true;

        modelPreview = instance.gameObject;
    }

    PBModelRenderer SpawnModelRenderer(PBModelRenderer prefab)
    {
        var instance = Instantiate(prefab, transform);
        if (objectPreview != null)
            Destroy(objectPreview);
        objectPreview = instance.gameObject;
        return instance;
    }

    public virtual void GetAndShowCards()
    {
        bossSO.chassisSO.TryUnlockItem();
    }

    private void TryGetPart()
    {
        var fixedColumn = 3;
        var gachaCards = GetCards();
        //if (gachaCards == null)
        //{
        //    cardGridCanvasGroup.Hide();
        //    return;
        //}

        foreach (var card in gachaCards)
        {
            if (card is GachaCard_Currency gachaCard_Currency)
            {
                gachaCard_Currency.ResourceLocationProvider = resourceLocationProvider;
            }
            card.GrantReward();
        }
        var duplicateGachaCardsGroups = gachaCards.GroupDuplicate();

        foreach (var card in cardControllers)
        {
            card.gameObject.SetActive(false);
        }
        for (int i = 0; i < duplicateGachaCardsGroups.Count; i++)
        {
            var card = cardControllers[i];
            card.gameObject.SetActive(true);
            card.Setup(duplicateGachaCardsGroups[i], true);
        }

        scaleGridLayoutGroup.fixedColumn = fixedColumn;
        scaleGridLayoutGroup.cellScale = originalScale * cellScaleAnimationCurve.Evaluate(scaleGridLayoutGroup.fixedColumn);
        scaleGridLayoutGroup.spacing = originalSpacing * cellScaleAnimationCurve.Evaluate(scaleGridLayoutGroup.fixedColumn);
        scaleGridLayoutGroup.UpdateView();
    }

    public List<GachaCard> GetCards()
    {
        if (bossSO == null)
        {
            return null;
        }

        //v1.5.1: Enable Reward Part Boss
        //BossSO bossSOReward = isSelectCharacterUI ? BossFightManager.Instance.bossSOSelected : bossSO;
        return (GachaCardGenerator.Instance as PBGachaCardGenerator).Generate(bossSO.gameOverRewardGroupInfo);
    }

    private void ActionClaimBoss()
    {
        #region Firebase
        PlayerPrefs.SetInt("Firebase_BossFightMenu", 1);
        #endregion

        PBChassisSO pbChassisSO = isSelectCharacterUI ? BossFightManager.Instance.select_bossChassisLock.value.Cast<PBChassisSO>() : bossSO.chassisSO;
        pbChassisSO.IsClaimedRV = true;
        lockedChainController.Play();

        claimByRVBtn.gameObject.SetActive(false);
        claimByPremium.gameObject.SetActive(false);
        claimNotAdsBtn.gameObject.SetActive(false);
        skipBtn.gameObject.SetActive(false);

        #region Progression Event
        int bossID = GetBossId(bossSO);
        string status = "Start";
        string type = GetClaimType(bossSO);
        GameEventHandler.Invoke(ProgressionEvent.ClaimBoss, status, bossID, type);
        #endregion

        SoundManager.Instance.PlayLoopSFX(SFX.ClaimBoss, 0.5f, false, false, gameObject);
        if (darkenImage != null)
        {
            var unlockedSequence = DOTween.Sequence()
                .SetDelay(1f)
                .Append
                (
                    darkenImage
                    .DOFade(1, 1)
                    .OnComplete(() =>
                    {
                        unlockedtxt.DOScale(3, 0);
                        unlockedtxt.DOFade(0, 0);
                        darkenImage.DOFade(0, 0);

                        //Handle Model
                        modelPreview.transform
                        .DOScale(1.5f, 0.1f).SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            modelPreview.transform
                            .DOScale(1, 0.5F)
                            .SetEase(Ease.InBack)
                            .OnComplete(() =>
                            {
                                TryGetPart();
                                unlockBossVFX.PlayFX();
                                cardGridCanvasGroup.Show();

                                //Glow Text
                                unlockedtxt.DOScale(1, 0.7f).SetEase(Ease.InBack);
                                unlockedtxt
                                .DOFade(1, 0.7f)
                                .SetEase(Ease.InBack)
                                .OnComplete(() =>
                                {
                                    backGroundUnlocked.SetActive(true);
                                    unlockedGlowingObject = Instantiate(unlockedtxt.gameObject, unlockedtxt.transform);
                                    unlockedGlowingObject.transform.localPosition = Vector3.zero;
                                    TMP_Text unlockedGlowingTxt = unlockedGlowingObject.GetComponent<TMP_Text>();
                                    if (unlockedGlowingTxt != null)
                                    {
                                        unlockedGlowingTxt.DOFade(0.4f, 0);
                                        unlockedGlowingTxt.DOScale(3f, 1f);
                                        unlockedGlowingTxt.DOFade(0, 0.5f).OnComplete(() =>
                                        {
                                            nextButtonCanvasGroup.Show();
                                            Destroy(unlockedGlowingObject);
                                        });
                                    }
                                });
                            });
                        });

                        //Play VFX
                        hightLightVFX.Play();
                    })
                ).Play();
        }

        GameEventHandler.Invoke(BossFightEventCode.OnClaimBossComplete);
    }

    private float CalculateRVToGemExchange(int rv)
    {
        return Mathf.Abs(rv) * ExchangeRateTableSO.GetExchangeRateOfOtherItems(ExchangeRateTableSO.ItemType.RV, ExchangeRateTableSO.ArenaFlags.All);
    }

    #region Progression Event Claim Boss

    private void TrackClaimBoss(BossSO bossSO, ClaimType claimType)
    {
        string key = GetTrackingKey(bossSO, claimType);
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetInt(key, 1);
        }
    }

    private bool HasClaimedBoss(BossSO bossSO, ClaimType claimType)
    {
        string key = GetTrackingKey(bossSO, claimType);
        return PlayerPrefs.HasKey(key);
    }

    private string GetClaimType(BossSO bossSO)
    {
        bool hasRVClaim = HasClaimedBoss(bossSO, ClaimType.RV);
        bool hasGemClaim = HasClaimedBoss(bossSO, ClaimType.Gem);

        if (hasRVClaim && !hasGemClaim)
        {
            return "RV";
        }
        else if (!hasRVClaim && hasGemClaim)
        {
            return "Gem";
        }
        else
        {
            return "Mixed";
        }
    }

    private string GetTrackingKey(BossSO bossSO, ClaimType claimType)
    {
        if (bossSO == null)
        {
            Debug.LogError("BossSO is null");
            return string.Empty;
        }

        return $"{bossSO.botInfo.name}-TrackingClaim{claimType}Boss";
    }

    private int GetBossId(BossSO bossSO)
    {
        BossChapterSO bossChapterSO = BossFightManager.Instance.bossMapSO.GetBossChapterDefault;
        int bossID = bossChapterSO.bossList.FindIndex(v => v == bossSO) + 1;
        return bossID;
    }

    public enum ClaimType
    {
        RV,
        Gem
    }

    #endregion
}
