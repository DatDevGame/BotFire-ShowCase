using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class ToastUI : Singleton<ToastUI>
{
    [SerializeField]
    private Vector2 m_DefaultPosition = new Vector2(0, 500f);
    [SerializeField]
    private NoticeFadeText m_NoticeTextPrefab;
    [SerializeField]
    private RectTransform m_Container;

    private void Start()
    {
        GameEventHandler.AddActionEvent(AdvertisingEventCode.OnShowAdNotAvailableNotice, OnShowAdNotAvailableNotice);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(AdvertisingEventCode.OnShowAdNotAvailableNotice, OnShowAdNotAvailableNotice);
    }

    private void OnShowAdNotAvailableNotice()
    {
        Show(I2LHelper.TranslateTerm(I2LTerm.RVButtonBehavior_Desc_Oops));
    }

    public static void Show(string message, Vector2 anchoredPos, Transform parent = null)
    {
        NoticeFadeText noticeFadeText = Instantiate(Instance.m_NoticeTextPrefab, parent != null ? parent : Instance.m_Container);
        noticeFadeText.SetText(message);
        noticeFadeText.RectTransform.anchoredPosition = anchoredPos;
    }

    public static void Show(string message, Transform parent = null)
    {
        Show(message, Instance.m_DefaultPosition, parent);
    }
}