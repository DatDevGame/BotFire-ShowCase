using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterEmotionConfigs", menuName = "PocketBots/Characters/CharacterEmotionConfigs")]
public class CharacterEmotionConfigs : SerializableScriptableObject
{
    [BoxGroup("Emotion")] public float CoolDownNextEmotion;
    [BoxGroup("Emotion")] public float EmotionDuration;
    [BoxGroup("Emotion")] public List<CharacterEmotions> DamagedRobotEmotions;
    [BoxGroup("Emotion")] public List<CharacterEmotions> AttackerRobotEmotions;

    [BoxGroup("Damage Tracker")] public float DamageTrackerTime;
    [BoxGroup("Damage Tracker")] public float DamagePercentage;
}
