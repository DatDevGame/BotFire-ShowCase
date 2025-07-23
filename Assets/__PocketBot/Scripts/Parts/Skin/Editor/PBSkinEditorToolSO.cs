using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

[WindowMenuItem("General", "Skin Editor Tool", "Assets/__PocketBot/")]
public class PBSkinEditorToolSO : ScriptableObject
{
    private const string MaterialAssetPath = "Assets/__PocketBot/RobotParts/Materials/Parts";
    private const string TextureAssetPath = "Assets/__PocketBot/RobotParts/Textures/Parts";

    [SerializeField]
    private AddressableAssetGroup skinGroup;
    [SerializeField]
    private Material templatePartMaterial;
    [SerializeField]
    private Texture2D templateTexture;
    [SerializeField, FolderPath]
    private string fbxModelPath;
    [SerializeField, FolderPath]
    private string skinIconPath;
    [SerializeField]
    private List<PBPartManagerSO> partManagerSOs;
    [SerializeField]
    private List<PBPartSO> partSOs;
    [SerializeField]
    private List<SkinSO> skins;
    [SerializeReference]
    private Requirement requirementTree;

    private static void AddNewSkinToTheLast(PBPartSO partSO)
    {
        if (partSO.TryGetModule(out SkinItemModule skinModule))
            skinModule.skins.Add(CreateNewSkin(skinModule.skins.Count));
        else
        {
            skinModule = partSO.GetOrAddModule<SkinItemModule>();
            skinModule.skins.Add(CreateNewSkin(0));
        }

        SkinSO CreateNewSkin(int index)
        {
            var skin = CreateInstance<SkinSO>();
            skin.name = $"Skin_{index}";
            skin.SetFieldValue("m_guid", GenerateSkinId());
            skin.SetFieldValue("m_PartSO", partSO);
            AssetDatabase.AddObjectToAsset(skin, AssetDatabase.GetAssetPath(partSO));
            AssetDatabase.SaveAssetIfDirty(partSO);
            return skin;

            string GenerateSkinId()
            {
                List<SkinSO> skins = EditorUtils.FindAssetsOfType<SkinSO>();
                string skinId = $"{partSO.guid}_{Guid.NewGuid()}";
                while (skins.Any(skin => skin.guid == skinId))
                {
                    skinId = $"{partSO.guid}_{Guid.NewGuid()}";
                }
                return skinId;
            }
        }
    }

    [MenuItem("Assets/Create/PocketBots/Skin")]
    private static void CreateSkin()
    {
        var objects = Selection.objects;
        foreach (var obj in objects)
        {
            if (obj is PBPartSO partSO)
            {
                AddNewSkinToTheLast(partSO);
                EditorUtility.SetDirty(partSO);
            }
        }
    }

    public static string ReplaceAt(string input, int index, char newChar)
    {
        if (input == null)
        {
            throw new ArgumentNullException("input");
        }
        StringBuilder builder = new StringBuilder(input);
        builder[index] = newChar;
        return builder.ToString();
    }

    private Material CreateMaterial(Texture texture)
    {
        var name = ReplaceAt(texture.name, texture.name.LastIndexOf("_"), ' ');
        var instance = Instantiate(templatePartMaterial);
        instance.name = name;
        instance.mainTexture = texture;
        var assetPath = $"{MaterialAssetPath}/{name}.mat";
        AssetDatabase.CreateAsset(instance, assetPath);
        return instance;
    }

    [HorizontalGroup("Split0", 0.5f)]
    [Button(ButtonSizes.Large)]
    private void GenerateMaterialsFromTextures()
    {
        var textures = EditorUtils
            .FindAssetsOfType<Texture>(TextureAssetPath)
            .ToList();
        foreach (var texture in textures)
        {
            CreateMaterial(texture);
        }
        AssetDatabase.Refresh();
    }

    [HorizontalGroup("Split0", 0.5f)]
    [Button(ButtonSizes.Large)]
    private void CreateOrFetchSkinModuleOfParts()
    {
        foreach (var partManagerSO in partManagerSOs)
        {
            foreach (var partSO in partManagerSO.Parts)
            {
                if (partSOs.Count <= 0)
                {
                    CreateOrFetchSkinModule(partSO);
                    EditorUtility.SetDirty(partSO);
                }
                else if (partSOs.Contains(partSO))
                {
                    CreateOrFetchSkinModule(partSO);
                    EditorUtility.SetDirty(partSO);
                }
            }
        }

        void CreateOrFetchSkinModule(PBPartSO partSO)
        {
            var materialAssetPath = "Assets/__PocketBot/RobotParts/Materials/Parts";
            var uniqueMaterials = GetUniqueMaterials();
            if (uniqueMaterials.Count <= 0)
                return;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            partSO.RemoveModule(partSO.GetModule<ColorItemModule>());
            var skinModule = partSO.GetOrAddModule<SkinItemModule>();
            if (uniqueMaterials.Count > skinModule.skins.Count)
            {
                var diffCount = uniqueMaterials.Count - skinModule.skins.Count;
                var skinCount = skinModule.skins.Count;
                for (int i = 0; i < diffCount; i++)
                {
                    AddNewSkinToTheLast(partSO);
                }
            }
            for (int i = 0; i < skinModule.skins.Count; i++)
            {
                var skin = skinModule.skins[i];
                var materials = uniqueMaterials[i];
                var assetReferences = new AssetReference[materials.Count];
                for (int j = 0; j < materials.Count; j++)
                {
                    var assetPath = AssetDatabase.GetAssetPath(materials[j]);
                    var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                    var assetEntry = settings.CreateOrMoveEntry(assetGuid, skinGroup);
                    assetReferences[j] = new AssetReference(assetEntry.guid);
                }
                skin.SetFieldValue("m_SkinMaterialReferences", assetReferences);
            }
            EditorUtility.SetDirty(partSO);

            List<List<Material>> GetUniqueMaterials()
            {
                var materials = EditorUtils.FindAssetsOfType<Material>(materialAssetPath)
                    .Where(material => material.name.Contains($"{partSO.name.ToString().ToLower()}"))
                    .ToList();
                if (materials == null || materials.Count <= 0)
                    return default;
                var uniqueMaterialDictionary = new Dictionary<string, List<Material>>();
                foreach (var material in materials)
                {
                    var index = material.name.LastIndexOf("(");
                    var materialName = material.name;
                    if (index != -1)
                    {
                        materialName = material.name.Substring(0, index - 1);
                    }
                    if (!uniqueMaterialDictionary.TryGetValue(materialName, out var mats))
                    {
                        mats = new List<Material>();
                        uniqueMaterialDictionary.Add(materialName, mats);
                    }
                    mats.Add(material);
                }
                return uniqueMaterialDictionary.Values.ToList();
            }
        }
    }

    // [HorizontalGroup("Split1", 0.5f)]
    // [Button(ButtonSizes.Large)]
    // private void AddComponentPartSkinToPrefab()
    // {
    //     foreach (var partManagerSO in partManagerSOs)
    //     {
    //         foreach (var partSO in partManagerSO.Parts)
    //         {
    //             AddComponentPartSkin(partSO);
    //             EditorUtility.SetDirty(partSO);
    //         }
    //     }

    //     void AddComponentPartSkin(PBPartSO partSO)
    //     {
    //         var modelPrefab = partSO.GetModelPrefabAsGameObject();
    //         if (modelPrefab == null)
    //             return;
    //         // var assetPath = AssetDatabase.GetAssetPath(modelPrefab);
    //         // modelPrefab = PrefabUtility.LoadPrefabContents(assetPath);
    //         // PBPartSkin partSkin = modelPrefab.AddComponent<PBPartSkin>();
    //         // foreach (var partColor in modelPrefab.GetComponentsInChildren<PBPartColor>(true))
    //         // {
    //         //     DestroyImmediate(partColor, true);
    //         // }
    //         var renderers = modelPrefab.GetComponentsInChildren<MeshRenderer>();
    //         // var dictionary = new Dictionary<int, List<Renderer>>();
    //         foreach (var renderer in renderers)
    //         {
    //             int count = renderer.sharedMaterials.Length;
    //             // renderer.sharedMaterials = new Material[count]
    //             // .FillAll(AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat"));
    //             // var rens = dictionary.Get(count);
    //             // if (rens == null)
    //             //     rens = new List<Renderer>();
    //             // rens.Add(renderer);
    //             // dictionary.Set(count, rens);
    //             if (count >= 2)
    //             {
    //                 Debug.Log($"Prefab {modelPrefab} - {renderer} has more than 2 sub mesh: {count}", modelPrefab);
    //             }
    //         }
    //         // foreach (var keyValuePair in dictionary)
    //         // {
    //         //     var skinPair = new PBPartSkin.SkinPair();
    //         //     skinPair.materialIndices.AddRange(CreateMaterialIndices(keyValuePair.Key));
    //         //     skinPair.renderers.AddRange(keyValuePair.Value);
    //         //     partSkin.skinPairs.Add(skinPair);
    //         // }
    //         // PrefabUtility.SaveAsPrefabAsset(modelPrefab, assetPath);
    //         // PrefabUtility.UnloadPrefabContents(modelPrefab);
    //     }

    //     // List<int> CreateMaterialIndices(int count)
    //     // {
    //     //     var list = new List<int>();
    //     //     for (int i = 0; i < count; i++)
    //     //     {
    //     //         list.Add(i);
    //     //     }
    //     //     return list;
    //     // }
    // }

    [HorizontalGroup("Split1", 0.5f)]
    [Button(ButtonSizes.Large)]
    private void UpdateFbxModels()
    {
        List<GameObject> fbxModels = AssetDatabase.LoadAllAssetsAtPath(fbxModelPath).Select(obj => obj as GameObject).ToList();
        Debug.Log($"Models Count: {fbxModels.Count}");
        foreach (var fbxModel in fbxModels)
        {
            var modelImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(fbxModel)) as ModelImporter;
            if (modelImporter.materialImportMode == ModelImporterMaterialImportMode.None)
                continue;
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            modelImporter.SaveAndReimport();
        }
    }

    [HorizontalGroup("Split1", 0.5f)]
    [Button(ButtonSizes.Large)]
    private void AddUnlockRequirementToSkins()
    {
        for (int i = 0; i < skins.Count; i++)
        {
            var skin = skins[i];
            var oldUnlockableModule = skin.GetModule<UnlockableItemModule>();
            skin.RemoveModule(oldUnlockableModule);
            var unlockableModule = skin.GetOrAddModule<SkinUnlockableItemModule>();
            var requirement = requirementTree;
            if (requirement is Requirement_RewardedAd requirement_RewardedAd)
            {
                requirement = new Requirement_RewardedAd
                {
                    requiredRewardedAd = requirement_RewardedAd.requiredRewardedAd,
                    itemSO = skin
                };
            }
            unlockableModule.SetFieldValue("m_UnlockRequirementTree", requirement);
            EditorUtility.SetDirty(skin);
        }
    }

    [HorizontalGroup("Split2", 0.5f)]
    [Button(ButtonSizes.Large)]
    private void FetchPartThumbnails()
    {
        foreach (var partManagerSO in partManagerSOs)
        {
            foreach (var partSO in partManagerSO.Parts)
            {
                if (partSO.PartType == PBPartType.Body && partSO.Cast<PBChassisSO>().IsSpecial)
                    break;
                if (partSO.TryGetModule(out MonoImageItemModule imageModule) && partSO.TryGetModule(out SkinItemModule skinItemModule))
                {
                    imageModule.SetFieldValue("m_ThumbnailImage", skinItemModule.defaultSkin.GetThumbnailImage());
                }
                EditorUtility.SetDirty(partSO);
            }
        }
    }

    [HorizontalGroup("Split2", 0.5f)]
    [Button(ButtonSizes.Large)]
    private void FetchSkinThumbnails()
    {
        foreach (var partManagerSO in partManagerSOs)
        {
            foreach (var partSO in partManagerSO.Parts)
            {
                if (partSO.TryGetModule(out SkinItemModule skinItemModule))
                {
                    var skins = skinItemModule.skins;
                    for (int i = 0; i < skins.Count; i++)
                    {
                        Debug.Log($"{partSO} - Skin {i}");
                        if (partSO.PartType == PBPartType.Body && partSO.Cast<PBChassisSO>().IsSpecial)
                        {
                            SetThumbnail(partSO.GetThumbnailImage(), skins[i]);
                        }
                        else
                        {
                            string iconName = i == 0 ? $"{skinIconPath}/{partSO.name.ToLower()}_icon.png" : $"{skinIconPath}/{partSO.name.ToLower()}_icon_{i - 1}.png";
                            SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>(iconName), skins[i]);
                        }
                    }
                }
                EditorUtility.SetDirty(partSO);
            }
        }

        void SetThumbnail(Sprite sprite, SkinSO skinSO)
        {
            var imageModule = skinSO.GetOrAddModule<MonoImageItemModule>();
            imageModule.SetFieldValue("m_ThumbnailImage", sprite);
            EditorUtility.SetDirty(skinSO);
        }
    }

    #region Unit-Test
    // [Button]
    // private void TestSkin()
    // {
    //     for (int i = 0; i < skins.Count; i++)
    //     {
    //         var skin = skins[i];
    //         var isUnlocked = skin.IsUnlocked();
    //         // Skin that already unlocked
    //         if (isUnlocked)
    //         {
    //             Debug.Log($"{skin} already unlocked");
    //         }
    //         else
    //         {
    //             // Skin unlock by currency
    //             if (skin.IsPricedItem())
    //             {
    //                 Debug.Log($"{skin} require {skin.GetPrice()} {skin.GetCurrencyType()} currency to unlock");
    //             }
    //             // Skin unlock by RV
    //             else if (skin.IsRVItem())
    //             {
    //                 Debug.Log($"{skin} require {skin.GetRVWatchedCount()}/{skin.GetRequiredRVCount()} RV to unlock");
    //             }
    //         }
    //     }
    // }
    #endregion
}