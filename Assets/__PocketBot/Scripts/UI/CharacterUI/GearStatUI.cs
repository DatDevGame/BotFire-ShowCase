using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GearStatUI : MonoBehaviour
{
    [SerializeField]
    private Color upgradeTxtColor;
    [SerializeField]
    private Color nextStatValueColor;
    [SerializeField]
    private float shinyFrequency = 2f;
    [SerializeField]
    private float shinySpeed = 0.5f;
    [SerializeField]
    private RangeFloatValue shinyMovingRange = new RangeFloatValue(-100f, 180f);
    [SerializeField]
    private TextMeshProUGUI statValueText;
    [SerializeField]
    private Image blueBGImage, greenBGImage;
    [SerializeField]
    private Image gradientShinyImage;

    private void Update()
    {
        if (gradientShinyImage != null && gradientShinyImage.gameObject.activeInHierarchy)
        {
            Vector2 anchoredPos = new Vector2(Mathf.Lerp(shinyMovingRange.minValue, shinyMovingRange.maxValue, Mathf.Repeat(Time.time * shinySpeed, shinyFrequency)), gradientShinyImage.rectTransform.anchoredPosition.y);
            gradientShinyImage.rectTransform.anchoredPosition = anchoredPos;
        }
    }

    public void SetStat(float currentStatValue, float nextStatValue)
    {
        statValueText.SetText($"{currentStatValue.ToRoundedText()}<size=44><voffset=-2><sprite=3></voffset></size><color=#{ColorUtility.ToHtmlStringRGB(nextStatValueColor)}>{(nextStatValue - currentStatValue).ToRoundedText()}");
    }

    public void SetStat(float currentStatValue)
    {
        SetStat($"{currentStatValue.ToRoundedText()}");
    }

    public void SetStat(string currentStatString)
    {
        statValueText.SetText(currentStatString);
    }

    public void SetUpgradable(bool isUpgradable)
    {
        blueBGImage.gameObject.SetActive(!isUpgradable);
        greenBGImage.gameObject.SetActive(isUpgradable);
    }
}