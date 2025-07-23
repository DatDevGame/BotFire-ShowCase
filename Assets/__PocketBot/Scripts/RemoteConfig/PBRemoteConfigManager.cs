using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames;
using HyrphusQ.Helpers;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(-200)]
public class PBRemoteConfigManager : RemoteConfigManager
{
    private static bool isRemoteConfigReady = false;
    private static event Action _onRemoteConfigReady;

    public static event Action onRemoteConfigReady
    {
        add
        {
            if (isRemoteConfigReady)
                value?.Invoke();
            _onRemoteConfigReady += value;
        }
        remove
        {
            _onRemoteConfigReady -= value;
        }
    }

    [SerializeField]
    private List<GroupBasedABTestSO> groupBasedABTestSOs;
    [SerializeField]
    private List<FeatureBasedABTestSO> featureBasedABTestSOs;
    [SerializeField]
    private ABTestGroupDataSO testGroupDataSO;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void OnBeforeSceneLoad()
    {
        if (GameDataSO.Instance.isDevMode)
            return;
#if LatteGames_CLRemoteConfig
        CL_RemoteConfigHelper.OnRemoteReady += OnRemoteReady;

        void OnRemoteReady()
        {
            CL_RemoteConfigHelper.OnRemoteReady -= OnRemoteReady;
            NotifyEventRemoteConfigReady();
        }
#else
        NotifyEventRemoteConfigReady();
#endif
    }

    public static void NotifyEventRemoteConfigReady()
    {
        if (isRemoteConfigReady)
            return;
        isRemoteConfigReady = true;
        _onRemoteConfigReady?.Invoke();
    }

    public static bool IsRemoteConfigReady()
    {
        return isRemoteConfigReady;
    }

    public static string GetGroupName()
    {
        return (Instance as PBRemoteConfigManager).testGroupDataSO.GroupName;
    }
}
#if UNITY_EDITOR
[InitializeOnLoad]
public static class PBResetWhenExitPlayModeProcessor
{
    static PBResetWhenExitPlayModeProcessor()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
    {
        if (stateChange == PlayModeStateChange.EnteredEditMode)
        {
            var pbRemoteConfigManager = AssetDatabase.LoadAssetAtPath<PBRemoteConfigManager>("Assets/__PocketBot/PersistentManager/Prefabs/PB_RemoteConfigManager.prefab");
            var groupBasedABTestSOs = pbRemoteConfigManager.GetFieldValue<List<GroupBasedABTestSO>>("groupBasedABTestSOs");
            foreach (var abTestSO in groupBasedABTestSOs)
            {
                abTestSO.InjectData(0);
            }
        }
    }
}
#endif