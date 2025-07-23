using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "CharacterAnimationKeySO", menuName = "PocketBots/Characters/CharacterAnimationKeySO")]
public class CharacterAnimationKeySO : SerializableScriptableObject
{
    public Dictionary<CharacterState, string> CharacterAnimationKey => m_CharacterAnimationKey;
    public Dictionary<CharacterEmotions, int> EmotionsBlendShapesIndex => m_EmotionsBlendShapesIndex;

    [SerializeField] protected SerializedDictionary<CharacterState, string> m_CharacterAnimationKey;
    [SerializeField] protected SerializedDictionary<CharacterEmotions, int> m_EmotionsBlendShapesIndex;
}
