using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "PocketBots/Characters/CharacterSO")]
public class CharacterSO : ItemSO
{
    [ReadOnly]
    public string m_Key;

    [BoxGroup("Config")] public bool IsFakeLock;
    [BoxGroup("Emotion")] public SerializedDictionary<CharacterEmotions, Sprite> EmojiSpites;

#if UNITY_EDITOR
    [OnInspectorGUI]
    protected virtual void OnInspectorGUI()
    {
        GenerateSaveKey();
    }
    protected virtual void GenerateSaveKey()
    {
        if (string.IsNullOrEmpty(m_Key) && !string.IsNullOrEmpty(name))
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
            m_Key = $"{name}_{guid}";
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
