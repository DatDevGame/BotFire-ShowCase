using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using HightLightDebug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PackReward
{
    public enum PackType
    {
        Currency,
        Item
    }

    public enum RequirementsPack
    {
        Ads,
        Coin,
        Gem
    }

    [CreateAssetMenu(fileName = "PackRewardSO", menuName = "PocketBots/PackReward/PackRewardSO")]
    public class PackRewardSO : SerializedScriptableObject
    {
        [PreviewField(ObjectFieldAlignment.Left), TitleGroup("Info"), PropertyOrder(0)]
        public Sprite icon;

        [ReadOnly]
        public string m_Key;

        [ReadOnly]
        public CurrencySO CurrencySO_Stand;
        [ReadOnly]
        public CurrencySO CurrencySO_Premium;

        public PackType PackType;

        [TitleGroup("Time")]
        [LabelText("Time Waiting for Reward (Seconds)")]
        public int TimeWaitingForReward;

        [TitleGroup("Requirements")]
        public ReducedValue ReducedValue;

        [TitleGroup("Resource (Reward)"), ShowIf("m_IsShowingCurrency")]
        public CurrencyPackReward CurrencyPack;

        [TitleGroup("Resource (Reward)"), HideIf("m_IsShowingCurrency")]
        public ItemPackReward ItemPack;

        private bool m_IsShowingCurrency => PackType == PackType.Currency;

        #region TimeReward
        public virtual string RemainingTime
        {
            get
            {
                TimeSpan interval = DateTime.Now - LastRewardTime;
                var remainingSeconds = TimeWaitingForReward - interval.TotalSeconds;
                interval = TimeSpan.FromSeconds(remainingSeconds);
                return string.Format("{0:00}h{1:00}m{2:00}s", interval.Hours, interval.Minutes, interval.Seconds);
            }
        }

        public virtual string RemainingTimeInMinute
        {
            get
            {
                TimeSpan interval = DateTime.Now - LastRewardTime;
                var remainingSeconds = TimeWaitingForReward - interval.TotalSeconds;
                interval = TimeSpan.FromSeconds(remainingSeconds);
                return string.Format("{0:00}MIN {1:00}s", interval.Minutes, interval.Seconds);
            }
        }

        protected virtual string lastRewardTime
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
                long time = long.Parse(lastRewardTime);
                return new DateTime(time);
            }
            private set => lastRewardTime = (value.Ticks.ToString());
        }

        public virtual bool IsGetReward
        {
            get
            {
                TimeSpan interval = DateTime.Now - LastRewardTime;
                return interval.TotalSeconds > TimeWaitingForReward;
            }
        }
        #endregion

        #region Requirements
        protected virtual string AdsKey => $"{RequirementsPack.Ads}{m_Key}";
        protected virtual void ResetWatchedAds() => PlayerPrefs.SetInt(AdsKey, 0);
        public int GetTotalAdsWatched() => PlayerPrefs.GetInt(AdsKey, 0);
        public bool IsEnoughAds => GetTotalAdsWatched() >= ReducedValue.Value;
        public virtual void SetAdsValue(int value = 1)
        {
            int oldValue = PlayerPrefs.GetInt(AdsKey, 0);
            PlayerPrefs.SetInt(AdsKey, oldValue + value);
        }

        public virtual bool IsClaim()
        {
            if (ReducedValue.RequirementsPack == RequirementsPack.Ads)
            {
                return GetTotalAdsWatched() >= ReducedValue.Value && IsGetReward;
            }
            else if (ReducedValue.RequirementsPack == RequirementsPack.Coin)
            {
                return CurrencySO_Stand.value >= ReducedValue.Value && IsGetReward;
            }
            else if (ReducedValue.RequirementsPack == RequirementsPack.Gem)
            {
                return CurrencySO_Premium.value >= ReducedValue.Value && IsGetReward;
            }
            return false;
        }
        #endregion

        #region Acquire & Spend
        protected virtual void AcquireResource(ResourceLocationProvider resourceLocationProvider)
        {
            switch (PackType)
            {
                case PackType.Currency:
                    HandleCurrencyAcquisition(resourceLocationProvider);
                    break;

                case PackType.Item:
                    DebugPro.PinkBold($"{ItemPack.ItemSO.name} - Card: +{ItemPack.Value}");
                    ItemPack.ItemSO.TryUnlockItem();
                    ItemPack.ItemSO.UpdateNumOfCards(ItemPack.ItemSO.GetNumOfCards() + ItemPack.Value);
                    break;

                default:
                    DebugPro.RedBold("Invalid PackType!");
                    break;
            }
        }

        protected virtual void AcquireCurrency(CurrencySO currency, ResourceLocationProvider resourceLocationProvider)
        {
            if (resourceLocationProvider != null)
            {
                currency.Acquire(CurrencyPack.Value, resourceLocationProvider.GetLocation(), resourceLocationProvider.GetItemId());
            }
            else
            {
                currency.AcquireWithoutLogEvent(CurrencyPack.Value);
            }
        }

        protected virtual void SpendCurrency(CurrencySO currency, ResourceLocationProvider resourceLocationProvider)
        {
            if (resourceLocationProvider != null)
            {
                currency.Spend(ReducedValue.Value, resourceLocationProvider.GetLocation(), resourceLocationProvider.GetItemId());
            }
            else
            {
                currency.SpendWithoutLogEvent(ReducedValue.Value);
            }
        }

        protected virtual void SpendResource(ResourceLocationProvider resourceLocationProvider)
        {
            switch (ReducedValue.RequirementsPack)
            {
                case RequirementsPack.Coin:
                    SpendCurrency(CurrencySO_Stand, resourceLocationProvider);
                    break;

                case RequirementsPack.Gem:
                    SpendCurrency(CurrencySO_Premium, resourceLocationProvider);
                    break;

                case RequirementsPack.Ads:
                    DebugPro.CyanBold("Spend Ads!");
                    break;

                default:
                    DebugPro.RedBold("Invalid RequirementsPack!");
                    break;
            }
        }

        protected virtual void HandleCurrencyAcquisition(ResourceLocationProvider resourceLocationProvider)
        {
            switch (CurrencyPack.CurrencyType)
            {
                case CurrencyType.Standard:
                    AcquireCurrency(CurrencySO_Stand, resourceLocationProvider);
                    break;

                case CurrencyType.Premium:
                    AcquireCurrency(CurrencySO_Premium, resourceLocationProvider);
                    break;

                default:
                    DebugPro.RedBold("Invalid RequirementsPack!");
                    break;
            }
        }
        #endregion

        [Button("Reset Time", ButtonSizes.Large), GUIColor(1, 1, 0), HorizontalGroup("Action Button")]
        public virtual void ResetReward()
        {
            ResetWatchedAds();
            LastRewardTime = DateTime.Now;
        }

        [Button("Set Time Reward Complete", ButtonSizes.Large), GUIColor(1, 1, 0.7f), HorizontalGroup("Action Button")]
        public virtual void ResetNow()
        {
            LastRewardTime = DateTime.Now - TimeSpan.FromSeconds(TimeWaitingForReward);
        }

        public virtual void Claim(ResourceLocationProvider resourceLocationProvider)
        {
            if (!IsClaim())
            {
                DebugPro.RedBold("You Can Not Claim!");
                return;
            }

            SpendResource(resourceLocationProvider);
            AcquireResource(resourceLocationProvider);
            ResetReward();
        }

        public virtual void ActivePack()
        {
            if (!PlayerPrefs.HasKey(m_Key))
            {
                PlayerPrefs.SetInt(m_Key, 1);
                ResetReward();
                ResetNow();
            }
        }

#if UNITY_EDITOR
        [Button("Claim", ButtonSizes.Large), GUIColor(0, 1, 0), HorizontalGroup("Action Button")]
        public virtual void ClaimEditor()
        {
            Claim(new ResourceLocationProvider(ResourceLocation.None, "Test"));
        }

        protected virtual void GenerateSaveKey()
        {
            if (string.IsNullOrEmpty(m_Key) && !string.IsNullOrEmpty(name))
            {
                var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                m_Key = $"{name}_{guid}";
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        [OnInspectorGUI]
        protected virtual void OnInspectorGUI()
        {
            GenerateSaveKey();
            if(CurrencySO_Stand == null || CurrencySO_Premium == null)
            {
                var standCurrency = AssetDatabase.LoadAssetAtPath<CurrencySO>("Assets/_HybridCasualLibrary/_InternalPackage/Economy/GameConfigs/CurrencySO_Standard.asset");
                var premiumCurrency = AssetDatabase.LoadAssetAtPath<CurrencySO>("Assets/_HybridCasualLibrary/_InternalPackage/Economy/GameConfigs/CurrencySO_Premium.asset");

                if(standCurrency != null)
                    CurrencySO_Stand = standCurrency;
                else
                    DebugPro.RedBold("Not Found StandCurrencySO");

                if(premiumCurrency != null)
                    CurrencySO_Premium = premiumCurrency;
                else
                    DebugPro.RedBold("Not Found PremiumCurrencySO");
            }

    
            if(TimeWaitingForReward == 0) return;
            // Create a new GUIStyle to change the label's color to yellow
            GUIStyle yellowTextStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.yellow },
                fontStyle = FontStyle.Bold, // Make the text bold for emphasis
                alignment = TextAnchor.MiddleCenter // Center-align the text
            };

            GUIStyle greenTextStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.green },
                fontStyle = FontStyle.Bold, // Make the text bold for emphasis
                alignment = TextAnchor.MiddleCenter // Center-align the text
            };

            GUIStyle cyanTextStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.cyan},
                fontStyle = FontStyle.Bold, // Make the text bold for emphasis
                alignment = TextAnchor.MiddleCenter // Center-align the text
            };

            // Create a GUIStyle for the separator lines
            GUIStyle separatorStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.grey },
                alignment = TextAnchor.MiddleCenter // Center-align the separator
            };

            // Disable GUI to prevent editing of the field
            GUI.enabled = false;

            // Design a separator line for better structure
            EditorGUILayout.LabelField("=================================", separatorStyle);

            // Display reward state based on canGetReward
            if (!IsClaim())
            {
                // Time reward information
                if(!IsGetReward)
                    EditorGUILayout.LabelField($"Time Reward: {RemainingTime}", yellowTextStyle);
                else
                {
                    EditorGUILayout.LabelField($"Time Reward: Complete", cyanTextStyle);

                    if(ReducedValue.RequirementsPack != RequirementsPack.Ads)
                        EditorGUILayout.LabelField($"{ReducedValue.RequirementsPack}: Not Enough", yellowTextStyle);
                }
            }
            else
            {
                // Indicate the reward is ready
                EditorGUILayout.LabelField($"You Can Claim Your Reward", cyanTextStyle);
            }

            // Add another separator line for better visual structure
            EditorGUILayout.LabelField("=================================", separatorStyle);

            if(ReducedValue.RequirementsPack == RequirementsPack.Ads)
            {
                EditorGUILayout.LabelField("=================================", separatorStyle);

                EditorGUILayout.LabelField($"Ads: {GetTotalAdsWatched()}/{ReducedValue.Value}", GetTotalAdsWatched() >= ReducedValue.Value ? cyanTextStyle : greenTextStyle);

                EditorGUILayout.LabelField("=================================", separatorStyle);
            }

            // Enable the GUI back
            GUI.enabled = true;

            }

#endif
    }
    [Serializable]
        public class CurrencyPackReward
        {
            public CurrencyType CurrencyType;
            public float Value;
        }

        [Serializable]
        public class ItemPackReward
        {
            public ItemSO ItemSO;
            public int Value;
        }

        [Serializable]
        public class ReducedValue
        {
            public RequirementsPack RequirementsPack;
            public float Value;
        }
}
