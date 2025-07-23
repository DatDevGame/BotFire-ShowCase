using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

public class CombatEffectController : MonoBehaviour
{
    public event Action<CombatEffect> onEffectApplied = delegate { };
    public event Action<CombatEffect> onEffectStacked = delegate { };
    public event Action<CombatEffect> onEffectRemoved = delegate { };

    [SerializeField]
    private List<CombatEffectConfigSO> m_CombatEffectConfigSOs;

    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private ICombatEntity m_AffectedEntity;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private List<CombatEffect> m_ActiveEffects = new List<CombatEffect>();

    public CombatEffectStatuses combatEffectStatuses
    {
        get
        {
            CombatEffectStatuses combatEffectStatuses = CombatEffectStatuses.None;
            List<CombatEffect> currentActiveEffects = activeEffects;
            foreach (var currentActiveEffect in currentActiveEffects)
            {
                combatEffectStatuses |= currentActiveEffect.effectStatus;
            }
            return combatEffectStatuses;
        }
    }
    public ICombatEntity affectedEntity
    {
        get
        {
            if (m_AffectedEntity == null)
            {
                m_AffectedEntity = GetComponent<ICombatEntity>();
            }
            return m_AffectedEntity;
        }
    }
    public List<CombatEffectConfigSO> combatEffectConfigSOs => m_CombatEffectConfigSOs;
    public List<CombatEffect> activeEffects => m_ActiveEffects;

    private void Awake()
    {
        m_AffectedEntity = GetComponent<ICombatEntity>();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            CombatEffect combatEffect = activeEffects[i];
            combatEffect.Update(deltaTime);
            if (combatEffect.remainingDuration <= 0)
            {
                RemoveEffect(combatEffect);
            }
        }
        Test();
    }

    public void ApplyEffect<T>(CombatEffect<T> effect) where T : CombatEffectConfigSO
    {
        ApplyEffect(effect as CombatEffect);
    }

    public void ApplyEffect(CombatEffect effect)
    {
        if (effect == null)
            return;
        // Check if an effect is blocking by current active effects
        if ((effect.blockingEffectStatuses & combatEffectStatuses) != 0)
        {
            Log($"{affectedEntity.name} with status [{combatEffectStatuses}] and cannot receive effect: {effect.name}");
            return;
        }

        effect.onEffectApplied += onEffectApplied;
        effect.onEffectRemoved += onEffectRemoved;
        effect.onEffectStacked += onEffectStacked;
        effect.Initialize(this, m_CombatEffectConfigSOs.FirstOrDefault(c => c.effectStatus == effect.effectStatus));
        // Check if an effect of the same type is already active and can stack
        CombatEffect existingEffect = activeEffects.FirstOrDefault(e => e.GetType() == effect.GetType());
        if (existingEffect != null && existingEffect.canStack)
        {
            existingEffect.Stack(effect);
        }
        else
        {
            effect.Apply();
            activeEffects.Add(effect);
        }
    }

    public void RemoveEffect(CombatEffect effectToRemove)
    {
        if (effectToRemove == null)
            return;
        effectToRemove.Remove();
        effectToRemove.onEffectApplied -= onEffectApplied;
        effectToRemove.onEffectRemoved -= onEffectRemoved;
        effectToRemove.onEffectStacked -= onEffectStacked;
        activeEffects.Remove(effectToRemove);
    }

    public void RemoveEffectByType<T>() where T : CombatEffect
    {
        CombatEffect effect = activeEffects.OfType<T>().FirstOrDefault();
        RemoveEffect(effect);
    }

    public T GetEffect<T>() where T : CombatEffect
    {
        return activeEffects.OfType<T>().FirstOrDefault();
    }

    public List<T> GetAllEffectsOfType<T>() where T : CombatEffect
    {
        return activeEffects.OfType<T>().ToList();
    }

    public void ClearAllEffects(Predicate<CombatEffect> predicate = null)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (predicate?.Invoke(activeEffects[i]) ?? true)
                RemoveEffect(activeEffects[i]);
        }
        activeEffects.Clear();
    }

    public void Log(string message, string tag = "CombatEffectController", Object context = null)
    {
        LGDebug.Log($"{message} - [Time: {Time.time}]", tag, context ?? this);
    }

    private void Test()
    {
        CombatEffect[] combatEffects = new CombatEffect[]
        {
            new AirborneEffect(1f, true, Vector3.up * 30f, ForceMode.VelocityChange, Vector3.right * 50f, ForceMode.VelocityChange, false, null),
            new DisarmEffect(3f, false, null),
            new InvincibleEffect(3f, false, null),
            new SlowEffect(3f, 0.65f, 0.95f, false, null),
            new StunEffect(3f, false, null)
        };
        for (int i = 0; i < combatEffects.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha5 + i))
            {
                ApplyEffect(combatEffects[i]);
            }
        }
    }
}