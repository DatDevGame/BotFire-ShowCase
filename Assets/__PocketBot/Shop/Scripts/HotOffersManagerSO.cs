using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "HotOffersManagerSO", menuName = "PocketBots/PackReward/HotOffersManagerSO")]
public class HotOffersManagerSO : SerializedScriptableObject
{
    public bool IsActivePack => m_isActivePack;

    [ReadOnly]
    public string m_Key;

    [SerializeField, BoxGroup("Config")] protected int m_TimeResetReward;
    protected string m_ActiveKey => $"Active-{m_Key}";
    protected bool m_isActivePack => PlayerPrefs.HasKey(m_ActiveKey);

    #region TimeReward
    protected virtual string m_LastRewardTime
    {
        get
        {
            return PlayerPrefs.GetString($"Time-{m_Key}", "0");
        }
        set
        {
            PlayerPrefs.SetString($"Time-{m_Key}", value);
            value = PlayerPrefs.GetString($"Time-{m_Key}", "0");
        }
    }

    public virtual DateTime LastRewardTime
    {
        get
        {
            long time = long.Parse(m_LastRewardTime);
            return new DateTime(time);
        }
        private set => m_LastRewardTime = (value.Ticks.ToString());
    }

    public virtual bool IsResetReward
    {
        get
        {
            TimeSpan interval = DateTime.Now - LastRewardTime;
            return interval.TotalSeconds > m_TimeResetReward;
        }
    }

    /// <summary>
    /// EX: 15MIN 20S - 15(timeValueFontSize), MIN(labelFontSize)
    /// </summary>
    /// <param name="timeValueFontSize"></param>
    /// <param name="labelFontSize"></param>
    /// <returns></returns>
    public virtual string GetRemainingTimeHandle(float timeValueFontSize, float labelFontSize)
    {
        TimeSpan interval = DateTime.Now - LastRewardTime;
        double remainingSeconds = m_TimeResetReward - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);

        string minuteAndSeconds = string.Format(
            "<size={0}>{1:00}<size={2}>M <size={0}>{3:00}<size={2}>S",
            timeValueFontSize, interval.Minutes, labelFontSize, interval.Seconds
        );

        string hourAndMinutes = string.Format(
            "<size={0}>{1:00}<size={2}>H <size={0}>{3:00}<size={2}>M",
            timeValueFontSize, interval.Hours, labelFontSize, interval.Minutes
        );

        string handleTextTimeSpanAds = interval.TotalMinutes >= 60 ? hourAndMinutes : minuteAndSeconds;

        return handleTextTimeSpanAds;
    }

    [Button("Reset Time", ButtonSizes.Large), GUIColor(1, 1, 0), HorizontalGroup("Action Button")]
    public virtual void ResetReward()
    {
        LastRewardTime = DateTime.Now;
    }

    [Button("Set Time Reward Complete", ButtonSizes.Large), GUIColor(1, 1, 0.7f), HorizontalGroup("Action Button")]
    public virtual void ResetNow()
    {
        LastRewardTime = DateTime.Now - TimeSpan.FromSeconds(m_TimeResetReward);
    }
    #endregion

    public virtual void ActivePack()
    {
        if (!m_isActivePack)
        {
            PlayerPrefs.SetInt(m_ActiveKey, 1);
            ResetReward();
        }
    }

#if UNITY_EDITOR
    [Button("Clear Active Key", ButtonSizes.Large), GUIColor(0.1f, 0.9f, 0.5f), HorizontalGroup("Action Button 2")]
    public virtual void ClearActiveKey()
    {
        PlayerPrefs.DeleteKey(m_ActiveKey);
    }

    [OnInspectorGUI]
    protected virtual void LoadEditor()
    {
        GenerateSaveKey();
    }

    protected virtual void GenerateSaveKey()
    {
            if (string.IsNullOrEmpty(m_Key) && !string.IsNullOrEmpty(name) && m_Key != name)
            {
                var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                m_Key = $"{name}_{guid}";
                UnityEditor.EditorUtility.SetDirty(this);
            }
    }
#endif
}
