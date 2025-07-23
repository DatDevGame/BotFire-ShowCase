using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.PvP;
using LatteGames.Template;
using TMPro;
using UnityEngine;

public class PBPvPLookingForOpponentUI : MonoBehaviour
{
    [SerializeField]
    protected ModeVariable m_ChosenModeVariable;
    [SerializeField]
    protected PlayerInfoVariable m_InfoOfPlayer;
    [SerializeField]
    private PBPvPPlayerNameUI m_PlayerInfoTransform, m_OpponentInfoTransform;
    [SerializeField]
    private RectTransform m_ContainerTransform, m_CenterTransform, m_UnknownedOpponentTransform;
    [SerializeField]
    private TextMeshProUGUI m_SearchingForOpponentText;
    [SerializeField]
    private RectTransform m_MiddleContentTransform, m_HeaderContentTransform;
    [SerializeField]
    private TextMeshProUGUI m_TimerTMP;
    [SerializeField]
    CanvasGroupVisibility canvasGroupVisibility;

    private bool isEndCamRotation = false;
    private bool _isSearchingOpponent = false;

    private void Awake()
    {
        if (m_ChosenModeVariable.value != Mode.Normal)
        {
            gameObject.SetActive(false);
            return;
        }
        canvasGroupVisibility.HideImmediately();
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnCompleteCamRotation, OnCompleteCamRotation);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnOpponentFounded, OnOpponentFounded);
    }

    private void OnDestroy()
    {
        if (m_ChosenModeVariable.value != Mode.Normal)
            return;
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnCompleteCamRotation, OnCompleteCamRotation);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnOpponentFounded, OnOpponentFounded);
    }

    private void Start()
    {
        //if (m_ChosenModeVariable.value != Mode.Normal)
        //    return;
        //StartCoroutine(CountLookingForOpponent_CR());
        //m_PlayerInfoTransform.PersonalInfo = m_InfoOfPlayer.value.personalInfo;
    }
    private void OnCompleteCamRotation()
    {
        canvasGroupVisibility.Show();
        isEndCamRotation = true;
    }
    private IEnumerator CountLookingForOpponent_CR()
    {
        var totalTimeToWait = 0;
        var waitForOneSecs = new WaitForSeconds(1f);
        while (true)
        {
            m_TimerTMP.text = totalTimeToWait++.ToString();
            yield return waitForOneSecs;
        }
    }
    private IEnumerator DelayOnOpponentFounded(object[] parameters)
    {
        if (m_ChosenModeVariable.value != Mode.Normal)
            yield break;

        var arenaSO = parameters[0] as PBPvPArenaSO;
        var opponent = parameters[1] as PlayerInfoVariable;
        var match = PBPvPMatch.CreateMatch(arenaSO, Mode.Normal, m_InfoOfPlayer, opponent);
        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        GameEventHandler.Invoke(CharacterRoomEvent.OnBuildRobotCharacterSinglePVP, opponent);

        yield return new WaitUntil(() => isEndCamRotation);
        _isSearchingOpponent = true;
        if (m_ChosenModeVariable.value == Mode.Normal)
        {
            //StartCoroutine(CountLookingForOpponent_CR());
            m_PlayerInfoTransform.PersonalInfo = m_InfoOfPlayer.value.personalInfo;
        }
        //yield return new WaitForSeconds(2);

        m_SearchingForOpponentText.gameObject.SetActive(false);
        StopAllCoroutines();
        SoundManager.Instance.PlaySFX(PBSFX.UIOpponentAppear);
        m_OpponentInfoTransform.PersonalInfo = opponent.value.personalInfo;
        matchManager.PrepareMatch(match);
        _isSearchingOpponent = false;
        StartCoroutine(DOSnapAnimation_CR());
    }
    private void OnOpponentFounded(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        StartCoroutine(DelayOnOpponentFounded(parameters));
    }

    private IEnumerator DOSnapAnimation_CR()
    {
        //m_MiddleContentTransform.localScale = Vector3.zero;

        m_UnknownedOpponentTransform.gameObject.SetActive(false);
        m_PlayerInfoTransform.GetComponent<Animation>().Play();
        m_OpponentInfoTransform.GetComponent<Animation>().Play();

        yield return new WaitForSeconds(0.75f);
        m_CenterTransform.DOMove(m_HeaderContentTransform.position, 0).OnComplete(OnSnapCompleted);

        yield return new WaitForSeconds(0.2f);
        //m_MiddleContentTransform.DOScale(Vector3.one, AnimationDuration.TINY);

        void OnSnapCompleted()
        {
            m_ContainerTransform.gameObject.SetActive(false);
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (_isSearchingOpponent)
        {
            _isSearchingOpponent = false;

            string name = "";
            GameObject pbFightingStage = PBFightingStage.Instance.gameObject;
            if (pbFightingStage != null)
            {
                name = pbFightingStage.name.Replace("(Clone)", "");
            }

            string stageName = name;
            string mainPhase = "Matchmaking";
            string subPhase = "MatchStarting";
            GameEventHandler.Invoke(DesignEvent.QuitCheck, stageName, mainPhase, subPhase);
        }
    }
}
