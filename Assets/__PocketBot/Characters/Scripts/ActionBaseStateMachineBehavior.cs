using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBaseStateMachineBehavior : StateMachineBehaviour
{
    public CharacterState CharacterState;
    [ShowInInspector] public Action<ActionBehaviorInfo> OnStartStateAnimation;
    [ShowInInspector] public Action<ActionBehaviorInfo> OnEndStateAnimation;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ActionBehaviorInfo actionBehaviorInfo = new ActionBehaviorInfo(CharacterState, stateInfo);
        OnStartStateAnimation?.Invoke(actionBehaviorInfo);
    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ActionBehaviorInfo emotionBehaviorInfo = new ActionBehaviorInfo(CharacterState, stateInfo);
        OnEndStateAnimation?.Invoke(emotionBehaviorInfo);
    }
}

public class ActionBehaviorInfo
{
    public ActionBehaviorInfo(CharacterState characterState, AnimatorStateInfo animatorStateInfo)
    {
        CharacterState = characterState;
        AnimatorStateInfo = animatorStateInfo;
    }
    public CharacterState CharacterState;
    public AnimatorStateInfo AnimatorStateInfo;
}
