//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using HyrphusQ.Helpers;
//using Sirenix.OdinInspector;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;

//public class ABTestGroup
//{
//    public ABTestGroup(Dictionary<string, object> remoteConfigValueDictionary)
//    {
//        this.remoteConfigValueDictionary = remoteConfigValueDictionary;
//    }

//    [ShowInInspector, HorizontalGroup]
//    private Dictionary<string, object> remoteConfigValueDictionary;

//    public string GetGroupName()
//    {
//        return remoteConfigValueDictionary.Get("ABTestGroupDataSO_group_name").ToString();
//    }

//    public void OverrideTTPRemoteConfig()
//    {
//#if TTP_ANALYTICS
//        Type ttpAnalyticsType = typeof(Tabtale.TTPlugins.TTPAnalytics);
//        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
//        FieldInfo field = ttpAnalyticsType.GetField("_overrideConfig", bindingFlags);
//        field.SetValue(null, remoteConfigValueDictionary);
//        MethodInfo method = ttpAnalyticsType.GetMethod("NotifyOnRemoteFetchCompletedEvent", bindingFlags);
//        method.Invoke(null, new object[] { true });
//#endif
//    }
//}

//[DefaultExecutionOrder(-200)]
//public class CheatABTestGroup : MonoBehaviour
//{
//    private const string RemoteUrl = "https://docs.google.com/spreadsheets/d/1UlGG8DQDuYNG2U3XoTRssQzkHk-i_VzkVRZdnMavI7w/export?format=csv&gid=1023358254";
//    [ShowInInspector]
//    private static bool IsDownloadCompleted = false;
//    [ShowInInspector]
//    private static List<ABTestGroup> TestGroups = new();

//    [SerializeField]
//    private bool enableCheatABTest = true;
//    [SerializeField]
//    private RectTransform cheatGroupUI;
//    [SerializeField]
//    private TMP_Dropdown dropDown;
//    [SerializeField]
//    private Button startButton;

//    private IEnumerator Start()
//    {
//        if (!enableCheatABTest)
//        {
//            dropDown.enabled = false;
//            startButton.onClick.RemoveListener(OnGroupSelected);
//            startButton.gameObject.SetActive(false);
//            cheatGroupUI.gameObject.SetActive(false);
//#if TTP_ANALYTICS
//            if (!CL_RemoteConfigHelper.Instance.IsRemoteReady())
//            {
//                CL_RemoteConfigHelper.OnRemoteReady += OnRemoteReady;
//                void OnRemoteReady()
//                {
//                    CL_RemoteConfigHelper.OnRemoteReady -= OnRemoteReady;
//                    PBRemoteConfigManager.NotifyEventRemoteConfigReady();
//                }
//            }
//            else
//            {
//                PBRemoteConfigManager.NotifyEventRemoteConfigReady();
//            }
//#else
//            PBRemoteConfigManager.NotifyEventRemoteConfigReady();
//#endif
//            yield break;
//        }
//#if TTP_ANALYTICS
//        yield return new WaitUntil(() => IsDownloadCompleted);
//        if (TestGroups.Count <= 1)
//        {
//            StartGame(0);
//            yield break;
//        }
//        dropDown.options.Clear();
//        dropDown.AddOptions(TestGroups.Select(item => item.GetGroupName()).ToList());
//        startButton.onClick.AddListener(OnGroupSelected);
//        yield return new WaitUntil(() => dropDown.TryGetComponentInChildren(out Canvas canvas));
//        var canvases = dropDown.GetComponentsInChildren<Canvas>(true);
//        foreach (var canvas in canvases)
//        {
//            canvas.sortingOrder = short.MaxValue;
//        }
//#else
//        StartGame(0);
//        yield break;
//#endif

//        void OnGroupSelected()
//        {
//            StartGame(dropDown.value);
//        }

//        void StartGame(int groupIndex)
//        {
//            dropDown.enabled = false;
//            startButton.onClick.RemoveListener(OnGroupSelected);
//            startButton.gameObject.SetActive(false);
//            cheatGroupUI.gameObject.SetActive(false);
//            if (TestGroups.IsValidIndex(groupIndex))
//                TestGroups[groupIndex].OverrideTTPRemoteConfig();
//            PBRemoteConfigManager.NotifyEventRemoteConfigReady();
//        }
//    }

//#if TTP_ANALYTICS
//    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
//    private static void OnBeforeSceneLoad()
//    {
//        if (!GameDataSO.Instance.isDevMode)
//            return;
//        DownloadRemoteConfigAsCsv(OnDownloadCompleted);

//        void OnDownloadCompleted(bool isSucceeded, string resultText)
//        {
//            IsDownloadCompleted = true;
//            if (!isSucceeded)
//                return;
//            TestGroups = ParseCsvData(resultText);
//        }
//    }

//    [Button]
//    private static void DownloadRemoteConfigAsCsv(Action<bool, string> callback)
//    {
//        var webRequest = UnityWebRequest.Get(RemoteUrl);
//        var asyncOperation = webRequest.SendWebRequest();
//        asyncOperation.completed += OnCompleted;

//        void OnCompleted(AsyncOperation asyncOperation)
//        {
//            bool isSucceeded = false;
//            string resultText = string.Empty;
//            try
//            {
//                isSucceeded = string.IsNullOrEmpty(webRequest.error);
//                if (isSucceeded)
//                {
//                    switch (webRequest.result)
//                    {
//                        case UnityWebRequest.Result.Success:
//                            resultText = webRequest.downloadHandler.text;
//                            Debug.Log(resultText);
//                            break;
//                        default:
//                            break;
//                    }
//                }
//                else
//                {
//                    Debug.LogError($"Failed: {webRequest.error} - Maybe there is a problem with your internet-connection Bruh");
//                }
//            }
//            catch (Exception e)
//            {
//                isSucceeded = false;
//                Debug.LogException(e);
//            }
//            callback.Invoke(isSucceeded, resultText);
//        }
//    }

//    private static List<ABTestGroup> ParseCsvData(string resultText)
//    {
//        var result = new List<Dictionary<string, object>>();
//        using (var reader = new StringReader(resultText))
//        {
//            string headerLine = reader.ReadLine();
//            if (headerLine == null)
//                throw new Exception("CSV data is empty");

//            string[] headers = headerLine.Split(',');
//            result = new List<Dictionary<string, object>>(new Dictionary<string, object>[headers.Length - 3]);
//            for (int i = 0; i < result.Count; i++)
//            {
//                result[i] = new Dictionary<string, object>();
//            }
//            string line;
//            while ((line = reader.ReadLine()) != null)
//            {
//                string[] values = line.Split(',');
//                if (values.Length <= 0)
//                    continue;

//                // Key 'CLOK Parameter ID'
//                string clokParameterId = values[2];

//                for (int i = 0; i < headers.Length - 3; i++)
//                {
//                    // Value of each groups
//                    string value = values[i + 3];
//                    if (string.IsNullOrEmpty(value))
//                        continue;
//                    result[i].Set(clokParameterId, value);
//                }
//            }
//        }

//        List<ABTestGroup> abTestGroups = new List<ABTestGroup>();
//        for (int i = 0; i < result.Count; i++)
//        {
//            abTestGroups.Add(new ABTestGroup(result[i]));
//        }
//        return abTestGroups;
//    }

//    public static List<ABTestGroup> GetTestGroups()
//    {
//        return TestGroups;
//    }

//#endif
//}