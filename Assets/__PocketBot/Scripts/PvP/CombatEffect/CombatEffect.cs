using System;
using System.Collections;
using UnityEngine;

public abstract class CombatEffect
{
    #region Constructors
    public CombatEffect(float duration, bool isAffectedByAbility, ICombatEntity sourceEntity, bool canStack = true)
    {
        this.duration = duration;
        this.isAffectedByAbility = isAffectedByAbility;
        this.canStack = canStack;
        this.sourceEntity = sourceEntity;
        remainingDuration = duration;
    }
    #endregion

    public event Action<CombatEffect> onEffectApplied = delegate { };
    public event Action<CombatEffect> onEffectRemoved = delegate { };
    public event Action<CombatEffect> onEffectStacked = delegate { };
    public event Action<float> onDurationUpdated = delegate { };

    public virtual string name => GetType().Name;
    public virtual bool isActive { get; protected set; }
    public virtual float duration { get; protected set; }
    public virtual float remainingDuration { get; protected set; }
    public virtual bool canStack { get; protected set; }
    public virtual bool isAffectedByAbility { get; protected set; }
    public abstract CombatEffectStatuses effectStatus { get; }
    public abstract CombatEffectStatuses blockingEffectStatuses { get; }
    public virtual ICombatEntity affectedEntity => controller.affectedEntity;
    public virtual ICombatEntity sourceEntity { get; protected set; }
    public virtual CombatEffectController controller { get; protected set; }

    protected Coroutine StartCoroutine(IEnumerator routine)
    {
        return controller.StartCoroutine(routine);
    }

    protected void StopCoroutine(IEnumerator routine)
    {
        controller.StopCoroutine(routine);
    }

    protected void StopCoroutine(Coroutine routine)
    {
        controller.StopCoroutine(routine);
    }

    public virtual void Apply()
    {
        isActive = true;
        remainingDuration = duration;
        onEffectApplied.Invoke(this);
        controller.Log($"{controller.affectedEntity.name} applied with effect ({remainingDuration}): {name}");
    }

    public virtual void Update(float deltaTime)
    {
        remainingDuration = Mathf.Max(0f, remainingDuration - deltaTime);
        onDurationUpdated.Invoke(remainingDuration);
    }

    public virtual void Remove()
    {
        isActive = false;
        onEffectRemoved.Invoke(this);
        controller.Log($"{controller.affectedEntity.name} effect removed ({remainingDuration}): {name}");
    }

    public virtual void Stack(CombatEffect otherEffect)
    {
        remainingDuration = Mathf.Max(remainingDuration, otherEffect.duration);
        sourceEntity = otherEffect.sourceEntity;
        onEffectStacked.Invoke(this);
        controller.Log($"{controller.affectedEntity.name} effect stacked: {name}, new duration: {remainingDuration}");
    }

    public virtual void Initialize(CombatEffectController controller, CombatEffectConfigSO configSO)
    {
        this.controller = controller;
    }
}
public abstract class CombatEffect<T> : CombatEffect where T : CombatEffectConfigSO
{
    protected CombatEffect(float duration, bool isAffectedByAbility, ICombatEntity sourceEntity, bool canStack = true) : base(duration, isAffectedByAbility, sourceEntity, canStack)
    {
    }

    public virtual T configSO { get; protected set; }

    public override void Initialize(CombatEffectController controller, CombatEffectConfigSO configSO)
    {
        this.controller = controller;
        this.configSO = configSO as T;
    }
}