using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using HyrphusQ.Helpers;
using LatteGames.GameManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AddressableAssets;
#endif

public class PBPartSkin : MonoBehaviour
{
    [InfoBox("Some renderers refers to materials in addressable directly. Pls fix it", InfoMessageType.Error, "IsReferToAddressableMaterials")]
    [SerializeField]
    private List<SkinPair> m_SkinPairs = new List<SkinPair>();

    private PBPart m_Part;
    private SkinSO m_CurrentSkin;
    private SkinListView skinListView
    {
        get
        {
            return ObjectFindCache<SkinListView>.Get();
        }
    }
    public PBPart part
    {
        get
        {
            if (m_Part == null)
                m_Part = GetComponentInChildren<PBPart>();
            return m_Part;
        }
        set
        {
            m_Part = value;
            if (value != null)
            {
                if (value.RobotChassis.Robot.IsPreview
                    && SceneManager.GetActiveScene().name == SceneName.MainScene.ToString()
                    && skinListView != null && skinListView.currentSelectedCell != null
                    && part.PartSO.guid == skinListView.currentSelectedCell.item.Cast<SkinSO>().partSO.guid)
                {
                    EquipSkin(skinListView.currentSelectedCell.item.Cast<SkinSO>());
                }
                else
                {
                    EquipCurrentSkin(value.PartSO);
                }
            }
        }
    }
    public List<SkinPair> skinPairs => m_SkinPairs;

#if UNITY_EDITOR
    private int m_EditorSkinIndex = -1;
    private PBPartSO m_EditorPartSO;

    [OnInspectorGUI]
    private void OnInspectorGUI()
    {
        if (!enabled)
            return;
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null || prefabStage.prefabContentsRoot != gameObject)
            return;
        if (m_EditorPartSO == null)
            m_EditorPartSO = EditorUtils.FindAssetsOfType<PBPartSO>().FirstOrDefault(item => item.GetModelPrefabAsGameObject()?.name == name);
        if (m_EditorPartSO == null)
            return;
        var skinModule = m_EditorPartSO?.GetModule<SkinItemModule>();
        var skinIndex = EditorGUILayout.IntSlider("Skin Index", m_EditorSkinIndex, 0, skinModule?.skins.Count - 1 ?? 0);
        if (skinIndex != m_EditorSkinIndex)
        {
            m_EditorSkinIndex = skinIndex;
            if (m_EditorPartSO != null && m_EditorPartSO.TryGetModule(out skinModule))
            {
                UpdateSkin(skinModule.skins[m_EditorSkinIndex].skinMaterials);
            }
        }
    }

    private bool IsReferToAddressableMaterials()
    {
        if (m_EditorPartSO == null)
        {
            m_EditorPartSO = EditorUtils.FindAssetsOfType<PBPartSO>().FirstOrDefault(item => item.GetModelPrefabAsGameObject()?.name == name);
        }
        if (m_EditorPartSO == null)
            return false;
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var prefabSkin = m_EditorPartSO.GetModelPrefabAsGameObject().GetComponent<PBPartSkin>();
        if (prefabSkin != null && prefabSkin.m_SkinPairs != null)
        {
            foreach (var skinPair in prefabSkin.m_SkinPairs)
            {
                foreach (var renderer in skinPair.renderers)
                {
                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material != null && settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material))) != null)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    [GUIColor("#FF000077"), Button(size: ButtonSizes.Large, Icon = SdfIconType.Bug), ShowIf("IsReferToAddressableMaterials")]
    private void FixIt()
    {
        var partSO = EditorUtils.FindAssetsOfType<PBPartSO>().FirstOrDefault(item => item.GetModelPrefabAsGameObject()?.name == name);
        if (partSO == null)
            return;
        var prefabModel = partSO.GetModelPrefabAsGameObject();
        var prefabSkin = prefabModel.GetComponent<PBPartSkin>();
        foreach (var skinPair in prefabSkin.m_SkinPairs)
        {
            foreach (var renderer in skinPair.renderers)
            {
                renderer.sharedMaterials = new Material[renderer.sharedMaterials.Length]
                .FillAll(AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat"));
            }
        }
        PrefabUtility.SavePrefabAsset(prefabModel);
    }

    [Button(size: ButtonSizes.Large)]
    private void UpdateDefaultMaterials()
    {
        foreach (var skinPair in m_SkinPairs)
        {
            foreach (var renderer in skinPair.renderers)
            {
                renderer.sharedMaterials = new Material[renderer.sharedMaterials.Length]
                .FillAll(AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat"));
            }
        }
        EditorUtility.SetDirty(this);
    }
#endif

    private void Start()
    {
        if (part != null && part.PartSO != null)
        {
            if (part.RobotChassis.Robot.IsPreview && SceneManager.GetActiveScene().name == SceneName.MainScene.ToString())
            {
                if (skinListView != null)
                {
                    skinListView.onItemSelected.AddListener(OnSkinSelected);
                }
            }
            if (m_CurrentSkin == null)
                EquipCurrentSkin(part.PartSO);
        }
    }

    private void OnDestroy()
    {
        if (part != null && part.RobotChassis != null && part.RobotChassis.Robot != null && part.RobotChassis.Robot.IsPreview)
        {
            if (skinListView != null)
            {
                skinListView.onItemSelected.RemoveListener(OnSkinSelected);
            }
        }
    }

    private void OnSkinSelected(ItemListView.SelectedEventData eventData)
    {
        var selectedPartSO = eventData.itemSO.Cast<SkinSO>().partSO;
        if (part == null)
            return;
        if (!(selectedPartSO.PartType == PBPartType.Body && part.PartSO.PartType == PBPartType.Wheels))
        {
            if (part == null || part.PartSO.guid != selectedPartSO.guid)
                return;
        }
        EquipSkin(eventData.itemSO.Cast<SkinSO>());
    }

    public void UpdateSkin(Material[] materials)
    {
        if (materials == null || materials.Length <= 0)
            return;
        foreach (var skinPair in m_SkinPairs)
        {
            var sharedMaterials = new Material[skinPair.materialIndices.Count];
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                sharedMaterials[i] = materials[skinPair.materialIndices[i]];
            }
            skinPair.renderers.ForEach(meshRenderer => meshRenderer.sharedMaterials = sharedMaterials);
        }
    }

    public void EquipSkin(SkinSO skin)
    {
        if (skin == null)
            return;
        m_CurrentSkin = skin;
        m_CurrentSkin.GetOrLoadSkinResources(OnLoadCompleted);

        void OnLoadCompleted(Material[] materials)
        {
            if (this == null)
                return;
            if (m_CurrentSkin != skin)
                return;
            UpdateSkin(materials);
        }
    }

    public void EquipCurrentSkin(PBPartSO partSO)
    {
        if (partSO == null || !partSO.TryGetModule(out AbstractSkinItemModule skinModule))
            return;
        EquipSkin(skinModule.currentSkin);
    }

    [Serializable]
    public class SkinPair
    {
        [SerializeField]
        private List<Renderer> m_MeshRenderers = new List<Renderer>();
        [SerializeField]
        private List<int> m_MaterialIndices = new List<int>();

        public List<Renderer> renderers => m_MeshRenderers;
        public List<int> materialIndices => m_MaterialIndices;
    }
}