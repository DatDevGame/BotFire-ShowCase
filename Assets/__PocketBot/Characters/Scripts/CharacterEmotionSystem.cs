using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static BattleRoyaleLeaderboardRow;

public class CharacterEmotionSystem : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private RawImage m_PlayerScreen;
    [SerializeField, BoxGroup("Ref")] private RawImage m_OpponentScreen;
    [SerializeField, BoxGroup("Ref")] private RectTransform m_PlayerRectEmoji;
    [SerializeField, BoxGroup("Ref")] private Image m_PlayerImageEmoji;
    [SerializeField, BoxGroup("Ref")] private RectTransform m_OpponentRectEmoji;
    [SerializeField, BoxGroup("Ref")] private Image m_OpponentImageEmoji;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_EmotionScreenCanvasGroup;

    [SerializeField, BoxGroup("Data")] private CharacterEmotionConfigs m_CharacterEmotionConfigs;
#if UNITY_EDITOR
    [SerializeField, BoxGroup("Editor")] private TestMatchMakingSO m_TestMatchMakingSO;
#endif

    private CharacterInfo m_DamagedInfo;
    private CharacterInfo m_AttackerInfo;

    private IEnumerator m_EmotionCharacterCR;
    private CharacterRoomPVP m_CharacterRoomPVP;
    private bool m_IsPlayingEmotion;

    private class DamageTracker
    {
        public float TotalDamage = 0;
        public float StartTime = 0;
    }

    private Dictionary<PBRobot, DamageTracker> m_DamageTrackers = new Dictionary<PBRobot, DamageTracker>();

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        //GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotDamaged, OnRobotDamaged);
    }

    private void OnDestroy()
    {
        //TODO: Hide IAP & Popup
        //GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotDamaged, OnRobotDamaged);
    }

    private void OnRobotDamaged(object[] parameters)
    {
        if (parameters == null || parameters.Length < 2)
            return;

        if (m_CharacterRoomPVP == null)
            m_CharacterRoomPVP = ObjectFindCache<CharacterRoomPVP>.Get();

        PBRobot damagedRobot = (PBRobot)parameters[0];
        float damage = (float)parameters[1];
        float currentTime = Time.time;
        IAttackable IAttacker = (IAttackable)parameters[3];
        if (IAttacker == null)
            return;
        if (IAttacker is not PBPart attackerPart)
            return;
        if (attackerPart == null || attackerPart.RobotChassis == null || attackerPart.RobotChassis.Robot == null)
            return;

        PBRobot attackerRobot = attackerPart.RobotChassis.Robot;
        if (!m_DamageTrackers.ContainsKey(damagedRobot))
            m_DamageTrackers[damagedRobot] = new DamageTracker { TotalDamage = 0, StartTime = currentTime };
        DamageTracker tracker = m_DamageTrackers[damagedRobot];

        if (currentTime - tracker.StartTime > m_CharacterEmotionConfigs.DamageTrackerTime)
        {
            tracker.TotalDamage = 0;
            tracker.StartTime = currentTime;
        }
        tracker.TotalDamage += damage;

        float damagePercentage = (tracker.TotalDamage / damagedRobot.MaxHealth) * 100;
        if (damagePercentage >= m_CharacterEmotionConfigs.DamagePercentage)
        {
            if (!m_IsPlayingEmotion)
            {
                if (m_EmotionCharacterCR != null)
                    StopCoroutine(m_EmotionCharacterCR);
                m_EmotionCharacterCR = EmotionCharacterScreens(damagedRobot, attackerRobot);
                StartCoroutine(m_EmotionCharacterCR);
            }

            tracker.TotalDamage = 0;
            tracker.StartTime = currentTime;
        }
    }

    private IEnumerator EmotionCharacterScreens(PBRobot damagedRobot, PBRobot attackerRobot)
    {
        if (!IsAnyRobotLocal(damagedRobot, attackerRobot))
            yield break;

        m_IsPlayingEmotion = true;
        var damagedInfo = GetCharacterInfo(damagedRobot);
        var attackerInfo = GetCharacterInfo(attackerRobot);
        m_DamagedInfo = damagedInfo;
        m_AttackerInfo = attackerInfo;
        ActivateScreens(true);
        if (damagedInfo == null || attackerInfo == null)
            m_OpponentScreen.transform.parent.gameObject.SetActive(false);

        //Set Default
        if(damagedInfo != null && damagedInfo.CharacterSystem != null)
            damagedInfo.CharacterSystem.CurrentState = CharacterState.Control;
        if (attackerInfo != null && attackerInfo.CharacterSystem != null)
            attackerInfo.CharacterSystem.CurrentState = CharacterState.Control;

#if UNITY_EDITOR
        if (m_TestMatchMakingSO.IsTest)
        {
            if (damagedRobot.RobotLayer == 31)
            {
                damagedInfo = m_CharacterRoomPVP.PlayerCharacterInfo;
                attackerInfo = m_CharacterRoomPVP.OpponentCharacterInfos[0];
            }
            else
            {
                attackerInfo = m_CharacterRoomPVP.PlayerCharacterInfo;
                damagedInfo = m_CharacterRoomPVP.OpponentCharacterInfos[0];
            }

            if (damagedInfo != null)
            {
                damagedInfo.RenderCamera.gameObject.SetActive(true);
                damagedInfo.RenderCamera.transform.parent.gameObject.SetActive(true);
            }

            if (attackerInfo != null)
            {
                attackerInfo.RenderCamera.gameObject.SetActive(true);
                attackerInfo.RenderCamera.transform.parent.gameObject.SetActive(true);
            }
            m_OpponentScreen.transform.parent.gameObject.SetActive(true);

            //Set Default
            if (damagedInfo != null && damagedInfo.CharacterSystem != null)
                damagedInfo.CharacterSystem.CurrentState = CharacterState.Control;
            if (attackerInfo != null && attackerInfo.CharacterSystem != null)
                attackerInfo.CharacterSystem.CurrentState = CharacterState.Control;
        }
#endif

        bool isPlayerDamaged = damagedRobot.PersonalInfo.isLocal;
        UpdateScreen(damagedInfo, attackerInfo, isPlayerDamaged);
        yield return new WaitForSeconds(1f);
        ResetEmotionStates(damagedInfo, attackerInfo);
        yield return new WaitForSeconds(0.5f);
        ActivateScreens(false);

        yield return new WaitForSeconds(m_CharacterEmotionConfigs.CoolDownNextEmotion);
        m_IsPlayingEmotion = false;
    }

    private bool IsAnyRobotLocal(PBRobot damagedRobot, PBRobot attackerRobot)
    {
        return damagedRobot.PersonalInfo.isLocal || attackerRobot.PersonalInfo.isLocal;
    }

    private CharacterInfo GetCharacterInfo(PBRobot robot)
    {
        if (m_CharacterRoomPVP.OpponentCharacterInfoDictionary.TryGetValue(robot.PlayerInfoVariable, out var characterInfo))
            return characterInfo;

        return null;
    }

    private void ActivateScreens(bool isActive)
    {
        if (isActive)
            m_EmotionScreenCanvasGroup.Show();
        else
            m_EmotionScreenCanvasGroup.Hide();

        if(m_DamagedInfo != null)
            m_DamagedInfo.RenderCamera.gameObject.SetActive(isActive);
        if (m_AttackerInfo != null)
            m_AttackerInfo.RenderCamera.gameObject.SetActive(isActive);
    }

    private void ResetEmotionStates(CharacterInfo damagedInfo, CharacterInfo attackerInfo)
    {
        if (damagedInfo != null)
        {
            damagedInfo.CharacterSystem.CurrentEmotionState = CharacterEmotions.None;
            //damagedInfo.CharacterSystem.CurrentState = CharacterState.Idle;
            //damagedInfo.CharacterSystem.CurrentState = CharacterState.Control;
        }

        if (attackerInfo != null)
        {
            attackerInfo.CharacterSystem.CurrentEmotionState = CharacterEmotions.None;
            //attackerInfo.CharacterSystem.CurrentState = CharacterState.Idle;
            //attackerInfo.CharacterSystem.CurrentState = CharacterState.Control;
        }
        m_PlayerRectEmoji.DOScale(Vector3.zero, AnimationDuration.SSHORT).SetEase(Ease.InBack);
        m_OpponentRectEmoji.DOScale(Vector3.zero, AnimationDuration.SSHORT).SetEase(Ease.InBack);
        //m_CharacterRoomPVP.OpponentCharacterInfoDictionary.ForEach(v => v.Value.CharacterSystem.CurrentEmotionState = CharacterEmotions.None);
    }

    private void UpdateScreen(CharacterInfo damagedInfo, CharacterInfo attackerInfo, bool isPlayerDamaged)
    {
        if (isPlayerDamaged)
        {
            UpdateCharacterInfo(damagedInfo, m_PlayerScreen, isPlayerDamaged);
            UpdateCharacterInfo(attackerInfo, m_OpponentScreen, !isPlayerDamaged);
        }
        else
        {
            UpdateCharacterInfo(damagedInfo, m_OpponentScreen, !isPlayerDamaged);
            UpdateCharacterInfo(attackerInfo, m_PlayerScreen, isPlayerDamaged);
        }

        // Define a list of CharacterState for randomDamaged
        List<CharacterState> randomDamaged = new List<CharacterState>
        {
            CharacterState.Angry,
            CharacterState.Panic
        };

        // Assign the correct state to attackerBehavior
        CharacterState attackerBehavior = CharacterState.Excited;
        m_PlayerRectEmoji.DOScale(Vector3.zero, 0);
        m_OpponentRectEmoji.DOScale(Vector3.zero, 0);

        if (damagedInfo != null && damagedInfo.CharacterSystem != null)
        {
            damagedInfo.CharacterSystem.CurrentState = randomDamaged.GetRandom();

            if (damagedInfo.CharacterSO.EmojiSpites.ContainsKey(damagedInfo.CharacterSystem.CurrentEmotionState))
            {
                if (isPlayerDamaged)
                    m_PlayerImageEmoji.sprite = damagedInfo.CharacterSO.EmojiSpites[damagedInfo.CharacterSystem.CurrentEmotionState];
                else
                    m_OpponentImageEmoji.sprite = damagedInfo.CharacterSO.EmojiSpites[damagedInfo.CharacterSystem.CurrentEmotionState];
            }
        }

        if (attackerInfo != null && attackerInfo.CharacterSystem != null)
        {
            attackerInfo.CharacterSystem.CurrentState = attackerBehavior;
            if (attackerInfo.CharacterSO.EmojiSpites.ContainsKey(attackerInfo.CharacterSystem.CurrentEmotionState))
            {
                if (!isPlayerDamaged)
                    m_PlayerImageEmoji.sprite = attackerInfo.CharacterSO.EmojiSpites[attackerInfo.CharacterSystem.CurrentEmotionState];
                else
                    m_OpponentImageEmoji.sprite = attackerInfo.CharacterSO.EmojiSpites[attackerInfo.CharacterSystem.CurrentEmotionState];
            }
        }

        m_PlayerRectEmoji
            .DOScale(Vector3.one, AnimationDuration.SSHORT).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                m_PlayerImageEmoji.transform.DOShakeRotation(1f, strength: 30f).SetEase(Ease.Linear);
            });


        m_OpponentRectEmoji
            .DOScale(Vector3.one, AnimationDuration.SSHORT).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                m_OpponentImageEmoji.transform.DOShakeRotation(1f, strength: 30f).SetEase(Ease.Linear);
            });
    }

    private void UpdateCharacterInfo(CharacterInfo characterInfo, RawImage screen, bool isDamaged)
    {
        if (characterInfo == null) return;
        characterInfo.gameObject.SetActive(true);
        screen.texture = characterInfo.RenderTexture;

        characterInfo.CharacterSystem.CurrentEmotionState = isDamaged
            ? m_CharacterEmotionConfigs.DamagedRobotEmotions.GetRandom()
            : m_CharacterEmotionConfigs.AttackerRobotEmotions.GetRandom();
    }
}
