using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GachaSystem.Core;
using HyrphusQ.Helpers;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public static class DateTimeExtensions
{
    public static string ToReadableTimeSpan(this DateTime dateTime, DateTime to, int length = 3)
    {
        var timeSpan = to - dateTime;

        // Handle special cases
        if (timeSpan.TotalSeconds < 0)
        {
            return "In the future";
        }
        else if (timeSpan.TotalSeconds < 60)
        {
            return $"{timeSpan.Seconds}s";
        }

        // Build the result string
        var sb = new StringBuilder();
        int weekAmount = 0;
        if (timeSpan.Days > 7)
        {
            weekAmount = Mathf.FloorToInt((float)timeSpan.Days / 7);
            sb.Append($"{weekAmount}w ");
            length--;
        }
        if (length > 0 && timeSpan.Days > 0)
        {
            sb.Append($"{timeSpan.Days - weekAmount * 7}d ");
            length--;
        }
        if (length > 0 && timeSpan.Hours > 0)
        {
            sb.Append($"{timeSpan.Hours}h ");
            length--;
        }
        if (length > 0 && timeSpan.Minutes > 0)
        {
            sb.Append($"{timeSpan.Minutes}m ");
            length--;
        }
        if (length > 0 && timeSpan.Seconds > 0)
        {
            sb.Append($"{timeSpan.Seconds}s");
        }

        // Remove trailing whitespace
        return sb.ToString().Trim();
    }

    public static string GetFormattedDate(this DateTime date, CultureInfo culture)
    {
        try
        {
            if (culture.Name == "en-US")
            {
                return date.ToString("dddd, MMMM dd, yyyy", culture);
            }
            else
            {
                DateTimeFormatInfo dateTimeFormat = culture.DateTimeFormat;
                return date.ToString(dateTimeFormat.ShortDatePattern, culture);
            }
        }
        catch (CultureNotFoundException)
        {
            return "Invalid culture code.";
        }
    }
}
