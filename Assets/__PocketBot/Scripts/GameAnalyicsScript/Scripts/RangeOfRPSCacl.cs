using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "RangeRPSCalc", menuName = "PocketBots/Analytics/RangeRPSCalc")]
public class RangeOfRPSCacl : PPrefIntVariable
{
    public string Representation;
    public List<RangeRPSValue> rangeRPSValues;

    [Button("Test Value")]
    public float GetResultForValue(float value)
    {
        if (value <= -100)
            return -6;
        if (value >= 100)
            return 6;

        foreach (var range in rangeRPSValues)
        {
            if (value >= range.MinValue && value <= range.MaxValue)
            {
                return range.Result;
            }
        }
        return 0f; // Trả về giá trị mặc định
    }

    [Button("Load")]
    private void ParseRanges()
    {
        rangeRPSValues = new List<RangeRPSValue>();

        // Split the string by comma and newline to get each range
        var ranges = Representation.Split(new[] { ",\n", "|" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var range in ranges)
        {
            // Example: (-0.15,0.15) = 0
            var parts = range.Split(new[] { " = " }, StringSplitOptions.None);
            var rangePart = parts[0].Trim();
            var resultPart = parts[1].Trim();

            bool minInclusive = rangePart.StartsWith("[");
            bool maxInclusive = rangePart.EndsWith("]");

            rangePart = rangePart.Trim('(', ')', '[', ']');
            var values = rangePart.Split(',');

            float minValue = float.Parse(values[0], CultureInfo.InvariantCulture);
            float maxValue = float.Parse(values[1], CultureInfo.InvariantCulture);

            // Boundaries are determined based on inclusivity
            // If not inclusive, add or subtract a small value to represent > or <
            if (!minInclusive) minValue += float.Epsilon; // > minValue
            if (!maxInclusive) maxValue -= float.Epsilon; // < maxValue

            float result = float.Parse(resultPart, CultureInfo.InvariantCulture);

            rangeRPSValues.Add(new RangeRPSValue
            {
                MinValue = minValue + (minInclusive ? 0 : 0.01f),
                MaxValue = maxValue - (maxInclusive ? 0 : 0.01f),
                Result = result
            });;
        }
    }
}

[System.Serializable]
public class RangeRPSValue
{
    public float MinValue;
    public float MaxValue;
    public float Result;
}
