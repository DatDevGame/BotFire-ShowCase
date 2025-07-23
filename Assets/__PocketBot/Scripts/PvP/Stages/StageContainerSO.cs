using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "StageContainerSO", menuName = "PocketBots/Stage/StageContainerSO", order = 0)]

public class StageContainerSO : SerializedScriptableObject
{
    public List<PBFightingStage> list = new List<PBFightingStage>();
#if UNITY_EDITOR
    [BoxGroup("Editor Only")]
    [FolderPath]
    public string productsFolderPath;
    [BoxGroup("Editor Only")]
    [Button("GetProducts")]
    void GetProducts()
    {
        if (list != null)
        {
            list.Clear();
        }
        list = FindPrefabsOfType<PBFightingStage>("prefab", productsFolderPath);
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [BoxGroup("Editor Only")]
    [FolderPath]
    public string thumbnailFolderPath;
    [BoxGroup("Editor Only")]
    [Button("AssignThumbnails")]
    void AssignThumbnails()
    {
        if (list != null)
        {
            var thumbnailList = FindPrefabsOfType<Sprite>("texture", thumbnailFolderPath);
            foreach (var item in list)
            {
                var thumbnail = thumbnailList.Find(x => x.name == item.name);
                if (thumbnail != null)
                {
                    item.SetFieldValue("thumbnail", thumbnail);
                    UnityEditor.EditorUtility.SetDirty(item);
                }
                else
                {
                    Debug.Log($"Can't find the thumbnail of {item.name}");
                }
            }
        }
    }

    public static List<T> FindPrefabsOfType<T>(string typeName, string path = "Assets") where T : UnityEngine.Object
    {
        var guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeName}", new[] { path });
        List<T> result = new List<T>();
        foreach (var t in guids)
        {
            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(t);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                result.Add(asset);
            }
        }
        return result;
    }
#endif
}

