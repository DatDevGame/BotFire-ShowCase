using Cinemachine;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using UnityEngine.TextCore.Text;
using System;
using LatteGames.Template;
using System.Text.RegularExpressions;
using UnityEngine.Experimental.Rendering;

public enum CharacterRoomEvent
{
    OnStartMatch,
    OnEndPreviewEndRound,
    OnBuildRobotCharacterSinglePVP,
    OnBuildRobotCharacterBossPVP,
    OnBuildRobotCharacterBattlePVP
}

public class CharacterRoomPVP : MonoBehaviour
{
    public Action OnFinalMatchViewCharacter;
    public Action<Action> OnStartCameraFinalMatch;
    public CharacterInfo PlayerCharacterInfo => m_PlayerCharacterInfo;
    public List<CharacterInfo> OpponentCharacterInfos => m_OpponentCharacterInfos;
    public Dictionary<PlayerInfo, CharacterInfo> OpponentCharacterInfoDictionary => m_OpponentCharacterInfoDictionary;
    public CinemachineVirtualCamera CinemachineVirtualCamera => m_CinemachineVirtualCamera;

    [SerializeField, BoxGroup("Config")] private bool m_IsFTUE;

    [SerializeField, BoxGroup("Ref")] private Transform m_PlayerSpawnPoint;
    [SerializeField, BoxGroup("Ref")] private Transform m_PointStartCamera;
    [SerializeField, BoxGroup("Ref")] private Transform m_PointEndCameraSingle;
    [SerializeField, BoxGroup("Ref")] private Transform m_PointEndCameraBattle;
    [SerializeField, BoxGroup("Ref")] private CinemachineVirtualCamera m_CinemachineVirtualCamera;
    [SerializeField, BoxGroup("Ref")] private CharacterInfo m_PlayerCharacterInfo;
    [SerializeField, BoxGroup("Ref")] private List<CharacterInfo> m_OpponentCharacterInfos;

    [SerializeField, BoxGroup("Data")] private PlayerInfoVariable m_PlayerInfoVariable;
    [SerializeField, BoxGroup("Data")] private PlayerInfoVariable m_OpponentInfoVariable;
    [SerializeField, BoxGroup("Data")] private CharacterManagerSO m_CharacterManagerSO;
    [SerializeField, BoxGroup("Data")] private PVPGameOverConfigSO m_PVPGameOverConfigSO;


    private CharacterSystem m_CharacterPlayer;
    private List<CharacterSystem> m_CharacterOpponents;
    private Dictionary<PlayerInfo, CharacterInfo> m_OpponentCharacterInfoDictionary;
    private bool m_IsWinMatch;
    private object[] m_BuildRobotSingelePVPParameters;
    private object[] m_BuildRobotBossPVPParameters;
    private object[] m_BuildRobotBattlePVPParameters;

    private void Awake()
    {
        ObjectFindCache<CharacterRoomPVP>.Add(this);
        m_OpponentCharacterInfoDictionary = new Dictionary<PlayerInfo, CharacterInfo>();

        OnStartCameraFinalMatch += OnStartPreviewEndRound;
        GameEventHandler.AddActionEvent(CharacterRoomEvent.OnStartMatch, OnStartCamera);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.AddActionEvent(CharacterRoomEvent.OnBuildRobotCharacterSinglePVP, OnBuildRobotCharacterSinglePVP);
        GameEventHandler.AddActionEvent(CharacterRoomEvent.OnBuildRobotCharacterBossPVP, OnBuildRobotCharacterBossPVP);
        GameEventHandler.AddActionEvent(CharacterRoomEvent.OnBuildRobotCharacterBattlePVP, OnBuildRobotCharacterBattlePVP);
    }

    private void Start()
    {
        if (m_IsFTUE)
            InitCharacter();
    }

    private void OnDestroy()
    {
        ObjectFindCache<CharacterRoomPVP>.Remove(this);
        OnStartCameraFinalMatch -= OnStartPreviewEndRound;
        GameEventHandler.RemoveActionEvent(CharacterRoomEvent.OnStartMatch, OnStartCamera);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.RemoveActionEvent(CharacterRoomEvent.OnBuildRobotCharacterSinglePVP, OnBuildRobotCharacterSinglePVP);
        GameEventHandler.RemoveActionEvent(CharacterRoomEvent.OnBuildRobotCharacterBossPVP, OnBuildRobotCharacterBossPVP);
        GameEventHandler.RemoveActionEvent(CharacterRoomEvent.OnBuildRobotCharacterBattlePVP, OnBuildRobotCharacterBattlePVP);
    }
    private void InitCharacter()
    {
        Mode mode = m_CharacterManagerSO.CurrentMode.value;

        m_CharacterOpponents = new List<CharacterSystem>();
        CharacterSO playerCharacterSO = m_CharacterManagerSO.PlayerCharacterSO.value as CharacterSO;
        List<CharacterSO> opponentCharacterSOs = m_CharacterManagerSO.OpponentCharacterSOs
            .Where(v => v.value is CharacterSO)
            .Select(v => v.value as CharacterSO)
            .ToList();
        OnEnableInfoFollowingMode(mode);

        if (playerCharacterSO != null)
        {
            m_CharacterPlayer = SpawnCharacter(playerCharacterSO, m_PlayerSpawnPoint);
            m_CharacterPlayer.EnableController();
            m_CharacterPlayer.SetActivePVP(true);
            m_CharacterPlayer.IsPlayer = true;
            m_PlayerCharacterInfo.SetCharacterSystem(m_CharacterPlayer, playerCharacterSO);
            m_CharacterPlayer.gameObject.SetLayer(m_PlayerSpawnPoint.gameObject.layer, true);
            m_CharacterPlayer.Controller.gameObject.SetLayer(0);
        }
        //FTUE
        if (m_IsFTUE)
        {
            if (m_PlayerInfoVariable != null)
                SpawnRobotCharacter(m_PlayerInfoVariable, m_PlayerCharacterInfo);

            //Set Default Animation
            m_CharacterPlayer.CurrentState = CharacterState.Control;
            return;
        }

        if (mode == Mode.Normal || mode == Mode.Boss)
        {
            var opponentCharacterSystem = SpawnCharacter(opponentCharacterSOs[0], m_OpponentCharacterInfos[0].transform);
            opponentCharacterSystem.gameObject.SetLayer(m_OpponentCharacterInfos[0].gameObject.layer, true);
            opponentCharacterSystem.Controller.gameObject.SetLayer(0);
            opponentCharacterSystem.EnableController();
            opponentCharacterSystem.SetActivePVP(true);
            m_OpponentCharacterInfos[0].SetCharacterSystem(opponentCharacterSystem, opponentCharacterSOs[0]);
            m_CharacterOpponents.Add(opponentCharacterSystem);
        }
        else if (mode == Mode.Battle)
        {
            if (opponentCharacterSOs != null && opponentCharacterSOs.Count > 0)
            {
                for (int i = 0; i < opponentCharacterSOs.Count; i++)
                {
                    var opponentCharacterSystem = SpawnCharacter(opponentCharacterSOs[i], m_OpponentCharacterInfos[i].transform);
                    opponentCharacterSystem.IsPlayer = false;
                    opponentCharacterSystem.gameObject.SetLayer(m_OpponentCharacterInfos[i].gameObject.layer, true);
                    opponentCharacterSystem.Controller.gameObject.SetLayer(0);
                    opponentCharacterSystem.EnableController();
                    opponentCharacterSystem.SetActivePVP(true);
                    m_OpponentCharacterInfos[i].SetCharacterSystem(opponentCharacterSystem, opponentCharacterSOs[i]);
                    m_CharacterOpponents.Add(opponentCharacterSystem);
                }
            }
        }

        //Set Default Animation
        m_CharacterPlayer.CurrentState = CharacterState.Control;
        m_CharacterOpponents.ForEach(character => character.CurrentState = CharacterState.Control);
    }

    private IEnumerator SetCharacterStateWithDelay(List<CharacterSystem> characterOpponents, float timeMoveCameraDuration)
    {
        foreach (var character in characterOpponents)
        {
            character.CurrentState = CharacterState.ReadyFight;
            yield return new WaitForSeconds(timeMoveCameraDuration / characterOpponents.Count);
        }
    }
    private void OnStartCamera()
    {
        DOVirtual.DelayedCall(0.1f, () => { GameEventHandler.Invoke(PBPvPEventCode.OnCompleteCamRotation); });
        //Mode mode = m_CharacterManagerSO.CurrentMode.value;
        //float timeDurationCamera = mode == Mode.Normal ? 2 : 5;
        //float delayMoveCameraDuration = 0.5f;
        //float delayCallOnComplete = 1f;

        //m_CinemachineVirtualCamera.enabled = true;
        //m_CharacterPlayer.CurrentState = CharacterState.Wave;

        //if (mode == Mode.Normal)
        //    m_CharacterOpponents.ForEach((v) => { v.CurrentState = CharacterState.Wave; });
        //else
        //    StartCoroutine(SetCharacterStateWithDelay(m_CharacterOpponents, timeDurationCamera));

        //Vector3 pointEndCamera = mode == Mode.Normal ? m_PointEndCameraSingle.position : m_PointEndCameraBattle.position;
        //m_CinemachineVirtualCamera.transform
        //    .DOMove(pointEndCamera, timeDurationCamera).SetEase(Ease.Linear)
        //    .SetDelay(delayMoveCameraDuration)
        //    .OnComplete(() =>
        //    {
        //        DOVirtual.DelayedCall(delayCallOnComplete, () =>
        //        {
        //            m_CinemachineVirtualCamera.enabled = false;
        //            GameEventHandler.Invoke(PBPvPEventCode.OnCompleteCamRotation);
        //        })
        //        .OnComplete(() => 
        //        {
        //            m_PlayerCharacterModelRobot.RemoveRobot();
        //            m_OpponentCharacterModelRobots.ForEach(v => v.RemoveRobot());
        //            OnEnableAllPoint(false);
        //        });
        //    });
    }
    private void OnStartPreviewEndRound(Action action)
    {
        PlayerCharacterInfo.gameObject.SetActive(true);
        m_CharacterPlayer.CurrentState = m_IsWinMatch ? CharacterState.Victory : CharacterState.Defeat;

        CinemachineBrain cinemachineBrain = MainCameraFindCache.Get().GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
            cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;

        m_CharacterPlayer.SetActivePVP(false);
        //m_CharacterPlayer.CurrentState = CharacterState.Idle;
        //m_CharacterOpponents.ForEach(characerSystem =>
        //{
        //    characerSystem.SetActivePVP(false);
        //    characerSystem.CurrentState = CharacterState.Idle;
        //});

        Mode mode = m_CharacterManagerSO.CurrentMode.value;

        if (m_BuildRobotSingelePVPParameters != null && m_BuildRobotSingelePVPParameters.Length > 0 && mode == Mode.Normal)
            OnBuildRobotCharacterSinglePVP(m_BuildRobotSingelePVPParameters);
        if (m_BuildRobotBossPVPParameters != null && m_BuildRobotBossPVPParameters.Length > 0 && mode == Mode.Boss)
            OnBuildRobotCharacterBossPVP(m_BuildRobotBossPVPParameters);
        if (m_BuildRobotBattlePVPParameters != null && m_BuildRobotBattlePVPParameters.Length > 0 && mode == Mode.Battle)
            OnBuildRobotCharacterBattlePVP(m_BuildRobotBattlePVPParameters);

        m_OpponentCharacterInfos.ForEach(v => v.gameObject.SetActive(false));

        m_CinemachineVirtualCamera.enabled = true;
        OnFinalMatchViewCharacter?.Invoke();

        m_CinemachineVirtualCamera.transform
            .DOMove(m_PointEndCameraSingle.position, 0).SetEase(Ease.Linear);
        StartCoroutine(CommonCoroutine.Delay(m_PVPGameOverConfigSO.TimeWaitCharacter, false, () =>
        {
            action?.Invoke();
        }));
    }
    private void OnMatchStarted()
    {
        InitCharacter();
    }
    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        m_IsWinMatch = matchOfPlayer.isVictory;
    }
    private void OnEnableInfoFollowingMode(Mode mode)
    {
        if (m_IsFTUE)
        {
            m_OpponentCharacterInfos.ForEach(v => v.gameObject.SetActive(false));
            m_PlayerCharacterInfo.gameObject.SetActive(true);
            return;
        }

        if (mode == Mode.Normal)
        {
            m_PlayerCharacterInfo.gameObject.SetActive(true);
            m_OpponentCharacterInfos[0].gameObject.SetActive(true);
        }
        else if (mode == Mode.Boss)
        {
            m_PlayerCharacterInfo.gameObject.SetActive(true);
        }
        else
        {
            m_OpponentCharacterInfos.ForEach(v => v.gameObject.SetActive(true));
        }
    }
    private void OnBuildRobotCharacterSinglePVP(object[] parameters)
    {
        if (parameters == null || parameters[0] == null) return;
        if (parameters[0] is not PlayerInfoVariable opponentInfo) return;
        m_BuildRobotSingelePVPParameters = parameters;

        //Spawn Robot Player
        if (m_PlayerInfoVariable != null)
            SpawnRobotCharacter(m_PlayerInfoVariable, m_PlayerCharacterInfo);

        //Spawn Robot Opponent
        if (m_OpponentInfoVariable != null)
            SpawnRobotCharacter(opponentInfo, m_OpponentCharacterInfos[0]);
    }
    private void OnBuildRobotCharacterBossPVP(object[] parameters)
    {
        if (parameters == null || parameters[0] == null) return;
        if (parameters[0] is not PlayerInfo opponentInfo) return;
        m_BuildRobotBossPVPParameters = parameters;
        PlayerInfoVariable opponentInfoVariable = new PlayerInfoVariable();
        opponentInfoVariable.value = opponentInfo;

        //Spawn Robot Player
        if (m_PlayerInfoVariable != null)
            SpawnRobotCharacter(m_PlayerInfoVariable, m_PlayerCharacterInfo);

        //Spawn Robot Opponent
        if (m_OpponentInfoVariable != null)
            SpawnRobotCharacter(opponentInfoVariable, m_OpponentCharacterInfos[0]);
    }
    private void OnBuildRobotCharacterBattlePVP(object[] parameters)
    {
        if (parameters == null || parameters[0] == null) return;
        m_BuildRobotBattlePVPParameters = parameters;
        List<object[]> listparameters = parameters[0] as List<object[]>;

        //Spawn Robot Player
        if (m_PlayerInfoVariable != null)
            SpawnRobotCharacter(m_PlayerInfoVariable, m_PlayerCharacterInfo);

        for (int i = 0; i < listparameters.Count; i++)
        {
            if (listparameters[i] == null) return;
            if (listparameters[i][0] is not PBPvPArenaSO arenaSO) return;
            if (listparameters[i][1] is not PlayerInfoVariable opponentVariable) return;

            //Spawn Robot Opponent
            if (opponentVariable != null)
                SpawnRobotCharacter(opponentVariable, m_OpponentCharacterInfos[i]);
        }
    }

    private void SpawnRobotCharacter(PlayerInfo info, CharacterInfo characterInfo)
    {
        if (!m_OpponentCharacterInfoDictionary.ContainsKey(info))
            m_OpponentCharacterInfoDictionary.Add(info, characterInfo);

        PBPlayerInfo robotStats = info.Cast<PBPlayerInfo>();
        PBChassisSO chassisSO = robotStats.robotStatsSO.chassisInUse.value.Cast<PBChassisSO>();
    }

    private CharacterSystem SpawnCharacter(CharacterSO characterSO, Transform holder)
    {
        CharacterSystem characterSystem = null;
        if (characterSO.TryGetModule<GameObjectModelPrefabItemModule>(out var modelPrefabItemModule))
        {
            var characterModel = modelPrefabItemModule.modelPrefabAsGameObject.GetComponent<CharacterSystem>();
            if (characterModel != null)
            {
                var instanceCharacter = Instantiate(characterModel, holder);
                instanceCharacter.transform.localPosition = Vector3.zero;
                characterSystem = instanceCharacter;
            }
        }
        return characterSystem;
    }
}
