using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class AbstractColorItemModule : ItemModule
{
    public abstract bool IsTintColor { get; }
    public abstract Color CurrentColor { get; set; }
}
public class ColorItemModule : AbstractColorItemModule
{
    public event Action<ColorItemModule> onColorChanged;

    [SerializeField]
    protected bool isTintColor;
    [SerializeField]
    protected Color defaultColor;
    [SerializeField]
    protected List<Color> hsvAdjustedColors;

    protected string ColorSaveKey => $"{nameof(ColorItemModule)}_{m_ItemSO.guid}";

    public virtual int CurrentColorIndex
    {
        get
        {
            var currentHexCode = PlayerPrefs.GetString(ColorSaveKey, ColorUtility.ToHtmlStringRGBA(DefaultColor));
            for (int i = 0; i < hsvAdjustedColors.Count; i++)
            {
                var color = hsvAdjustedColors[i];
                if (ColorUtility.ToHtmlStringRGBA(color) == currentHexCode)
                    return i;
            }
            return 0;
        }
    }
    public override bool IsTintColor => isTintColor;
    public override Color CurrentColor
    {
        get
        {
            var currentHexCode = PlayerPrefs.GetString(ColorSaveKey, ColorUtility.ToHtmlStringRGBA(DefaultColor));
            foreach (var color in hsvAdjustedColors)
            {
                if (ColorUtility.ToHtmlStringRGBA(color) == currentHexCode)
                    return color;
            }
            return DefaultColor;
        }
        set
        {
            var color = hsvAdjustedColors[IndexOf(value)];
            var previousHexCode = ColorUtility.ToHtmlStringRGBA(CurrentColor);
            var currentHexCode = ColorUtility.ToHtmlStringRGBA(color);
            PlayerPrefs.SetString(ColorSaveKey, currentHexCode);
            if (currentHexCode != previousHexCode)
                onColorChanged?.Invoke(this);
        }
    }
    public virtual Color DefaultColor => hsvAdjustedColors[0];
    public virtual List<Color> HSVAdjustedColors => hsvAdjustedColors;
    public virtual List<Color> ColorPalette
    {
        get
        {
            if (IsTintColor)
            {
                return HSVAdjustedColors;
            }
            var colorAfterAdjustments = new List<Color>();
            foreach (var hsvAdjustedColor in HSVAdjustedColors)
            {
                float4 hsvc = hsvAdjustedColor.ToFloat4();
                hsvc.x -= 0.5f;
                float4 color = ApplyHSBEffect(defaultColor.ToFloat4(), hsvc);
                colorAfterAdjustments.Add(color.ToColor());
            }
            return colorAfterAdjustments;
        }
    }

    private float3 ApplyHue(float3 aColor, float aHue)
    {
        float angle = math.radians(aHue);
        float3 k = new float3(0.57735f, 0.57735f, 0.57735f);
        float cosAngle = math.cos(angle);
        //Rodrigues' rotation formula
        return aColor * cosAngle + math.cross(k, aColor) * math.sin(angle) + k * math.dot(k, aColor) * (1 - cosAngle);
    }

    private float4 ApplyHSBEffect(float4 startColor, float4 hsbc)
    {
        float _Hue = 360f * hsbc.x;
        float _Saturation = hsbc.y * 2f;
        float _Brightness = hsbc.z * 2f - 1f;
        float _Contrast = hsbc.w * 2f;

        float4 outputColor = startColor;
        outputColor.xyz = ApplyHue(outputColor.xyz, _Hue);
        outputColor.xyz = (outputColor.xyz - 0.5f) * _Contrast + 0.5f;
        outputColor.xyz += _Brightness;
        float3 intensity = math.dot(outputColor.xyz, new float3(0.299f, 0.587f, 0.114f));
        outputColor.xyz = math.lerp(intensity, outputColor.xyz, _Saturation);

        return outputColor;
    }

    private int IndexOf(Color color)
    {
        var currentHexCode = ColorUtility.ToHtmlStringRGBA(color);
        var colorPalette = ColorPalette;
        for (int i = 0; i < colorPalette.Count; i++)
        {
            if (ColorUtility.ToHtmlStringRGBA(colorPalette[i]) == currentHexCode)
                return i;
        }
        return 0;
    }
}
public class ManuallyColorItemModule : AbstractColorItemModule
{
    public ManuallyColorItemModule(bool isTintColor, Color currentColor)
    {
        this.isTintColor = isTintColor;
        this.currentColor = currentColor;
    }

    protected bool isTintColor;
    protected Color currentColor;

    public override bool IsTintColor
    {
        get => isTintColor;
    }

    public override Color CurrentColor
    {
        get => currentColor;
        set => currentColor = value;
    }
}