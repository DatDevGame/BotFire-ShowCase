using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LatteGames.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public abstract class AIBotState : StateMachine.State
{
    protected AIBotState() : base(null)
    {
    }

    [SerializeField, ReadOnly]
    protected string stateId;
    [SerializeReference, PropertyOrder(10)]
    protected List<AIBotStateTransition> transitions;

    public string StateId => stateId;
    public AIBotController BotController { get; protected set; }

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(stateId))
            stateId = Guid.NewGuid().ToString();
    }

    protected virtual void OnDrawGizmos()
    {

    }

    protected virtual void OnDrawGizmosSelected()
    {

    }

    protected virtual void OnStateEnable()
    {

    }

    protected virtual void OnStateDisable()
    {

    }

    protected virtual void OnStateUpdate()
    {

    }

    [System.Diagnostics.Conditional(LGDebug.k_UnityEditorDefineSymbol)]
    protected virtual void Log(string message, Object context = null)
    {
        BotController.Log(message, nameof(AIBotState), context);
    }

    protected Coroutine StartCoroutine(IEnumerator routine)
    {
        return BotController.StartCoroutine(routine);
    }

    protected void StopCoroutine(IEnumerator routine)
    {
        BotController.StopCoroutine(routine);
    }

    protected void StopCoroutine(Coroutine routine)
    {
        BotController.StopCoroutine(routine);
    }

    public virtual void InitializeState(AIBotController botController)
    {
        foreach (var transition in transitions)
        {
            transition.InitializeTransition(this, botController);
        }
        BotController = botController;
        Transitions = transitions.Select(transition => transition.GetTransition()).ToList();
        Behaviour = new StateMachine.ActionBehaviour(OnStateEnable, OnStateDisable, OnStateUpdate);
    }
}