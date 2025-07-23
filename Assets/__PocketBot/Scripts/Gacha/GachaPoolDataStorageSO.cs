using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class GachaPoolSavedData : SavedData
{
    public List<string> priorityPartIdQueue = new List<string>();
    [NonSerialized, ShowInInspector, CustomValueDrawer("DrawElement")]
    public List<PBPartSO> priorityPartQueue = new List<PBPartSO>();

#if UNITY_EDITOR
    private PBPartSO DrawElement(PBPartSO partSO)
    {
        GUI.color = partSO.IsAvailable() ? Color.green : Color.red;
        partSO = EditorGUILayout.ObjectField(partSO, typeof(PBPartSO), false) as PBPartSO;
        GUI.color = Color.white; // Reset to default color
        return partSO;
    }
#endif
}
[CreateAssetMenu(fileName = "GachaPoolDataStorageSO", menuName = "PocketBots/Gacha/GachaPoolDataStorageSO")]
public class GachaPoolDataStorageSO : SavedDataSO<GachaPoolSavedData>
{
    [SerializeField]
    private PBPartManagerSO[] partManagerSOs;

    [NonSerialized]
    private Dictionary<string, PBPartSO> idToPartDictionary;

#if UNITY_EDITOR
    [ShowInInspector, HideInEditorMode, FoldoutGroup("Infos")]
    private GachaPoolSavedData PreviewData => data;
    [ShowInInspector, HideInEditorMode, FoldoutGroup("Infos")]
    public List<GachaPoolManager.DebugPart> AvailableParts => GachaPoolManager.AvailableParts;
    [ShowInInspector, HideInEditorMode, FoldoutGroup("Infos")]
    public List<GachaPoolManager.DebugPart> NotAvailableParts => GachaPoolManager.NotAvailableParts;
#endif

    private void OnEnable()
    {
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUnlocked, OnPartUnlocked);
    }

    private void OnDisable()
    {
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUnlocked, OnPartUnlocked);
    }

    private PBPartSO FindPartById(string id)
    {
        if (idToPartDictionary == null)
        {
            idToPartDictionary = new Dictionary<string, PBPartSO>();
            foreach (var partManagerSO in partManagerSOs)
            {
                foreach (PBPartSO partSO in partManagerSO.Cast<PBPartSO>())
                {
                    idToPartDictionary.Add(partSO.guid, partSO);
                }
            }
        }
        return idToPartDictionary.Get(id);
    }

    private void OnPartUnlocked(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var unlockedPartSO = parameters[1] as PBPartSO;
        data.priorityPartQueue.RemoveAll(partSO => partSO == unlockedPartSO);
        data.priorityPartIdQueue.RemoveAll(partId => partId == unlockedPartSO.guid);
    }

    public override void Load()
    {
        base.Load();

        data.priorityPartIdQueue.RemoveAll(id => FindPartById(id) == null);
        for (int i = 0; i < data.priorityPartIdQueue.Count; i++)
        {
            var partSO = FindPartById(data.priorityPartIdQueue[i]);
            data.priorityPartQueue.Add(partSO);
        }
    }

    public void EnqueuePriorityPart(PBPartSO partSO)
    {
        if (data.priorityPartIdQueue.Contains(partSO.guid))
            return;
        data.priorityPartQueue.Add(partSO);
        data.priorityPartIdQueue.Add(partSO.guid);
    }

    public PBPartSO DequeueAvailablePriorityPart()
    {
        var partSO = data.priorityPartQueue.FirstOrDefault(partSO => partSO.IsAvailable());
        if (partSO != null)
        {
            data.priorityPartQueue.Remove(partSO);
            data.priorityPartIdQueue.Remove(partSO.guid);
        }
        return partSO;
    }

    public List<PBPartSO> GetAvailablePriorityPartQueue()
    {
        var availableParts = data.priorityPartQueue.Where(partSO => partSO.IsAvailable()).ToList();
        return availableParts;
    }
}