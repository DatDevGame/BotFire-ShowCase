using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;
using UnityEditor;
using HyrphusQ.Helpers;

[Serializable]
public class SkinSO : ItemSO, IAddressableAssetResource, IRandomizable
{
    [SerializeField]
    protected float m_Probability = 0.2f;
    [SerializeField, ReadOnly, PropertyOrder(-1)]
    protected PBPartSO m_PartSO;
    [SerializeField]
    protected AssetReference[] m_SkinMaterialReferences;

    [NonSerialized]
    protected StateFlags m_StateFlags;
    [NonSerialized]
    protected Material[] m_SkinMaterials;

    public float Probability
    {
        get => m_Probability;
        set => m_Probability = value;
    }
    public StateFlags stateFlags
    {
        get => m_StateFlags;
        set => m_StateFlags = value;
    }
    public Material[] skinMaterials
    {
        get
        {
#if UNITY_EDITOR
            // Return materials directly from AssetReference.editorAsset when running on editor mode
            if (!Application.isPlaying)
            {
                return m_SkinMaterialReferences.Select(assetReference => assetReference.editorAsset as Material).ToArray();
            }
#endif
            return m_SkinMaterials;
        }
    }
    public Sprite icon => this.GetThumbnailImage();
    public PBPartSO partSO => m_PartSO;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        // Call OnValidate for all modules
        for (int i = 0; i < m_ItemModules.Count; i++)
        {
            var itemModule = m_ItemModules[i];
            itemModule.InvokeMethod<object>("OnValidate");
        }
    }

    [Button(ButtonSizes.Small), PropertyOrder(1)]
    private void DeleteSkin()
    {
        if (m_PartSO.TryGetModule(out SkinItemModule skinModule))
        {
            skinModule.skins.Remove(this);
            AssetDatabase.RemoveObjectFromAsset(this);
            for (int i = 0; i < skinModule.skins.Count; i++)
            {
                skinModule.skins[i].name = $"Skin_{i}";
            }
        }
        EditorUtility.SetDirty(m_PartSO);
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_PartSO));
    }
#endif

    public AssetReference[] CollectResources()
    {
        return m_SkinMaterialReferences;
    }

    public void ReleaseResources()
    {
        m_SkinMaterials = null;
    }

    public IAsyncTask<Material[]> GetOrLoadSkinResources()
    {
        return AddressableAssetManager.Instance.GetOrLoadAssetResources<Material>(this);
    }

    public void GetOrLoadSkinResources(Action<Material[]> callback)
    {
        if (m_SkinMaterials != null)
        {
            callback?.Invoke(m_SkinMaterials);
            return;
        }
        AddressableAssetManager.Instance.GetOrLoadAssetResources<Material>(this, OnCompleted);

        void OnCompleted(IAsyncTask<Material[]> loadingAsyncTask)
        {
            m_SkinMaterials = loadingAsyncTask.result;
            callback?.Invoke(m_SkinMaterials);
        }
    }
}