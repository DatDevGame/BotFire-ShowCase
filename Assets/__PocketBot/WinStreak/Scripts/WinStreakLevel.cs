using System.Collections;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using DG.Tweening;

public class WinStreakLevel : MonoBehaviour
{
    public SerializedDictionary<int, GameObject> LevelStreaks => m_LevelStreaks;

    [SerializeField, BoxGroup("Config")] private Sprite m_AvatarDefault;
    [SerializeField, BoxGroup("Config")] private Sprite m_AvatarBreak;

    [SerializeField, BoxGroup("Ref")] private TMP_Text m_WinSreakText;
    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<int, GameObject> m_LevelStreaks;

    [SerializeField, BoxGroup("Badge Animation")] private AnimationCurve scalingCurve;
    [SerializeField, BoxGroup("Badge Animation")] private Material m_BadgeMat;

    [SerializeField, BoxGroup("Optional")] private Slider m_CurrentSlider;

    private Material badgeMat;
    private Image m_CurrentLevelStreak;

    private void Awake()
    {
        badgeMat = Instantiate(m_BadgeMat);
        if (m_CurrentSlider != null)
        {
            m_CurrentSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnDestroy()
    {
        if (m_CurrentSlider != null)
        {
            m_CurrentSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        m_WinSreakText.SetText(((int)value).ToString());
    }

    public void LoadAvatar(int level, bool isBreakLevel = false, bool isPlayAnimation = false)
    {
        int closestLevel = m_LevelStreaks.ElementAt(0).Key; // Default to level 3 if no lower level is found
        m_WinSreakText.SetText(level.ToString());
        // Iterate through levels to find the closest lower level, or use level 3 if none exist below
        foreach (var kvp in m_LevelStreaks)
        {
            if (kvp.Key <= level && kvp.Key > closestLevel)
            {
                closestLevel = kvp.Key;
            }
        }

        // Activate only the GameObject for the closest level and deactivate others
        foreach (var kvp in m_LevelStreaks)
        {
            kvp.Value.SetActive(kvp.Key == closestLevel);
            kvp.Value.GetComponent<Image>().sprite = isBreakLevel ? m_AvatarBreak : m_AvatarDefault;

            if (kvp.Key == closestLevel)
            {
                m_CurrentLevelStreak = kvp.Value.GetComponent<Image>();

                if (isPlayAnimation)
                {
                    var images = m_CurrentLevelStreak.GetComponentsInChildren<Image>();
                    foreach (var image in images)
                    {
                        image.material = badgeMat;
                    }
                    badgeMat.SetFloat("_ColorWeight", 0);
                    badgeMat.DOFloat(1, "_ColorWeight", AnimationDuration.SSHORT).SetEase(scalingCurve).OnComplete(() =>
                    {
                        foreach (var image in images)
                        {
                            image.material = null;
                        }
                    });

                    m_CurrentLevelStreak.transform.DOScale(1.5f, AnimationDuration.SSHORT).SetEase(scalingCurve).OnUpdate(() =>
                    {
                        if (m_CurrentLevelStreak.transform.localScale.x >= 1.4f)
                        {
                            m_CurrentLevelStreak.transform.Find("Avatar").GetComponent<Image>().sprite = isBreakLevel ? m_AvatarBreak : m_AvatarDefault;
                        }
                    });
                }
                else
                {
                    m_CurrentLevelStreak.transform.Find("Avatar").GetComponent<Image>().sprite = isBreakLevel ? m_AvatarBreak : m_AvatarDefault;
                }
            }
        }

        if (level == 0)
            m_CurrentLevelStreak.transform.Find("Avatar").GetComponent<Image>().sprite = m_AvatarBreak;
    }

    public void LoadAvatarBreakStreak(bool isPlayAnimation = false)
    {
        if (m_CurrentLevelStreak != null)
        {
            if (isPlayAnimation)
            {
                var images = m_CurrentLevelStreak.GetComponentsInChildren<Image>();
                foreach (var image in images)
                {
                    image.material = badgeMat;
                }
                badgeMat.SetFloat("_ColorWeight", 0);
                badgeMat.DOFloat(1, "_ColorWeight", AnimationDuration.SSHORT).SetEase(scalingCurve).SetDelay(AnimationDuration.SSHORT).OnComplete(() =>
                {
                    foreach (var image in images)
                    {
                        image.material = null;
                    }
                });
                m_CurrentLevelStreak.transform.DOScale(1.5f, AnimationDuration.SSHORT).SetEase(scalingCurve).SetDelay(AnimationDuration.SSHORT).OnUpdate(() =>
                {
                    if (m_CurrentLevelStreak.transform.localScale.x >= 1.4f)
                    {
                        m_CurrentLevelStreak.transform.Find("Avatar").GetComponent<Image>().sprite = m_AvatarBreak;
                    }
                });
            }
            else
            {
                m_CurrentLevelStreak.transform.Find("Avatar").GetComponent<Image>().sprite = m_AvatarBreak;
            }
        }
    }
}
