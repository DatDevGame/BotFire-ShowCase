using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using I2.Loc;
using LatteGames.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinCell : ItemCell, IResourceLocationProvider
{
    [SerializeField]
    protected GameObject m_LoadingProgressBar;
    [SerializeField]
    protected Image m_LoadingProgressImage;
    [SerializeField]
    protected Button m_CurrencyUnlockButton;
    [SerializeField]
    protected RVButtonBehavior m_RVUnlockButton;
    [SerializeField]
    protected Image m_PartBGImage;
    [SerializeField]
    protected Image m_ThumbnailImageWithLabel;
    [SerializeField]
    protected RectTransform m_ButtonGroupRectTransform;
    [SerializeField]
    protected GameObject m_RequiredRVLabel, m_RequiredGemLabel;
    [SerializeField]
    protected TextMeshProUGUI m_RequiredGemText, m_RequiredGemShadowText;
    [SerializeField]
    protected LocalizationParamsManager m_RequiredRVParamsManager;
    [SerializeField]
    protected List<Image> m_GradientImages;
    [SerializeField]
    protected SerializedDictionary<RarityType, Material> m_RarityMaterialDictionary;

    protected SkinListView m_SkinListView;
    protected Coroutine m_LoadingAssetCoroutine;

    protected void Awake()
    {
        m_RVUnlockButton.OnRewardGranted += OnRewardGranted;
        m_CurrencyUnlockButton.onClick.AddListener(OnUnlockItemClicked);
        foreach (var gradientImage in m_GradientImages)
        {
            gradientImage.material.SetVector("_ClipSoftness", gradientImage.canvasRenderer.clippingSoftness);
        }
        CurrencyManager.Instance[CurrencyType.Premium].onValueChanged += OnValueChanged;
    }

    protected void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance[CurrencyType.Premium].onValueChanged -= OnValueChanged;
    }

    protected void OnValueChanged(ValueDataChanged<float> eventData)
    {
        UpdateViewUnlockRequirementLabel();
    }

    protected void OnRewardGranted(RVButtonBehavior.RewardGrantedEventData eventData)
    {
        if (item == null || item.IsUnlocked())
            return;
        item.UpdateRVWatched();
        if (item.TryUnlockItem())
        {
            m_SkinListView.ForceSelectCell(this);
            SetActiveButonGroup(false);

            #region Progression Event
            try
            {
                string status = ProgressionEventStatus.Start;
                string partname_SkinID = $"{m_SkinListView.currentPartSO.GetModule<NameItemModule>().displayName}-{m_SkinListView.FindIndexOf(item)}";
                string currentcyType = "RV";
                GameEventHandler.Invoke(ProgressionEvent.BuySkin, status, partname_SkinID, currentcyType);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
        }
        UpdateView();

        #region RV Event
        try
        {
            Requirement_RewardedAd requirement_RewardedAd = null;
            if (item.TryGetModule(out UnlockableItemModule unlockableModule))
            {
                if (unlockableModule.TryGetUnlockRequirement(out Requirement_RewardedAd requirementRewardedAd))
                    requirement_RewardedAd = requirementRewardedAd;
            }

            PBPartSO pBPartSO = m_SkinListView.currentPartSO;
            string partType = $"{pBPartSO.PartType}";
            string partName = $"{pBPartSO.GetModule<NameItemModule>().displayName}";
            string skinIndex = $"{m_SkinListView.FindIndexOf(item)}";
            int adsCount = requirement_RewardedAd?.progressRewardedAd ?? 0;
            GameEventHandler.Invoke(MonetizationEventCode.GetSkin, partType, partName, skinIndex, adsCount);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    protected void OnUnlockItemClicked()
    {
        if (item == null || item.IsUnlocked())
            return;
        if (item.TryUnlockItem() && item.TryGetUnlockRequirement(out Requirement_Currency currencyRequirement))
        {
            currencyRequirement.resourceLocationProvider = this;
            currencyRequirement.ExecuteRequirement();
            m_SkinListView.ForceSelectCell(this);
            SetActiveButonGroup(false);
            UpdateView();
        }
    }

    protected void UpdateViewThumbnailImage()
    {
        var isUnlocked = item.IsUnlocked();
        var thumbnailSprite = m_ItemSO.GetThumbnailImage();
        m_ThumbnailImage.sprite = thumbnailSprite;
        m_ThumbnailImageWithLabel.sprite = thumbnailSprite;
        m_ThumbnailImage.gameObject.SetActive(isUnlocked);
        m_ThumbnailImageWithLabel.gameObject.SetActive(!isUnlocked);
    }

    protected void UpdateViewUnlockRequirementLabel()
    {
        var isUnlocked = item.IsUnlocked();
        if (isUnlocked)
        {
            m_RequiredRVLabel.SetActive(false);
            m_RequiredGemLabel.SetActive(false);
        }
        else
        {
            if (item.IsRVItem())
            {
                m_RequiredRVParamsManager.SetParameterValue("WatchedRV", item.GetRVWatchedCount().ToString());
                m_RequiredRVParamsManager.SetParameterValue("RequiredRV", item.GetRequiredRVCount().ToString());
                m_RequiredRVLabel.SetActive(true);
                m_RequiredGemLabel.SetActive(false);
            }
            else
            {
                var price = item.GetPrice();
                m_RequiredGemShadowText.SetText(price.ToRoundedText());
                m_RequiredGemText.SetText(price.ToRoundedText());
                m_RequiredGemLabel.SetActive(true);
                m_RequiredRVLabel.SetActive(false);
            }
        }
    }

    protected void UpdateViewButtonGroup()
    {
        if (item.IsUnlocked())
        {
            m_RVUnlockButton.gameObject.SetActive(false);
            m_CurrencyUnlockButton.gameObject.SetActive(false);
        }
        else
        {
            if (item.IsRVItem())
            {
                m_RVUnlockButton.gameObject.SetActive(true);
                m_CurrencyUnlockButton.gameObject.SetActive(false);
            }
            else
            {
                var isEnoughGem = CurrencyManager.Instance[item.GetCurrencyType()].value >= item.GetPrice();
                m_RVUnlockButton.gameObject.SetActive(false);
                m_CurrencyUnlockButton.gameObject.SetActive(true);
                m_CurrencyUnlockButton.interactable = isEnoughGem;
                m_CurrencyUnlockButton.GetComponent<GrayscaleUI>()?.SetGrayscale(!isEnoughGem);
            }
        }
    }

    protected void SetActiveButonGroup(bool isActive, float duration = AnimationDuration.TINY)
    {
        if (isActive)
        {
            m_ButtonGroupRectTransform.DOKill();
            m_ButtonGroupRectTransform.DOAnchorPos(new Vector2(m_ButtonGroupRectTransform.anchoredPosition.x, 20f), duration);
        }
        else
        {
            m_ButtonGroupRectTransform.DOKill();
            m_ButtonGroupRectTransform.DOAnchorPos(new Vector2(m_ButtonGroupRectTransform.anchoredPosition.x, m_ButtonGroupRectTransform.sizeDelta.y), duration).SetEase(Ease.InBack);
        }
    }

    protected IEnumerator DisplayLoadingProgress_CR(IAsyncTask asyncTask)
    {
        if (asyncTask.isCompleted)
            yield break;
        m_LoadingProgressBar.SetActive(true);
        while (true)
        {
            // m_LoadingProgressImage.fillAmount = asyncTask.percentageComplete;
            if (asyncTask.isCompleted)
            {
                yield return null;
                break;
            }
            yield return null;
        }
        m_LoadingProgressBar.SetActive(false);
        m_LoadingAssetCoroutine = null;
    }

    public override void Initialize(ItemSO item, ItemManagerSO itemManagerSO)
    {
        if (item == null)
            return;
        base.Initialize(item, itemManagerSO);
        m_PartBGImage.material = m_RarityMaterialDictionary.Get(item.GetRarityType());
        m_SkinListView = GetComponentInParent<SkinListView>();
        m_LoadingProgressBar.SetActive(false);
        SetActiveButonGroup(false, 0f);
    }

    public override void Select(bool isForceSelect = false)
    {
        base.Select(isForceSelect);
        if (!isForceSelect && !item.IsUnlocked())
            SetActiveButonGroup(true);
        if (m_LoadingAssetCoroutine == null)
            m_LoadingAssetCoroutine = StartCoroutine(DisplayLoadingProgress_CR(item.Cast<SkinSO>().GetOrLoadSkinResources()));
    }

    public override void Deselect()
    {
        base.Deselect();
        if (!item.IsUnlocked())
            SetActiveButonGroup(false);
    }

    public override void UpdateView()
    {
        if (item == null)
            return;
        m_ThumbnailLock?.gameObject.SetActive(!item.IsOwned());
        UpdateViewThumbnailImage();
        UpdateViewUnlockRequirementLabel();
        UpdateViewButtonGroup();
    }

    public string GetItemId()
    {
        if(m_SkinListView.currentPartSO == null)
            return "Skin";

        PBPartSO pbPartSO = m_SkinListView.currentPartSO;
        string partName = $"{pbPartSO.GetModule<NameItemModule>().displayName}";
        string skinID = $"{m_SkinListView.FindIndexOf(item)}";
        return $"{partName}_{skinID}";
    }

    public ResourceLocation GetLocation()
    {
        return ResourceLocation.UnlockSkin;
    }
}