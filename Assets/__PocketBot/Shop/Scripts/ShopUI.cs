using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Serializable]
    public class CurrencySection
    {
        [field: SerializeField]
        public CurrencyType currencyType { get; set; }
        [field: SerializeField]
        public EventCode onJumpToThisSectionEventCode { get; set; }
        [field: SerializeField]
        public RectTransform currencySectionRectTransform { get; set; }
        [field: SerializeField]
        public UnityEvent<CurrencySection> onJumpToThisSectionEvent { get; set; } = new UnityEvent<CurrencySection>();

        private void OnJumpToCurrencySection()
        {
            onJumpToThisSectionEvent.Invoke(this);
        }

        public void SubscribeEvents()
        {
            GameEventHandler.AddActionEvent(onJumpToThisSectionEventCode, OnJumpToCurrencySection);
        }

        public void UnsubscribeEvents()
        {
            GameEventHandler.RemoveActionEvent(onJumpToThisSectionEventCode, OnJumpToCurrencySection);
        }
    }

    [SerializeField]
    private ScrollRect m_ScrollRect;
    [SerializeField]
    private PPrefBoolVariable m_FirstTimeShowShopTabVar;
    [SerializeField]
    private List<CurrencySection> m_CurrencySections;

    [SerializeField, BoxGroup("Data - FTUE")] private PPrefBoolVariable m_ClickOpenBoxSlotTheFirstTimeFTUE;
    [SerializeField, BoxGroup("Data - FTUE")] private PPrefBoolVariable m_FTUE_StartUnlockBoxSlotTheFirstTime;

    private void Awake()
    {
        for (int i = 0; i < m_CurrencySections.Count; i++)
        {
            m_CurrencySections[i].SubscribeEvents();
            m_CurrencySections[i].onJumpToThisSectionEvent.AddListener(JumpToCurrencyPackSection);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < m_CurrencySections.Count; i++)
        {
            m_CurrencySections[i].UnsubscribeEvents();
            m_CurrencySections[i].onJumpToThisSectionEvent.RemoveListener(JumpToCurrencyPackSection);
        }
    }

    [Button]
    public void JumpToCurrencyPackSection(CurrencyType currencyType)
    {
        JumpToCurrencyPackSection(m_CurrencySections.Find(section => section.currencyType == currencyType));
    }

    public void JumpToCurrencyPackSection(CurrencySection currencySection)
    {
        if (m_ClickOpenBoxSlotTheFirstTimeFTUE.value && !m_FTUE_StartUnlockBoxSlotTheFirstTime.value)
            return;

        if (currencySection == null)
            return;
        var dockerController = FindAnyObjectByType<DockerController>();
        if (dockerController != null)
        {
            var shopButton = dockerController.GetButtonOfType(ButtonType.Shop);
            if (shopButton != null && shopButton.enabled)
            {
                dockerController.SelectManuallyButtonOfType(ButtonType.Shop);
                m_ScrollRect.FocusOnItem(currencySection.currencySectionRectTransform);
            }
        }
    }
}