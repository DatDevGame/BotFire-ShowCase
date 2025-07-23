using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LatteGames.GameManagement;


#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

public class PBGameManager : Singleton<PBGameManager>
{
    private const string PBGameManagerName = "PBGameManager";
    private const string PBGameManagerKeyId = "Prefab Guid";

    public static event Action _onSpawnCompleted;
    public static event Action onSpawnCompleted
    {
        add
        {
            if (isSpawned)
                value?.Invoke();
            _onSpawnCompleted += value;
        }
        remove
        {
            _onSpawnCompleted -= value;
        }
    }
    public static bool isSpawned => Instance != null;

    private static void NotifyEventSpawnCompleted()
    {
        _onSpawnCompleted?.Invoke();
    }
    /// <summary>
    /// Auto spawn PersistenceManager before scene is loaded (Resources version)
    /// *Note: It's only work when Project doesn't use Addressables (because we can't use Resources folder with Addressables, please use Addressables version below)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInstantiate_Resources()
    {
        PBRemoteConfigManager.onRemoteConfigReady += OnRemoteConfigReady;

        void OnRemoteConfigReady()
        {
            PBRemoteConfigManager.onRemoteConfigReady -= OnRemoteConfigReady;
            Instantiate(Resources.Load(PBGameManagerName)).name = PBGameManagerName;
            NotifyEventSpawnCompleted();
        }
    }
    /// <summary>
    /// Auto spawn PersistenceManager before scene is loaded (Addressables version) (it has not been tested yet!)
    /// </summary>
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void AutoInstantiate_Addressables()
    //{
    //    var asyncOperation = Addressables.InstantiateAsync<PFIGameManager>(PFIGameManagerKeyId);
    //    asyncOperation.Completed += OnInstantiateCompleted;

    //    void OnInstantiateCompleted(AsyncOperationHandle<PFIGameManager> asyncOperation)
    //    {
    //        asyncOperation.Result.name = PFIGameManagerName;
    //        NotifyEventSpawnCompleted();
    //    }
    //}

    [SerializeField]
    private List<ScriptableObject> m_PersistentSOs;

    private void Start()
    {
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearLogConsole();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneName.MainScene);
        }
    }
    private void ClearLogConsole()
    {
	    var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
#endif

    private void OnApplicationQuit()
    {
        // Reset screen dimming
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }
}