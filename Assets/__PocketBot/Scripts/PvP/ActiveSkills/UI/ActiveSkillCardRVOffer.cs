using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GachaSystem.Core;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using HyrphusQ.Helpers;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ActiveSkillCardRVOffer : MonoBehaviour
{
    [SerializeField]
    private int m_CardQuantity = 1;
    [SerializeField]
    private Image m_IconImage;
    [SerializeField]
    private TextAdapter m_CardQuantityText;
    [SerializeField]
    private TextAdapter m_RemainingTimeToRefillText;
    [SerializeField]
    private GameObject m_RefillPanelGO;
    [SerializeField]
    private GameObject m_EnabledRVButtonGO;
    [SerializeField]
    private GameObject m_DisabledRVButtonGO;
    [SerializeField]
    private RVButtonBehavior m_RVButton;
    [SerializeField]
    private Button m_Button;
    [SerializeField]
    private Image m_RVIconImage;
    [SerializeField]
    private RVTicketStateHandle m_RVTicketStateHandle;

    private ActiveSkillSO m_ActiveSkillSO;
    private Coroutine m_UpdateViewCoroutine;

    public int cardQuantity
    {
        get
        {
            return m_CardQuantity;
        }
        set
        {
            m_CardQuantity = value;
        }
    }

    private void Awake()
    {
        m_RVTicketStateHandle.SetFieldValue("originalAnchoredPos", m_RVIconImage.rectTransform.anchoredPosition);
        m_RVTicketStateHandle.InvokeMethod("UpdateView");
        m_RVButton.OnRewardGranted += OnRewardGranted;
    }

    private void OnEnable()
    {
        m_UpdateViewCoroutine = StartCoroutine(CommonCoroutine.Interval(1f, false, () => false, i => UpdateView()));
    }

    private void OnDisable()
    {
        if (m_UpdateViewCoroutine != null)
            StopCoroutine(m_UpdateViewCoroutine);
        m_UpdateViewCoroutine = null;
    }

    private DateTime GetUpcomingRewardTime(ActiveSkillSO activeSkillSO)
    {
        string dateTimeString = PlayerPrefs.GetString($"{activeSkillSO.guid}_RVPack_UpcomingRewardTime", DateTime.MinValue.ToString(CultureInfo.InvariantCulture));
        return DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture);
    }

    private void SetUpcomingRewardTime(ActiveSkillSO activeSkillSO, DateTime rvPackResetTime)
    {
        string dateTimeString = rvPackResetTime.ToString(CultureInfo.InvariantCulture);
        PlayerPrefs.SetString($"{activeSkillSO.guid}_RVPack_UpcomingRewardTime", dateTimeString);
    }

    private void OnRewardGranted(RVButtonBehavior.RewardGrantedEventData evemtData)
    {
        if (m_ActiveSkillSO == null)
            return;
        List<GachaCard> gachaCards = (PBGachaCardGenerator.Instance as PBGachaCardGenerator).Generate(new RewardGroupInfo()
        {
            generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>()
            {
                { m_ActiveSkillSO, new ShopProductSO.DiscountableValue() { value = m_CardQuantity } }
            }
        });
        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, null, null);
        SetUpcomingRewardTime(m_ActiveSkillSO, DateTime.Now.Date.AddDays(1));

        #region Source Event
        try
        {
            float skillCardCount = m_CardQuantity;
            ResourceLocationProvider resourceProvider = new ResourceLocationProvider(ResourceLocation.RV, $"FreeSkill");
            GameEventHandler.Invoke(LogSinkSource.SkillCard, skillCardCount, resourceProvider);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion

        #region MonetizationEventCode
        try
        {
            GameEventHandler.Invoke(MonetizationEventCode.FreesSkill);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

#if UNITY_EDITOR
    [Button]
    private void DeleteData(ActiveSkillSO activeSkillSO)
    {
        PlayerPrefs.DeleteKey($"{activeSkillSO.guid}_RVPack_UpcomingRewardTime");
    }

    [Button]
    private void DeleteAllData()
    {
        EditorUtils.FindAssetsOfType<ActiveSkillSO>().ForEach(activeSkillSO => DeleteData(activeSkillSO));
    }
#endif

    public void Initialize(ActiveSkillSO activeSkillSO)
    {
        m_ActiveSkillSO = activeSkillSO;
        m_IconImage.sprite = activeSkillSO.GetThumbnailImage();
        m_CardQuantityText.SetText(m_CardQuantityText.blueprintText.Replace(Const.StringValue.PlaceholderValue, m_CardQuantity.ToString()));
        UpdateView();
    }

    public void UpdateView()
    {
        if (m_ActiveSkillSO == null)
            return;
        DateTime upcomingRewardTime = GetUpcomingRewardTime(m_ActiveSkillSO);
        if (DateTime.Now > upcomingRewardTime)
        {
            if (!m_Button.interactable)
                m_Button.interactable = true;

            if (m_RefillPanelGO.activeSelf)
                m_RefillPanelGO.SetActive(false);

            if (m_DisabledRVButtonGO.activeSelf)
                m_DisabledRVButtonGO.SetActive(false);

            if (!m_EnabledRVButtonGO.activeSelf)
                m_EnabledRVButtonGO.SetActive(true);

            if (!m_CardQuantityText.gameObject.activeSelf)
                m_CardQuantityText.gameObject.SetActive(true);
        }
        else
        {
            if (m_Button.interactable)
                m_Button.interactable = false;

            if (!m_RefillPanelGO.activeSelf)
                m_RefillPanelGO.SetActive(true);

            if (!m_DisabledRVButtonGO.activeSelf)
                m_DisabledRVButtonGO.SetActive(true);

            if (m_EnabledRVButtonGO.activeSelf)
                m_EnabledRVButtonGO.SetActive(false);

            if (m_CardQuantityText.gameObject.activeSelf)
                m_CardQuantityText.gameObject.SetActive(false);

            m_RemainingTimeToRefillText.SetText(DateTime.Now.ToReadableTimeSpan(upcomingRewardTime, 2));
        }
    }
}