using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GachaSystem.Core;
using HyrphusQ.Helpers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public static class PBExtensionMethods
{
    public static T GetRandom<T>(this List<T> list, Predicate<T> predicate = null)
    {
        if (list.Count <= 0) return default;
        if (predicate != null)
        {
            list = list.FindAll(predicate);
        }
        if (list.Count == 0) return default;
        return list[Random.Range(0, list.Count)];
    }

    public static bool IsNullOrEmpty<T>(this List<T> list)
    {
        if (list == null) return true;
        if (list.Count == 0) return true;
        return true;
    }

    public static string ToLabelText(this CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.Standard:
                return "Coins";
            case CurrencyType.Premium:
                return "Gems";
            case CurrencyType.Medal:
                return "Medals";
            case CurrencyType.RVTicket:
                return "Skip Ads";
            default:
                return string.Empty;
        }
    }

    public static Color ToColor(this float3 value)
    {
        return new Color(value.x, value.y, value.z);
    }

    public static Color ToColor(this float4 value)
    {
        return new Color(value.x, value.y, value.z, value.w);
    }

    public static float4 ToFloat4(this Color color)
    {
        return new float4(color.r, color.g, color.b, color.a);
    }

    public static float3 ToFloat3(this Color color)
    {
        return new float3(color.r, color.g, color.b);
    }

    public static string TrimNonASCII(this string value)
    {
        string pattern = "[^ -~]+";
        Regex reg_exp = new Regex(pattern);
        return reg_exp.Replace(value, "");
    }
    public static int GetIntNumber(this string value)
    {
        // Define a regular expression pattern to match digits
        Regex regex = new Regex(@"\d+");

        // Match the first occurrence of digits in the input string
        Match match = regex.Match(value);

        // Check if the match was successful and parse the matched value to an integer
        if (match.Success && int.TryParse(match.Value, out int result))
        {
            return result;
        }
        return -1;
    }

    public static void SetGameLayerRecursive(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
        {
            SetGameLayerRecursive(child.gameObject, layer);
        }
    }

    public static void EnabledAllParts(this PBRobot robot, bool isEnable, bool includeInactive = false)
    {
        robot.IsDisarmed = !isEnable;
        isEnable = !robot.IsDisarmed;
        var parts = robot.transform.GetComponentsInChildren<PartBehaviour>(includeInactive);
        var colliders = robot.transform.GetComponentsInChildren<PBCollider>(includeInactive);
        if (parts == null || parts.Length <= 0)
            parts = robot.ChassisInstanceTransform.GetComponentsInChildren<PartBehaviour>(includeInactive);
        if (colliders == null || colliders.Length <= 0)
            colliders = robot.ChassisInstanceTransform.GetComponentsInChildren<PBCollider>(includeInactive);
        foreach (var part in parts)
        {
            part.enabled = isEnable;
        }
        foreach (var collider in colliders)
        {
            collider.enabled = isEnable;
        }
    }

    public static float RandomRange(this RangeValue<float> ranegFloatValue, Predicate<float> predicate = null) =>
        RandomHelper.RandomRange(ranegFloatValue.minValue, ranegFloatValue.maxValue, predicate);

    public static int RandomRange(this RangeValue<int> rangeIntValue, Predicate<int> predicate = null) =>
        RandomHelper.RandomRange(rangeIntValue.minValue, rangeIntValue.maxValue, predicate);

    public static Sprite GetOriginalPackThumbnail(this GachaPack gachaPack)
    {
        return gachaPack.GetOriginalPack().GetModule<MonoImageItemModule>().thumbnailImage;
    }

    public static string GetOriginalPackName(this GachaPack gachaPack)
    {
        return gachaPack.GetOriginalPack().GetModule<NameItemModule>().displayName;
    }

    public static int GetOriginalPackCardCount(this GachaPack gachaPack)
    {
        return gachaPack.GetOriginalPack().TotalCardsCount;
    }

    public static int GetOriginalPackGuaranteedCardsCount(this GachaPack gachaPack, RarityType rarity)
    {
        return gachaPack.GetOriginalPack().GetGuaranteedCardsCount(rarity);
    }

    public static Vector2 GetOriginalPackMoneyAmountRange(this GachaPack gachaPack)
    {
        return ((PBGachaPack)gachaPack.GetOriginalPack()).moneyAmountRange;
    }

    public static Vector2 GetOriginalPackGemAmountRange(this GachaPack gachaPack)
    {
        return ((PBGachaPack)gachaPack.GetOriginalPack()).gemAmountRange;
    }

    public static GachaPack GetOriginalPack(this GachaPack gachaPack)
    {
        if (gachaPack is PBManualGachaPack)
        {
            return ((PBManualGachaPack)gachaPack).SimulationFromGachaPack;
        }
        else
        {
            return gachaPack;
        }
    }

    public static T GetOrAddModule<T>(this ItemSO itemSO) where T : ItemModule, new()
    {
        if (!itemSO.TryGetModule(out T module))
        {
            module = ItemModule.CreateModule<T>(itemSO);
            itemSO.AddModule(module);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(itemSO);
#endif
        }
        return module;
    }

    public static DateTime GetDayOfNextWeek(this DateTime start, DayOfWeek dayOfWeek)
    {
        var daysToAdd = (dayOfWeek - start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd);
    }

    public static ExchangeRateTableSO.ItemType ToExchangeItemType(this PBPartType partType)
    {
        switch (partType)
        {
            case PBPartType.Body:
                return ExchangeRateTableSO.ItemType.Body;
            case PBPartType.Front:
                return ExchangeRateTableSO.ItemType.Front;
            case PBPartType.Upper:
                return ExchangeRateTableSO.ItemType.Upper;
            default:
                return ExchangeRateTableSO.ItemType.Body;
        }
    }

    public static string GetRemainingTime(this TimeBasedRewardSO timeBasedRewardSO)
    {
        TimeSpan interval = DateTime.Now - timeBasedRewardSO.LastRewardTime;
        var remainingSeconds = timeBasedRewardSO.CoolDownInterval - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);
        return string.Format("{0:00}:{1:00}:{2:00}", interval.Hours + (interval.Days * 24f), interval.Minutes, interval.Seconds);
    }

    public static string GetRemainingTimeInShort(this TimeBasedRewardSO timeBasedRewardSO)
    {
        TimeSpan interval = DateTime.Now - timeBasedRewardSO.LastRewardTime;
        var remainingSeconds = timeBasedRewardSO.CoolDownInterval - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);
        if (interval.Hours <= 0)
        {
            return string.Format("{0:00}M {1:00}S", interval.Minutes, interval.Seconds);
        }
        else
        {
            return string.Format("{0:00}H {1:00}M", interval.Hours + (interval.Days * 24f), interval.Minutes);
        }
    }

    public static string ToRemainingTime(this PPrefDatetimeVariable nextTimestampVar, int maxComponents = 4)
    {
        var componentCount = 0;
        var timeLeftBeforeReset = nextTimestampVar.value - DateTime.Now;
        var stringBuilder = new StringBuilder();
        if (timeLeftBeforeReset.Days > 0 && componentCount++ < maxComponents)
            stringBuilder.Append($"{timeLeftBeforeReset.Days}D ");
        if ((timeLeftBeforeReset.Hours > 0 || timeLeftBeforeReset.Days > 0) && componentCount++ < maxComponents)
            stringBuilder.Append($"{timeLeftBeforeReset.Hours}H ");
        if ((timeLeftBeforeReset.Minutes > 0 || timeLeftBeforeReset.Hours > 0 || timeLeftBeforeReset.Days > 0) && componentCount++ < maxComponents)
            stringBuilder.Append($"{timeLeftBeforeReset.Minutes}M ");
        if (componentCount++ < maxComponents)
            stringBuilder.Append($"{timeLeftBeforeReset.Seconds}S");
        return stringBuilder.ToString();
    }

    public static Vector2 CalcWorldPositionAtPivot(this RectTransform rectTransform, Vector2 pivot)
    {
        return (Vector2)rectTransform.position + Vector2.Scale(pivot - rectTransform.pivot, rectTransform.rect.size);
    }

    public static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, Vector2 focusPoint, bool isClamped = true)
    {
        Vector2 contentSize = scrollView.content.rect.size;
        Vector2 viewportSize = ((RectTransform)scrollView.content.parent).rect.size;
        Vector2 contentScale = scrollView.content.localScale;

        contentSize.Scale(contentScale);
        focusPoint.Scale(contentScale);

        Vector2 scrollPosition = scrollView.normalizedPosition;
        if (scrollView.horizontal && contentSize.x > viewportSize.x)
            scrollPosition.x = (focusPoint.x - viewportSize.x * 0.5f) / (contentSize.x - viewportSize.x);
        if (scrollView.vertical && contentSize.y > viewportSize.y)
            scrollPosition.y = (focusPoint.y - viewportSize.y * 0.5f) / (contentSize.y - viewportSize.y);
        if (isClamped)
        {
            scrollPosition.x = Mathf.Clamp01(scrollPosition.x);
            scrollPosition.y = Mathf.Clamp01(scrollPosition.y);
        }
        return scrollPosition;
    }

    public static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, RectTransform item, bool isClamped = true)
    {
        Vector2 itemCenterPoint = scrollView.content.InverseTransformPoint(item.transform.TransformPoint(item.rect.center));

        Vector2 contentSizeOffset = scrollView.content.rect.size;
        contentSizeOffset.Scale(scrollView.content.pivot);

        return scrollView.CalculateFocusedScrollPosition(itemCenterPoint + contentSizeOffset, isClamped);
    }
}