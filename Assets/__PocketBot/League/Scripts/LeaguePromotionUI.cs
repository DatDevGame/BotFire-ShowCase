using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeaguePromotionUI : ComposeCanvasElementVisibilityController
{
    [SerializeField]
    private DivisionUI m_DivisionUIPrefab;
    [SerializeField]
    private RectTransform m_DivisionContainer;
    [SerializeField]
    private RectTransform m_Container;
    [SerializeField]
    private RectTransform m_TargetDivisionBadgeRectTransform;
    [SerializeField]
    private Localize m_TitleTexLocalize;
    [SerializeField]
    private LocalizedString m_PromotedToNextDivisionLocalizedString;
    [SerializeField]
    private LocalizedString m_YouCompletedFinalDivisionLocalizedString;
    [SerializeField]
    private LocalizationParamsManager m_TitleDivisionParamsManager;
    [SerializeField]
    private LocalizationParamsManager m_NewDivisionBestRewardParamsManager;
    [SerializeField]
    private LeagueRewardUI m_LeagueRewardUI;
    [SerializeField]
    private GameObject m_NewDivisionInfo;
    [SerializeField]
    private PBModelRenderer m_PlayerModelRenderer;
    [SerializeField]
    private RawImage m_PlayerRawImage;
    [SerializeField]
    private EventTrigger m_BGEventTrigger;
    [SerializeField]
    private TextMeshProUGUI m_NumOfPlayersText;
    [SerializeField]
    private EZAnimBase[] m_PhaseAnimControllers;

    private PBModelRenderer m_TempModelRenderer;
    private List<DivisionUI> m_DivisionUIs = new List<DivisionUI>();

    public int currentDivisionIndex { get; set; }
    public int nextDivisionIndex { get; set; }
    public LeagueDataSO leagueDataSO => LeagueManager.leagueDataSO;

    private void Start()
    {
        m_DivisionUIs = LeagueDivisionUI.GenerateDivisionUI(m_DivisionUIPrefab, m_DivisionContainer);
    }

    private IEnumerator PlayGetPromotedAnimation_CR(int currentDivisionIndex, int nextDivisionIndex)
    {
        #region Firebase Event
        if (nextDivisionIndex < leagueDataSO.divisions.Length)
        {
            try
            {
                string division = GetDivisionID().ToLower();
                int numberOfPlayers = leagueDataSO.divisions[nextDivisionIndex].playerPoolCount;
                GameEventHandler.Invoke(LogFirebaseEventCode.StartOfDivisionPopUp, division, numberOfPlayers);
                string GetDivisionID()
                {
                    int divisionID = nextDivisionIndex + 1;
                    return divisionID switch
                    {
                        1 => "Rookie",
                        2 => "Contender",
                        3 => "Advanced",
                        4 => "Expert",
                        5 => "Elite",
                        _ => "Unknown",
                    };
                }

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
        }
        #endregion

        bool isAnimCompleted = false;
        int clampedNextDivisionIndex = Mathf.Clamp(nextDivisionIndex, 0, leagueDataSO.divisions.Length - 1);
        LeagueDivision currentDivision = leagueDataSO.divisions[currentDivisionIndex];
        LeagueDivision nextDivision = leagueDataSO.divisions[clampedNextDivisionIndex];
        GetAnimControllerOfPhase(1).SetToEnd();
        GetAnimControllerOfPhase(2).SetToStart();
        for (int i = 0; i < m_DivisionUIs.Count; i++)
        {
            m_DivisionUIs[i].Initialize(leagueDataSO.divisions[i], LeagueDataSO.GetStatus(i, currentDivisionIndex));
            m_DivisionUIs[i].visibilityAnim.SetToEnd();
            m_DivisionUIs[i].FadeIn(0f);
        }
        yield return Yielders.Get(AnimationDuration.SSHORT);
        if (currentDivisionIndex < m_DivisionUIs.Count - 1)
        {
            RectTransform originSelectionFrame = m_DivisionUIs[currentDivisionIndex].selectionFrameRectTransform;
            RectTransform clonedSelectionFrame = Instantiate(originSelectionFrame, originSelectionFrame.parent);
            clonedSelectionFrame.SetParent(m_Container);
            clonedSelectionFrame.anchorMin = clonedSelectionFrame.anchorMax = Vector2.one * 0.5f;
            clonedSelectionFrame.sizeDelta = originSelectionFrame.rect.size;
            clonedSelectionFrame.position = originSelectionFrame.position;
            clonedSelectionFrame.DOPunchScale(-0.15f * Vector3.one, AnimationDuration.SSHORT + 0.25f, 4, 1f).SetEase(Ease.Linear);
            clonedSelectionFrame.DOMove(m_DivisionUIs[nextDivisionIndex].selectionFrameRectTransform.position, AnimationDuration.SSHORT).SetEase(Ease.Linear);
            SoundManager.Instance.PlaySFX(PBSFX.UIPromotedDivision);
            m_DivisionUIs[currentDivisionIndex].UpdateStatus(LeagueDataSO.Status.Upcoming);
            yield return new WaitForSeconds(AnimationDuration.SSHORT + 0.25f);
            m_DivisionUIs[nextDivisionIndex].UpdateStatus(LeagueDataSO.Status.Present);
            clonedSelectionFrame.gameObject.SetActive(false);
            Destroy(clonedSelectionFrame.gameObject);
        }
        DivisionUI nextDivisionUI = Instantiate(m_DivisionUIs[clampedNextDivisionIndex], m_Container);
        nextDivisionUI.rectTransform.position = m_DivisionUIs[clampedNextDivisionIndex].rectTransform.position;
        nextDivisionUI.rectTransform.sizeDelta = m_DivisionUIs[clampedNextDivisionIndex].rectTransform.sizeDelta;
        nextDivisionUI.visibilityAnim.SetToEnd();
        // FadeOut all divisions
        for (int i = 0; i < m_DivisionUIs.Count; i++)
        {
            m_DivisionUIs[i].FadeOut(AnimationDuration.TINY);
        }
        GetAnimControllerOfPhase(1).InversePlay(() => isAnimCompleted = true);
        yield return new WaitUntil(() => isAnimCompleted);
        isAnimCompleted = false;
        // Then move the badge to the top
        nextDivisionUI.FadeOut(AnimationDuration.TINY);
        nextDivisionUI.iconImage.transform.SetParent(m_Container);
        nextDivisionUI.iconImage.transform.SetSiblingIndex(m_Container.childCount - 3);
        nextDivisionUI.iconImage.transform.DOMove(m_TargetDivisionBadgeRectTransform.position, AnimationDuration.SSHORT + AnimationDuration.TINY).SetEase(Ease.Linear);
        nextDivisionUI.iconImage.rectTransform.DOSizeDelta(m_TargetDivisionBadgeRectTransform.sizeDelta, AnimationDuration.SSHORT + AnimationDuration.TINY).SetEase(Ease.Linear);
        SoundManager.Instance.PlaySFX(PBSFX.UIBadgeMoves);
        yield return Yielders.Get(AnimationDuration.SSHORT + AnimationDuration.TINY);
        UpdateViewPhase2();
        SpawnPreviewBot();
        GetAnimControllerOfPhase(2).Play(() => isAnimCompleted = true);
        yield return new WaitUntil(() => isAnimCompleted);
        ConfettiParticleUI.Instance.PlayFX();
        SoundManager.Instance.PlaySFX(PBSFX.UIConfettiTrumpet);
        EventTrigger.Entry pointerDownEvent = m_BGEventTrigger.triggers.Find(item => item.eventID == EventTriggerType.PointerDown);
        pointerDownEvent.callback.AddListener(OnTapContinue);
        //ReleaseTempResources();

        EZAnimBase GetAnimControllerOfPhase(int phase)
        {
            return m_PhaseAnimControllers[Mathf.Clamp(phase - 1, 0, m_PhaseAnimControllers.Length - 1)];
        }

        void UpdateViewPhase2()
        {
            if (currentDivisionIndex < m_DivisionUIs.Count - 1)
            {
                m_TitleTexLocalize.Term = m_PromotedToNextDivisionLocalizedString.mTerm;
                m_LeagueRewardUI.UpdateViewWithBestReward(nextDivision.bestRewardInfo);
                m_NewDivisionInfo.gameObject.SetActive(true);
                RunCountdownAnimation();
            }
            else
            {
                m_TitleTexLocalize.Term = m_YouCompletedFinalDivisionLocalizedString.mTerm;
                m_NewDivisionInfo.gameObject.SetActive(false);
            }
            m_TitleDivisionParamsManager.SetParameterValue("NewDivision", nextDivision.displayName);
            m_TitleDivisionParamsManager.SetParameterValue("FinalDivision", nextDivision.displayName);
            m_NewDivisionBestRewardParamsManager.SetParameterValue("NewDivision", nextDivision.displayName);
        }

        void SpawnPreviewBot()
        {
            m_TempModelRenderer = Instantiate(m_PlayerModelRenderer);
            m_TempModelRenderer.transform.position = Vector3.right * 5000f;
            m_TempModelRenderer.renderCamera.targetTexture = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.Default);
            m_PlayerRawImage.texture = m_TempModelRenderer.renderCamera.targetTexture;
        }

        void RunCountdownAnimation()
        {
            float originFontSize = m_NumOfPlayersText.fontSize;
            Sequence sequence = DOTween.Sequence();
            sequence
                .Join(m_NumOfPlayersText.DOFontSize(originFontSize * 1.4f, AnimationDuration.ZERO))
                .Join(m_NumOfPlayersText.DOCounter(currentDivision.playerPoolCount, nextDivision.playerPoolCount, 2.5f))
                .Join(m_NumOfPlayersText.rectTransform.DOPunchPosition(Vector3.down * 10f, 0.25f, 5, 1f).SetLoops(Mathf.FloorToInt(2.5f / 0.25f)))
                .Append(m_NumOfPlayersText.DOFontSize(originFontSize, AnimationDuration.SSHORT))
                .Play();
            SoundManager.Instance.PlaySFX(PBSFX.UIPlayerPool);
        }

        void OnTapContinue(BaseEventData eventData)
        {
            HideImmediately();
            ReleaseTempResources();
            pointerDownEvent.callback.RemoveListener(OnTapContinue);
        }

        void ReleaseTempResources()
        {
            RenderTexture.ReleaseTemporary(m_TempModelRenderer.renderCamera.targetTexture);
            Destroy(m_TempModelRenderer.gameObject);
            Destroy(nextDivisionUI.gameObject);
            Destroy(nextDivisionUI.iconImage.gameObject);
        }
    }

    public void PlayGetPromotedAnimation(int currentDivisionIndex = 0, int nextDivisionIndex = 1)
    {
        StartCoroutine(PlayGetPromotedAnimation_CR(currentDivisionIndex, nextDivisionIndex));
    }

    [Button]
    public override void Show()
    {
        base.Show();
        PlayGetPromotedAnimation(currentDivisionIndex, nextDivisionIndex);
    }

    [Button]
    public override void ShowImmediately()
    {
        base.ShowImmediately();
        PlayGetPromotedAnimation(currentDivisionIndex, nextDivisionIndex);
    }

    public void Initialize(int currentDivisionIndex, int nextDivisionIndex)
    {
        this.currentDivisionIndex = currentDivisionIndex;
        this.nextDivisionIndex = nextDivisionIndex;
    }
}