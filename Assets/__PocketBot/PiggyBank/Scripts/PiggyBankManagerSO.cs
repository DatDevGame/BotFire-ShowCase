using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "PiggyBankManagerSO", menuName = "PocketBots/PiggyBank/PiggyBankManagerSO")]
public class PiggyBankManagerSO : SerializedScriptableObject
{
    [ReadOnly]
    public string m_Key;

    public bool IsEnoughReward => m_CurrentGem >= m_PiggyBankLevelSOs[m_CurrentLevel].SavedGems;
    public bool IsDisplayed => m_HighestTrophy.value >= m_TrophyConditionDisplayed;
    public bool IsHasTimeFilled => PlayerPrefs.HasKey(m_KeyTimeFullFilled);
    public bool IsShowTheFirstTime => PlayerPrefs.HasKey(m_KeyShowTheFirstTime);
    public bool IsOpenFullPiggy => PlayerPrefs.HasKey(m_KeyOpenFullPiggy);
    public bool IsMaxLevel => m_CurrentLevel >= m_PiggyBankLevelSOs.Count;

    public PPrefIntVariable CurrentGem => m_CurrentGem;
    public PPrefIntVariable CurrentLevel => m_CurrentLevel;
    public Color SliderFullColor => m_SliderFullColor;
    public Color SliderNotFullColor => m_SliderNotFullColor;
    public Color SliderTimeOutColor => m_SliderTimeOutColor;
    public Sprite SpriteNotFull => m_SpriteNotFull;

    [SerializeField, BoxGroup("Config")] private int m_TimeEndReward;
    [SerializeField, BoxGroup("Config")] private int m_TrophyConditionDisplayed;
    [SerializeField, BoxGroup("Config")] private Color m_SliderFullColor;
    [SerializeField, BoxGroup("Config")] private Color m_SliderNotFullColor;
    [SerializeField, BoxGroup("Config")] private Color m_SliderTimeOutColor;
    [SerializeField, BoxGroup("Config")] private Sprite m_SpriteNotFull;

    [SerializeField, BoxGroup("Data")] private HighestAchievedPPrefFloatTracker m_HighestTrophy;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_CurrentGem;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_CurrentLevel;
    [SerializeField, BoxGroup("Data")] private Dictionary<int, PiggyBankLevelSO> m_PiggyBankLevelSOs;

    private string m_KeyTimeFullFilled => $"KeyTimeFullFilled-{m_Key}";
    private string m_KeyShowTheFirstTime => $"KeyShowTheFirstTime-{m_Key}";
    private string m_KeyOpenFullPiggy => $"KeyOpenFullPiggy-{m_Key}";

    public PiggyBankLevelSO GetPiggyBankCurrent()
    {
        return m_PiggyBankLevelSOs[m_CurrentLevel];
    }
    public PiggyBankLevelSO GetPiggyBankNextLevel()
    {
        if (m_CurrentLevel < m_PiggyBankLevelSOs.Count)
            return m_PiggyBankLevelSOs[m_CurrentLevel + 1];
        else
            return m_PiggyBankLevelSOs[m_PiggyBankLevelSOs.Count - 1];
    }

    public PiggyBankLevelSO GetPiggyBankLevelSO(int level)
    {
        if (m_PiggyBankLevelSOs == null && m_PiggyBankLevelSOs.Count <= 0)
        {
            Debug.LogError("PiggyBankLevelSOs is Null");
            return null;
        }

        return m_PiggyBankLevelSOs[level - 1];
    }

    public int GetLevelPiggyBank(PiggyBankLevelSO piggyBankLevelSO)
    {
        if (m_PiggyBankLevelSOs.ContainsValue(piggyBankLevelSO))
        {
            var key = m_PiggyBankLevelSOs.FirstOrDefault(pair => pair.Value == piggyBankLevelSO).Key;
            return key;
        }
        return -1;
    }

    public void PerKill(int killCount)
    {
        m_CurrentGem.value += GetPiggyBankCurrent().PerKill * killCount;
        if (IsEnoughReward)
            m_CurrentGem.value = (int)GetPiggyBankCurrent().SavedGems;
    }

    public void UpgradeLevel()
    {
        if(m_CurrentLevel < m_PiggyBankLevelSOs.Count)
            m_CurrentLevel.value++;

        DeleteKeyShowOpenPiggy();
    }

    public void ActiveTime()
    {
        if(!IsHasTimeFilled)
        {
            PlayerPrefs.SetInt(m_KeyTimeFullFilled, 1);
            ResetTimeStart();
        }
    }

    public void ActiveShowTheFirstTime() => PlayerPrefs.SetInt(m_KeyShowTheFirstTime, 1);
    public void ActiveShowOpenPiggy() => PlayerPrefs.SetInt(m_KeyOpenFullPiggy, 1);
    public void DeleteKeyShowOpenPiggy() => PlayerPrefs.DeleteKey(m_KeyOpenFullPiggy);

    public void DeleteKeyTime()
    {
        PlayerPrefs.DeleteKey(m_KeyTimeFullFilled);
    }

    public void ResetDeault()
    {
        m_CurrentGem.value = 0;
        DeleteKeyTime();
        DeleteKeyShowOpenPiggy();
    }

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

    [ShowInInspector] public virtual bool IsTimeOutReward
    {
        get
        {
            TimeSpan interval = DateTime.Now - LastRewardTime;
            return interval.TotalSeconds > m_TimeEndReward;
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
        double remainingSeconds = m_TimeEndReward - interval.TotalSeconds;
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

        if (remainingSeconds <= 0)
            handleTextTimeSpanAds = string.Format(
            "<size={0}>00<size={2}>M <size={0}>00<size={2}>S",
            timeValueFontSize, interval.Hours, labelFontSize, interval.Minutes);

        return handleTextTimeSpanAds;
    }

    public virtual string GetRemainingTimeout(float timeValueFontSize, float labelFontSize)
    {
        TimeSpan interval = DateTime.Now - LastRewardTime;
        double remainingSeconds = m_TimeEndReward - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);

        string timeOut = string.Format(
            "<size={0}>00<size={2}>h <size={0}>0<size={2}>m",
            timeValueFontSize, interval.Hours, labelFontSize, interval.Minutes
        );

        return timeOut;
    }

    [Button("Reset Time Start", ButtonSizes.Large), GUIColor(1, 1, 0), HorizontalGroup("Action Button")]
    public virtual void ResetTimeStart()
    {
        LastRewardTime = DateTime.Now;
    }

    [Button("Reset Time Complete", ButtonSizes.Large), GUIColor(1, 1, 0.7f), HorizontalGroup("Action Button")]
    public virtual void ResetNow()
    {
        LastRewardTime = DateTime.Now - TimeSpan.FromSeconds(m_TimeEndReward);
    }
    #endregion

#if UNITY_EDITOR
    [OnInspectorGUI]
    protected virtual void LoadEditor()
    {
        GenerateSaveKey();
    }

    [Button("Reset Data", ButtonSizes.Large), GUIColor(0.1f, 0.9f, 0.5f), HorizontalGroup("Action Button")]
    public void ResetData()
    {
        ResetDeault();
        m_CurrentLevel.value = 1;
        m_CurrentGem.value = 0;

        for (int i = 0; i < m_PiggyBankLevelSOs.Count; i++)
        {
            string key = $"KeyAutoOpenCurrentPiggy-{i+1}-{m_Key}";
            PlayerPrefs.DeleteKey(key);
        }
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
