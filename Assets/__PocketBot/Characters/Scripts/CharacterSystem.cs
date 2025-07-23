using DG.Tweening;
using LatteGames;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum CharacterState
{
    Idle,
    Control,
    ReadyFight,
    Panic,
    Excited,
    Angry,
    Victory,
    Defeat
}
public enum CharacterEmotions
{
    None,
    Anger,
    Sadness,
    Happiness,
    Surprise,
    Blink,
}
public class CharacterSystem : MonoBehaviour
{
    [ShowInInspector, BoxGroup("Preview"), PropertyOrder(99)]
    public CharacterState CurrentState
    {
        get => m_CurrentState;
        set
        {
            if (m_CurrentState == value)
                return;

            m_OnChangeState?.Invoke(value);
            m_CurrentState = value;
            AnimationHandle(value);
        }
    }
    public CharacterEmotions CurrentEmotionState
    {
        get => m_CurrentEmotionState;
        set
        {
            if (m_SkinnedMeshRenderer == null) return;
            m_CurrentEmotionState = value;
            if (m_SkinnedMeshRenderer != null)
                SetEmotion(m_CurrentEmotionState);
        }
    }
    public bool IsPlayer
    {
        get => m_IsPlayer;
        set
        {
            m_IsPlayer = value;
        }
    }
    public Transform PlayerHeadPointCamera => m_PlayerHeadPointCamera;
    public Transform OpponentHeadPointCamera => m_OpponentHeadPointCamera;
    public Transform HeadCharacter => m_HeadCharacter;
    public Transform Controller => m_Controller;
    public Animator Animator => m_Animator;

    [SerializeField, BoxGroup("Config")] protected float m_ScaleController;

    [SerializeField, BoxGroup("Ref")] protected Transform m_PlayerHeadPointCamera;
    [SerializeField, BoxGroup("Ref")] protected Transform m_OpponentHeadPointCamera;
    [SerializeField, BoxGroup("Ref")] protected Transform m_HeadCharacter;
    [SerializeField, BoxGroup("Ref")] protected Transform m_Controller;
    [SerializeField, BoxGroup("Ref")] protected SkinnedMeshRenderer m_SkinnedMeshRenderer;
    [SerializeField, BoxGroup("Ref")] protected Animator m_Animator;

    [SerializeField, BoxGroup("Data")] protected CharacterAnimationKeySO m_AnimationKeySO;
    [SerializeField, BoxGroup("Data")] protected CharacterEmotionConfigs m_CharacterEmotionConfigs;

    protected bool m_IsPlayer;
    private bool m_IsInPVP;
    protected Sequence m_EmotionSequence;
    protected IEnumerator m_BlinkBlendShapeCR;
    protected CharacterState m_CurrentState;
    protected CharacterEmotions m_CurrentEmotionState;
    protected Action<CharacterState> m_OnChangeState;
    protected List<ActionBaseStateMachineBehavior> m_ActionBaseStateMachineBehaviors;
    protected List<EmotionBehaviorBase> m_EmotionBehaviors;

    private void OnEnable()
    {
        if (m_BlinkBlendShapeCR != null)
            StopCoroutine(m_BlinkBlendShapeCR);
        m_BlinkBlendShapeCR = LoopBlendShapeBlinkAnimation();
        StartCoroutine(m_BlinkBlendShapeCR);

        OnInit();
    }
    private void OnDisable()
    {
        if (m_BlinkBlendShapeCR != null)
            StopCoroutine(m_BlinkBlendShapeCR);
    }

    public void SetActivePVP(bool isInPVP) => m_IsInPVP = isInPVP;

    //Set Up In Animator
    public void EnableController()
    {
        if (m_Controller.localScale != Vector3.one * m_ScaleController)
        {
            if (m_Controller != null)
            {
                m_Controller.gameObject.SetLayer(0);
                m_Controller.DOScale(Vector3.one * m_ScaleController, AnimationDuration.SSHORT).SetEase(Ease.InOutBack);
            }

        }
        else
        {
            if (m_Controller != null)
            {
                m_Controller.gameObject.SetLayer(0);
                m_Controller.DOScale(Vector3.one * m_ScaleController, 0);
            }

        }
    }

    public void DisableController()
    {
        DisableController(false);
    }

    //Set Up In Animator
    public void DisableController(bool isNow = false)
    {
        if (m_Controller != null)
            m_Controller.DOScale(Vector3.zero, isNow ? 0 : AnimationDuration.SSHORT).SetEase(Ease.InBack);
    }
    protected void OnInit()
    {
        OnRemoveEventAnimation();
        //State Behavior
        m_ActionBaseStateMachineBehaviors = new List<ActionBaseStateMachineBehavior>();
        ActionBaseStateMachineBehavior[] ActionBaseStateMachineBehaviors = m_Animator.GetBehaviours<ActionBaseStateMachineBehavior>();
        ActionBaseStateMachineBehaviors.ForEach((actionStateMachineBehavior) =>
        {
            if (actionStateMachineBehavior != null)
            {
                m_ActionBaseStateMachineBehaviors.Add(actionStateMachineBehavior);
                actionStateMachineBehavior.OnStartStateAnimation += StartStateAnimation;
                actionStateMachineBehavior.OnEndStateAnimation += EndStateAnimation;
            }
        });

        //Emotion Behaviors
        m_EmotionBehaviors = new List<EmotionBehaviorBase>();
        EmotionBehaviorBase[] emotionBehaviorBases = m_Animator.GetBehaviours<EmotionBehaviorBase>();
        emotionBehaviorBases.ForEach((emotionStateMachineBehavior) =>
        {
            if (emotionStateMachineBehavior != null)
            {
                m_EmotionBehaviors.Add(emotionStateMachineBehavior);
                emotionStateMachineBehavior.OnStartEmotions += StartEmotion;
                emotionStateMachineBehavior.OnExitEmotions += EndEmotion;
            }
        });
    }

    private void OnRemoveEventAnimation()
    {
        if (m_ActionBaseStateMachineBehaviors != null && m_ActionBaseStateMachineBehaviors.Count > 0)
        {
            m_ActionBaseStateMachineBehaviors
                .Where(x => x != null)
                .ForEach((v) =>
                {
                    v.OnStartStateAnimation -= StartStateAnimation;
                    v.OnEndStateAnimation -= EndStateAnimation;
                });
        }

        if (m_EmotionBehaviors != null && m_EmotionBehaviors.Count > 0)
        {
            m_EmotionBehaviors
                .Where(x => x != null)
                .ForEach((v) =>
                {
                    v.OnStartEmotions -= StartEmotion;
                    v.OnExitEmotions -= EndEmotion;
                });
        }
    }
    private IEnumerator LoopBlendShapeBlinkAnimation()
    {
        while (true)
        {
            if (m_SkinnedMeshRenderer == null)
                break;

            float randomDuration = UnityEngine.Random.Range(1f, 2.5f);
            float animationDuration = randomDuration / 2;

            yield return DOVirtual.Float(0, 100, 0.3f, (value) =>
            {
                m_SkinnedMeshRenderer.SetBlendShapeWeight(m_AnimationKeySO.EmotionsBlendShapesIndex[CharacterEmotions.Blink], value);
            }).SetEase(Ease.InOutSine).WaitForCompletion();

            yield return DOVirtual.Float(100, 0, 0.2f, (value) =>
            {
                m_SkinnedMeshRenderer.SetBlendShapeWeight(m_AnimationKeySO.EmotionsBlendShapesIndex[CharacterEmotions.Blink], value);
            }).SetEase(Ease.InOutSine).WaitForCompletion();

            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 5f));
        }
    }

    protected void AnimationHandle(CharacterState characterState)
    {
        if (m_Animator == null)
        {
            Debug.LogError($"{this.name} Animator Is Null");
            return;
        }
        if (!m_AnimationKeySO.CharacterAnimationKey.ContainsKey(characterState))
        {
            Debug.LogError($"AnimationKey Is Null -> {characterState}");
            return;
        }

        string animationKey = m_AnimationKeySO.CharacterAnimationKey[characterState];
        m_Animator.SetTrigger(animationKey);

        if (characterState == CharacterState.Victory)
        {
            SetEmotion(CharacterEmotions.Surprise);
            DisableController(true);
        }
        else if (characterState == CharacterState.Defeat)
        {
            SetEmotion(CharacterEmotions.Sadness);
            DisableController(true);
        }

    }

    protected void SetEmotion(CharacterEmotions characterEmotions)
    {
        if (m_SkinnedMeshRenderer == null)
            return;

        if (m_EmotionSequence != null)
            m_EmotionSequence.Kill();

        m_EmotionSequence = DOTween.Sequence();
        bool hasActiveEmotion = false;
        float timeSetDefault = characterEmotions == CharacterEmotions.None ? m_CharacterEmotionConfigs.EmotionDuration : 0.1f;
        for (int i = 0; i < m_AnimationKeySO.EmotionsBlendShapesIndex.Count; i++)
        {
            float emotionValue = m_SkinnedMeshRenderer.GetBlendShapeWeight(m_AnimationKeySO.EmotionsBlendShapesIndex.ElementAt(i).Value);
            if (emotionValue != 0 && m_AnimationKeySO.EmotionsBlendShapesIndex.ElementAt(i).Key != CharacterEmotions.Blink)
            {
                hasActiveEmotion = true;
                int blendShapeIndex = m_AnimationKeySO.EmotionsBlendShapesIndex.ElementAt(i).Value;
                m_EmotionSequence.Append(DOVirtual.Float(emotionValue, 0, timeSetDefault, (value) =>
                {
                    m_SkinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, value);
                }));
            }
        }
        if (characterEmotions == CharacterEmotions.None)
            return;

        m_EmotionSequence.OnComplete(() =>
        {
            RunNewEmotion();
        });

        if (!hasActiveEmotion)
        {
            RunNewEmotion();
        }

        void RunNewEmotion()
        {
            int blendShapeIndex = m_AnimationKeySO.EmotionsBlendShapesIndex[characterEmotions];
            DOVirtual.Float(0, 100, m_CharacterEmotionConfigs.EmotionDuration, (value) =>
            {
                m_SkinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, value);
            });
        }
    }


    //Setup In Animator Behavior
    protected void StartStateAnimation(ActionBehaviorInfo actionBehaviorInfo)
    {
        if (actionBehaviorInfo == null) return;
    }
    protected void EndStateAnimation(ActionBehaviorInfo actionBehaviorInfo)
    {
        if (actionBehaviorInfo == null) return;

        if (CurrentState != CharacterState.Idle || CurrentState != CharacterState.Control)
        {
            if (m_IsInPVP)
                CurrentState = CharacterState.Control;
            else
                CurrentState = CharacterState.Idle;
        }
    }

    //Setup In Animator Behavior
    protected void StartEmotion(EmotionBehaviorInfo emotionBehaviorInfo)
    {
        if (emotionBehaviorInfo == null) return;
    }

    //Setup In Animator Behavior
    protected void EndEmotion(EmotionBehaviorInfo emotionBehaviorInfo)
    {
        if (emotionBehaviorInfo == null) return;
    }
}
