using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class PBSkinAssetResourceManager : Singleton<PBSkinAssetResourceManager>, ILoadingResource
{
    private static event Action _onLoadDefaultSkinsCompleted;
    public static event Action onLoadDefaultSkinsCompleted
    {
        add
        {
            if (isLoadDefaultSkinCompleted)
                value?.Invoke();
            _onLoadDefaultSkinsCompleted += value;
        }
        remove
        {
            _onLoadDefaultSkinsCompleted -= value;
        }
    }
    private static bool isLoadDefaultSkinCompleted;

    [SerializeField]
    private List<PBPartManagerSO> partManagerSOs;
    private List<SkinSO> skins;

    protected override void Awake()
    {
        GameEventHandler.AddActionEvent(SceneManagementEventCode.OnStartLoadScene, OnStartLoadScene);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(SceneManagementEventCode.OnStartLoadScene, OnStartLoadScene);
    }

    private void OnStartLoadScene(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var destinationSceneName = parameters[0] as string;
        if (destinationSceneName == SceneName.MainScene.ToString())
        {
            // Unload all unused skins
            UpdateStateFlagsOfAllSkins();
            AddressableAssetManager.Instance.ReleaseAllUnusedAssetResources();
        }
    }

    private void Initialize()
    {
        if (skins == null)
        {
            skins = new List<SkinSO>();
            foreach (var partManagerSO in partManagerSOs)
            {
                var partSOs = partManagerSO.Parts;
                foreach (var partSO in partSOs)
                {
                    if (partSO.TryGetModule(out SkinItemModule skinModule))
                    {
                        skins.AddRange(skinModule.skins);
                        foreach (var skin in skinModule.skins)
                        {
                            AddressableAssetManager.Instance.AddManipulateAssetResources(skin, (int)SceneName.MainScene);
                        }
                    }
                }
            }
        }
    }

    private IAsyncTask LoadDefaultSkins()
    {
        List<SkinSO> defaultSkins = new List<SkinSO>();
        foreach (var partManagerSO in partManagerSOs)
        {
            var partSOs = partManagerSO.Parts;
            foreach (var partSO in partSOs)
            {
                if (partSO.TryGetModule(out SkinItemModule skinModule))
                {
                    defaultSkins.Add(skinModule.currentSkin);
                }
            }
        }
        IAsyncTask loadingAsyncTask = AddressableAssetManager.Instance.GetOrLoadAssetResources<Material>(defaultSkins.ToArray(), OnCompleted);
        return loadingAsyncTask;

        void OnCompleted(IAsyncTask loadingAsyncTask)
        {
            isLoadDefaultSkinCompleted = true;
            _onLoadDefaultSkinsCompleted?.Invoke();
        }
    }

    private void UpdateStateFlagsOfAllSkins()
    {
        foreach (var partManagerSO in partManagerSOs)
        {
            var partSOs = partManagerSO.Parts;
            foreach (var partSO in partSOs)
            {
                if (partSO.TryGetModule(out SkinItemModule skinModule))
                {
                    var currentSkin = skinModule.currentSkin;
                    foreach (var skin in skinModule.skins)
                    {
                        skin.stateFlags = skin == currentSkin ? StateFlags.InUsed : (skin.stateFlags == StateFlags.Unloaded ? StateFlags.Unloaded : StateFlags.NotUsed);
                    }
                }
            }
        }
    }

    public IAsyncTask Load()
    {
        Initialize();
        return LoadDefaultSkins();
    }
}