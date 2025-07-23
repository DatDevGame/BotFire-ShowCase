using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.Helpers;
using UnityEditor;

[CreateAssetMenu(fileName = "PBPartSOImporter", menuName = "PocketBots/PartManagement/PBPartSOImporter")]
public class PBPartSOImporter : SerializableScriptableObject
{
#if UNITY_EDITOR
    [SerializeField] List<string> arenaFoundInData;
    [SerializeField] List<PBPartManagerSO> managers;

    [Button]
    void Import()
    {
        for (var i = 0; i < arenaFoundInData.Count; i++)
        {
            var profile_data = SplitCSV(arenaFoundInData[i]);
            for (var j = 0; j < profile_data.Count; j++)
            {
                var data_row = profile_data[j];
                Debug.Log(data_row[0] + "-" + i + 1);
                foreach (var manager in managers)
                {
                    foreach (var item in manager.value)
                    {
                        if (item.name == data_row[0])
                        {
                            Debug.Log(item.name + "-" + i + 1);
                            ((GachaItemSO)item).SetFieldValue("m_FoundInArena", i + 1);
                            EditorUtility.SetDirty(item);
                            goto find_another;
                        }
                    }
                }
            find_another:;
            }
        }
    }

    List<List<string>> SplitCSV(string csvData)
    {
        List<List<string>> data = new List<List<string>>();
        // Split data by newlines
        string[] lines = csvData.Split(' ');

        foreach (string line in lines)
        {
            // Split line by commas (assuming CSV delimiter is comma)
            string[] row = line.Split(',');
            List<string> rowList = new List<string>(row);
            data.Add(rowList);
        }
        return data;
    }
#endif
}
