using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using LatteGames.GameManagement;
using LatteGames.Monetization;
using LatteGames.PvP;
using LatteGames.Template;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossGameOverUI : MonoBehaviour
{
    [SerializeField]
    Button m_ContinueButton, m_NextButton;

    [SerializeField]
    PvPArenaVariable m_ChosenArenaVariable;

    bool m_IsVictory;
    Coroutine m_PlaySoundCoroutine;
    IUIVisibilityController m_UIVisibilityController;
    [SerializeField, BoxGroup("Ref")] private GameObject m_WinHeader;
    [SerializeField, BoxGroup("Ref")] private GameObject m_LoseHeader;
    [SerializeField, BoxGroup("Ref")] private EZAnimSequence m_HeaderEZAnimSequence;
    [SerializeField, BoxGroup("Ref")] private EZAnimSequence m_BodyEZAnimSequence;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroupVisibility;
    [SerializeField, BoxGroup("Data")] private PVPGameOverConfigSO m_PVPGameOverConfigSO;
    [SerializeField, TitleGroup("Data Mode")] private ModeVariable _currentChosenModeVariable;
    [SerializeField, TitleGroup("WIN VFX")] private ParticleSystem _winVFX;

    private int currentChapterIndex => BossFightManager.Instance.selectingChapterIndex;
    private BossChapterSO currentChapterSO => BossFightManager.Instance.bossMapSO.chapterList[currentChapterIndex];
    int currentBossIndex => currentChapterSO.bossIndex.value;

    private void Awake()
    {
        m_UIVisibilityController = GetComponent<IUIVisibilityController>();
        m_ContinueButton.onClick.AddListener(OnContinueBtnClicked);
        m_NextButton.onClick.AddListener(OnContinueBtnClicked);
        // m_RevengeNotAds.onClick.AddListener(ResetBattleBoss);

        //v1.5.1: Disable revenge Boss
        //_revengeRVButtonBehavior.OnRewardGranted += OnRewardGrantedRevenge;

        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnDestroy()
    {
        m_ContinueButton.onClick.RemoveListener(OnContinueBtnClicked);
        m_NextButton.onClick.RemoveListener(OnContinueBtnClicked);
        // m_RevengeNotAds.onClick.RemoveListener(ResetBattleBoss);

        //v1.5.1: Disable revenge Boss
        //_revengeRVButtonBehavior.OnRewardGranted -= OnRewardGrantedRevenge;

        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        if (m_PlaySoundCoroutine != null)
        {
            StopCoroutine(m_PlaySoundCoroutine);
        }
    }

    private void OnRewardGrantedRevenge(RVButtonBehavior.RewardGrantedEventData data)
    {
        ResetBattleBoss();

        #region MonetizationEventCode
        string adsLocation = "LoseBossUI";
        string bossName = $"{currentChapterSO.bossList[currentBossIndex].chassisSO.GetModule<NameItemModule>().displayName}";
        GameEventHandler.Invoke(MonetizationEventCode.RevengeBoss_LoseBossUI, adsLocation, bossName);
        #endregion
    }

    private void ResetBattleBoss()
    {
        GameEventHandler.Invoke(BossEventCode.BossFightStreak, currentChapterSO);
        BossFightManager.Instance.OnSelectCurrentBoss();
        BossFightManager.Instance.currentChosenModeVariable.value = Mode.Boss;
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnContinueBtnClicked()
    {
        m_ContinueButton.interactable = false;
        // m_RevengeButton.interactable = false;

        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        if (matchManager != null)
        {
            var matchOfPlayer = matchManager.GetCurrentMatchOfPlayer();
            matchManager.EndMatch(matchOfPlayer);
        }

        m_UIVisibilityController.Hide();
        SoundManager.Instance.PlaySFX(GeneralSFX.UIExitButton);
        if (m_IsVictory)
        {
            GameEventHandler.Invoke(BossFightEventCode.OnUnlockBoss);
        }
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (!matchOfPlayer.isAbleToComplete)
            return;

        m_IsVictory = matchOfPlayer.isVictory;
        if (matchOfPlayer.isVictory)
            SoundManager.Instance.PlaySFX(PBSFX.UIWinGuitar);
        else
            SoundManager.Instance.PlaySFX(PBSFX.UILoseGuitar);

        CharacterRoomPVP characterRoomPVP = ObjectFindCache<CharacterRoomPVP>.Get();
        if (characterRoomPVP != null)
            characterRoomPVP.OnFinalMatchViewCharacter += ShowHeader;

        CinemachineBrain cinemachineBrain = MainCameraFindCache.Get().GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cinemachineBrain.m_DefaultBlend.m_Time = 0.5f;
        }

        if (characterRoomPVP != null)
        {
            StartCoroutine(CommonCoroutine.Delay(2.5f, false, () =>
            {
                characterRoomPVP.OnStartCameraFinalMatch.Invoke(OnShowGameOver);
            }));
        }

        void ShowHeader()
        {
            m_MainCanvasGroupVisibility.Show();
            m_BodyEZAnimSequence.SetToStart();
            m_HeaderEZAnimSequence.Play();

            m_WinHeader.SetActive(m_IsVictory);
            m_LoseHeader.SetActive(!m_IsVictory);
            GameEventHandler.Invoke(PBPvPEventCode.OnShowGameOverUI, matchOfPlayer);
            if (matchOfPlayer.isVictory)
            {

                if (_winVFX != null)
                    _winVFX.Play();

                StartCoroutine(CommonCoroutine.Delay(0.5f, false, () =>
                {
                    SoundManager.Instance.PlaySFX(PBSFX.UIWinner);
                    SoundManager.Instance.PlaySFX(PBSFX.UIWinFlameThrower);
                }));
            }
            else
            {
                StartCoroutine(CommonCoroutine.Delay(0.5f, false, () =>
                {
                    SoundManager.Instance.PlaySFX(PBSFX.UIYouLose);
                }));
            }
        }
        void OnShowGameOver()
        {
            StartCoroutine(CommonCoroutine.Delay(m_PVPGameOverConfigSO.TimeWaitCharacter, false, () =>
            {
                m_BodyEZAnimSequence.Play();
            }));

            var arenaSO = m_ChosenArenaVariable.value;
            var isVictory = matchOfPlayer.isVictory;
            BossFightManager.Instance.LogBossFightOutcome(isVictory);
            if (isVictory)
            {
                StartCoroutine(CommonCoroutine.Delay(3.5f, false, () =>
                {
                    PiggyBankManager.Instance.CalcPerKill(isVictory);
                }));

                BossFightManager.Instance.DefeatCurrentBoss();
            }

            m_ContinueButton.gameObject.SetActive(isVictory);
            m_NextButton.gameObject.SetActive(!isVictory);
        }
    }
}
