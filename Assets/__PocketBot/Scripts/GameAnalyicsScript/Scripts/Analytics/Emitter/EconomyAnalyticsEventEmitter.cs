using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using PBAnalyticsEvents;

public class EconomyAnalyticsEventEmitter : MonoBehaviour
{
    private const string k_Standard = "Money";
    private const string k_Premium = "Gem";

    private void Awake()
    {
        GameEventHandler.AddActionEvent(EconomyEventCode.AcquireResource, OnAcquireResource);
        GameEventHandler.AddActionEvent(EconomyEventCode.ConsumeResource, OnComsumeResource);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(EconomyEventCode.AcquireResource, OnAcquireResource);
        GameEventHandler.RemoveActionEvent(EconomyEventCode.ConsumeResource, OnComsumeResource);
    }

    private string CurrencyTypeToResourceString(CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.Standard:
                return k_Standard;
            case CurrencyType.Premium:
                return k_Premium;
            default:
                return currencyType.ToString();
        }
    }

    private void OnAcquireResource(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var currencyType = (CurrencyType)parameters[0];
        var amount = (float)parameters[1];
        var resourceLocation = (ResourceLocation)parameters[2];
        var itemId = (string)parameters[3];
        PBAnalyticsManager.Instance.AcquireResource(resourceLocation.ToString(), itemId, CurrencyTypeToResourceString(currencyType), amount);
    }

    private void OnComsumeResource(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var currencyType = (CurrencyType)parameters[0];
        var amount = (float)parameters[1];
        var resourceLocation = (ResourceLocation)parameters[2];
        var itemId = (string)parameters[3];
        PBAnalyticsManager.Instance.ConsumeResource(resourceLocation.ToString(), itemId, CurrencyTypeToResourceString(currencyType), amount);
    }
}