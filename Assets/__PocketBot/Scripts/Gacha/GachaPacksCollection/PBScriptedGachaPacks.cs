using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PBScriptedGachaPacks", menuName = "PocketBots/Gacha/PBScriptedGachaPacks")]
public class PBScriptedGachaPacks : SerializableScriptableObject
{
    [SerializeReference]
    List<PBManualGachaPack> manualGachaPacks;
    [SerializeReference]
    PPrefIntVariable scriptedGachaPacksIndex;

    public PPrefIntVariable ScriptedGachaPacksIndex => scriptedGachaPacksIndex;
    public PBManualGachaPack currentManualGachaPack => manualGachaPacks[scriptedGachaPacksIndex.value];
    public bool isRemainPack => scriptedGachaPacksIndex.value < manualGachaPacks.Count;
    [NonSerialized]
    public IResourceLocationProvider resourceLocationProvider;

    public virtual ShopProductSO GetCurrentShopProductSO(PBManualGachaPack manualGachaPack)
    {
        var sameTypePacks =
            manualGachaPacks.FindAll(x => x.SimulationFromGachaPack == manualGachaPack.SimulationFromGachaPack);
        return sameTypePacks[GetPackTypeCurrentIndex(manualGachaPack.SimulationFromGachaPack)].ShopProductSO;
    }

    public virtual void IncreasePackTypeCurrentIndex(PBManualGachaPack manualGachaPack)
    {
        SetPackTypeCurrentIndex(manualGachaPack.SimulationFromGachaPack,
                                GetPackTypeCurrentIndex(manualGachaPack.SimulationFromGachaPack) + 1);
    }

    public virtual int GetPackTypeCurrentIndex(PBGachaPack gachaPack)
    {
        return PlayerPrefs.GetInt($"{guid}_{gachaPack.guid}_PackTypeCurrentIndex", 0);
    }

    public virtual void SetPackTypeCurrentIndex(PBGachaPack gachaPack, int value)
    {
        PlayerPrefs.SetInt($"{guid}_{gachaPack.guid}_PackTypeCurrentIndex", value);
    }

    public void GrantReward()
    {
        if (PackDockManager.Instance.TryToAddPack(currentManualGachaPack))
        {
            scriptedGachaPacksIndex.value++;
        }
    }

    public void Next()
    {
        scriptedGachaPacksIndex.value++;
    }

#if UNITY_EDITOR
    [BoxGroup("Editor Only")]
    [FolderPath]
    public string productsFolderPath;
    [BoxGroup("Editor Only")]
    [Button]
    void GetProducts()
    {
        if (manualGachaPacks != null)
        {
            manualGachaPacks.Clear();
        }
        manualGachaPacks = EditorUtils.FindAssetsOfType<PBManualGachaPack>(productsFolderPath);
        foreach (var item in manualGachaPacks)
        {
            item.scriptedGachaPacks = this;
            EditorUtility.SetDirty(item);
        }
        EditorUtility.SetDirty(this);
    }

    [OnInspectorGUI, PropertyOrder(100)]
    private void OnInspectorGUI()
    {
        var centerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("=============== PlayerPref Variables ===============", centerStyle);
        EditorGUILayout.LabelField("============= Pack Type Current Index =============", centerStyle);
        var distinctPacks = manualGachaPacks.GroupBy((x) => x.SimulationFromGachaPack).Select(group => group.First());
        foreach (var item in distinctPacks)
        {
            var gachaPack = item.SimulationFromGachaPack;
            var packTypeCurrentIndex =
                EditorGUILayout.IntField(item.GetDisplayName(), GetPackTypeCurrentIndex(gachaPack));
            SetPackTypeCurrentIndex(gachaPack, packTypeCurrentIndex);
        }
    }

    /// <summary>
    /// *NOTE: Do not delete this method
    /// </summary>
    void ClearAllProducts()
    {
        if (manualGachaPacks != null)
            manualGachaPacks.Clear();
        // Delete all files within the directory
        if (Directory.Exists(productsFolderPath))
        {
            foreach (string file in Directory.EnumerateFiles(productsFolderPath))
            {
                FileUtil.DeleteFileOrDirectory(file);
            }
        }
        EditorUtility.SetDirty(this);

    }

    /// <summary>
    /// *NOTE: Do not delete this method
    /// </summary>
    void CreateFolderIfNotFound()
    {
        var path = Path.Combine(Path.GetDirectoryName(Application.dataPath), productsFolderPath);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
#endif
}