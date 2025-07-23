using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionBehaviorBase : StateMachineBehaviour
{
    public CharacterEmotions CharacterEmotions;
    public Action<EmotionBehaviorInfo> OnStartEmotions;
    public Action<EmotionBehaviorInfo> OnExitEmotions;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (CharacterEmotions == CharacterEmotions.None) return;
        EmotionBehaviorInfo emotionBehaviorInfo = new EmotionBehaviorInfo(CharacterEmotions, stateInfo);
        OnStartEmotions?.Invoke(emotionBehaviorInfo);
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (CharacterEmotions == CharacterEmotions.None) return;
        EmotionBehaviorInfo emotionBehaviorInfo = new EmotionBehaviorInfo(CharacterEmotions, stateInfo);
        OnExitEmotions?.Invoke(emotionBehaviorInfo);
    }
}

public class EmotionBehaviorInfo
{
    public EmotionBehaviorInfo(CharacterEmotions characterEmotions, AnimatorStateInfo animatorStateInfo)
    {
        CharacterEmotions = characterEmotions;
        AnimatorStateInfo = animatorStateInfo;
    }
    public CharacterEmotions CharacterEmotions;
    public AnimatorStateInfo AnimatorStateInfo;
}
