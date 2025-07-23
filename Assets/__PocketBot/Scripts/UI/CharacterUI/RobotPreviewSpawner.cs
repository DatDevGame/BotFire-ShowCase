using UnityEngine;
using HyrphusQ.Events;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Animations;
using Sirenix.OdinInspector;
using DG.Tweening;
using System.Linq;

public class RobotPreviewSpawner : Singleton<RobotPreviewSpawner>
{
    [Header("Reference")]
    [SerializeField] ItemSOVariable tempChassisSO;
    [SerializeField] PBPartManagerSO skinManagerSO;
    [SerializeField] PBPartManagerSO chassisManagerSO;
    [SerializeField] PBPartManagerSO wheelManagerSO;
    [SerializeField] PBRobot robot;
    [SerializeField] PBChassisSO tempChassis;

    [SerializeField, BoxGroup("Reference")] private GameObject gearSlotNumberPrefab;
    [SerializeField, BoxGroup("Reference")] private List<GameObject> gearSlotNumbers;

    private List<Transform> slotFollowingPart = new List<Transform>();
    private List<PBChassis.PartContainer> partContainersPro = new List<PBChassis.PartContainer>();
    private List<GearSlotNumberUI> gearSlotNumberUIPros = new List<GearSlotNumberUI>();

    private PBPartType pbPartTypeTabUI;
    private IEnumerator FindPBChassisCR;
    private IEnumerator _delayHideRobotPreviewCR;

    private void Start()
    {
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnIndexTab, GetIndexTab);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnSelectSpecial, SelectGearSpecial);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearButtonClick, HandleOnGearButtonClick);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnTabButtonChange, HandleOnTabButtonClick);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearCardEquip, HandleOnGearButtonClick);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearCardUnEquip, HandleOnTabButtonClick);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, HandleMainSceneTabButtonClicked);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleCharacterUITabButtonClicked);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, SendInfo);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnSwapPart, OnSwapPart);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnUnlockBoss, OnBossUnlocking);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnDisableUnlockBoss, OnUnlockBossSceneDisable);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnClickTabButtonUI, OnClickTabButtonUI);

        tempChassis = Instantiate(chassisManagerSO).currentItemInUse.Cast<PBChassisSO>();

        // _delayHideRobotPreviewCR = DelayShowOrHideRobotPreview(1, false);
        // StartCoroutine(_delayHideRobotPreviewCR);
        HandleOnTabButtonClick(PBPartType.Body);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnIndexTab, GetIndexTab);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnSelectSpecial, SelectGearSpecial);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearButtonClick, HandleOnGearButtonClick);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnTabButtonChange, HandleOnTabButtonClick);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardEquip, HandleOnGearButtonClick);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardUnEquip, HandleOnTabButtonClick);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, HandleMainSceneTabButtonClicked);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleCharacterUITabButtonClicked);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnSwapPart, OnSwapPart);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, SendInfo);
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnUnlockBoss, OnBossUnlocking);
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnDisableUnlockBoss, OnUnlockBossSceneDisable);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnClickTabButtonUI, OnClickTabButtonUI);
    }

    private void FixedUpdate()
    {
        //Update Badge slot
        for (int i = 0; i < gearSlotNumbers.Count; i++)
        {
            if (slotFollowingPart[i] != null && gearSlotNumberUIPros[i] != null)
            {
                if (MainCameraFindCache.Get() == null)
                {
                    gearSlotNumberUIPros[i].ObjectNumberSlot.gameObject.SetActive(false);
                }
                else
                {
                    var posConvert = MainCameraFindCache.Get().WorldToScreenPoint(slotFollowingPart[i].position);

                    gearSlotNumberUIPros[i].ObjectNumberSlot.gameObject.SetActive(true);
                    // Smooth transition using DOTween
                    gearSlotNumberUIPros[i].ObjectNumberSlot.transform.DOMove(posConvert, 0.05f).SetEase(Ease.InOutSine);
                }
            }
        }
    }

    private void GenerateBody(ItemSO chassisSO, bool isBody)
    {
        var chassisSOInstance = Instantiate(chassisSO).Cast<PBChassisSO>();
        tempChassisSO.value = chassisSOInstance;
        List<BotPartSlot> botPartSlots = new List<BotPartSlot>(chassisSOInstance.AllPartSlots);
        if (!chassisSOInstance.IsSpecial)
        {
            List<PBPartSO> equippedWheels = new();
            botPartSlots = new List<BotPartSlot>();
            for (int i = 0; i < chassisSOInstance.AllPartSlots.Count; i++)
            {
                var slotInstance = new BotPartSlot(chassisSOInstance.AllPartSlots[i].PartSlotType, ScriptableObject.CreateInstance<ItemSOVariable>());
                botPartSlots.Add(slotInstance);
                if (isBody)
                {
                    slotInstance.PartVariableSO.value = chassisSOInstance.AllPartSlots[i].PartVariableSO.value;
                }
                else if (slotInstance.PartType == PBPartType.Wheels)
                {

                }
                else
                {
                    slotInstance.PartVariableSO.value = null;
                }
                chassisSOInstance.AllPartSlots[i] = slotInstance;
            }
        }

        AttachWheelToChassis();
        void AttachWheelToChassis()
        {
            var wheelSlots = chassisSOInstance.AllPartSlots.FindAll(x => x.PartType == PBPartType.Wheels);
            for (int i = 0; i < wheelSlots.Count; i++)
            {
                var wheel = wheelSlots[i];
                wheel.PartVariableSO.value = i < chassisSOInstance.AttachedWheels.Count ? chassisSOInstance.AttachedWheels[i] : chassisSOInstance.AttachedWheels.Last();
                ((PBPartSO)wheel.PartVariableSO.value).IsEquipped = true;
            }
        }
        robot.BuildRobot();
        SendInfo();

        if (FindPBChassisCR != null)
            StopCoroutine(FindPBChassisCR);
        FindPBChassisCR = FindPBChassis(botPartSlots, pbPartTypeTabUI);
        StartCoroutine(FindPBChassisCR);
    }

    void HandleOnGearButtonClick(params object[] parameters)
    {
        PBPartSO Part = (PBPartSO)parameters[0];
        PBPartSlot partSlot = (PBPartSlot)parameters[1];
        bool isSelect = false;
        if (parameters.Length >= 3)
            isSelect = (bool)parameters[2];

        if (partSlot.GetPartTypeOfPartSlot() == PBPartType.Body)
        {
            if (Part == tempChassis)
                return;
            BuildRobot(Part, partSlot);
        }

        if (Part.IsEquipped && isSelect)
        {
            GameEventHandler.Invoke(CharacterUIEventCode.OnSwapPart);
            return;
        }

        List<BotPartSlot> botPartSlots = new List<BotPartSlot>();
        List<BotPartSlot> partSlots = new List<BotPartSlot>();
        if (partSlot.GetPartTypeOfPartSlot() != PBPartType.Body)
        {
            PBChassisSO pBChassisSO = chassisManagerSO.currentItemInUse as PBChassisSO;
            for (int i = 0; i < pBChassisSO.AllPartSlots.Count; i++)
            {
                botPartSlots.Add(pBChassisSO.AllPartSlots[i]);
                if (partSlot.GetPartTypeOfPartSlot() == pBChassisSO.AllPartSlots[i].PartType)
                {
                    partSlots.Add(pBChassisSO.AllPartSlots[i]);
                }
            }

            if (isSelect)
            {
                if (partSlots.Count > 1)
                {
                    var slot0IsEmpty = partSlots[0].PartVariableSO.value == null;
                    var slot1IsEmpty = partSlots[1].PartVariableSO.value == null;

                    if (slot0IsEmpty && slot1IsEmpty)
                    {
                        BuildRobot(Part, partSlots[0].PartSlotType);
                    }
                    else if (!slot0IsEmpty && slot1IsEmpty)
                    {
                        BuildRobot(Part, partSlots[1].PartSlotType);
                    }
                    else
                    {
                        BuildRobot(Part, partSlots[0].PartSlotType);
                    }
                }
                else
                {
                    BuildRobot(Part, partSlot);
                }
            }
            else
                BuildRobot(Part, partSlot);

        }


        if (partSlot.GetPartTypeOfPartSlot() == PBPartType.Body)
        {
            tempChassis = Part.Cast<PBChassisSO>();
        }
        tempChassis = null;

        if (FindPBChassisCR != null)
            StopCoroutine(FindPBChassisCR);
        FindPBChassisCR = FindPBChassis(botPartSlots, partSlot.GetPartTypeOfPartSlot());
        StartCoroutine(FindPBChassisCR);
    }


    void HandleOnTabButtonClick(params object[] parameters)
    {
        slotFollowingPart.Clear();
        gearSlotNumbers.Clear();
        partContainersPro.Clear();
        gearSlotNumberUIPros.Clear();

        PBPartType Part = PBPartType.Body;

        if (parameters[0] is PBPartSO partSO)
        {
            Part = partSO.PartType;
        }
        else if (parameters[0] is PBPartType partType)
        {
            Part = partType;
        }
        else if (parameters[0] is GearTabButton gearTabButton)
        {
            Part = gearTabButton.PartType;
        }

        if ((int)Part == Const.IntValue.Invalid)
        {
            return;
        }
        else if (Part == PBPartType.Body)
        {
            GenerateBody(chassisManagerSO.currentItemInUse, true);
            return;
        }

        List<BotPartSlot> botPartSlots = new List<BotPartSlot>();
        var chassisSOInstance = Instantiate(chassisManagerSO.currentItemInUse).Cast<PBChassisSO>();
        tempChassisSO.value = chassisSOInstance;
        for (int i = 0; i < chassisSOInstance.AllPartSlots.Count; i++)
        {
            var slotInstance = new BotPartSlot(chassisSOInstance.AllPartSlots[i].PartSlotType, ScriptableObject.CreateInstance<ItemSOVariable>());
            slotInstance.PartVariableSO.value = chassisSOInstance.AllPartSlots[i].PartVariableSO.value;
            chassisSOInstance.AllPartSlots[i] = slotInstance;
            botPartSlots.Add(slotInstance);
        }
        AttachWheelToChassis();
        void AttachWheelToChassis()
        {
            var wheelSlots = chassisSOInstance.AllPartSlots.FindAll(x => x.PartType == PBPartType.Wheels);
            for (int i = 0; i < wheelSlots.Count; i++)
            {
                var wheel = wheelSlots[i];
                wheel.PartVariableSO.value = i < chassisSOInstance.AttachedWheels.Count ? chassisSOInstance.AttachedWheels[i] : chassisSOInstance.AttachedWheels.Last();
                ((PBPartSO)wheel.PartVariableSO.value).IsEquipped = true;
            }
        }
        robot.BuildRobot();
        SendInfo();

        if (FindPBChassisCR != null)
            StopCoroutine(FindPBChassisCR);
        FindPBChassisCR = FindPBChassis(botPartSlots, pbPartTypeTabUI);
        StartCoroutine(FindPBChassisCR);
    }

    private IEnumerator FindPBChassis(List<BotPartSlot> botPartSlots, PBPartType pbPartTypes)
    {
        if (pbPartTypeTabUI != pbPartTypes) yield break;

        PBChassisSO chassisSOInstance = chassisManagerSO.currentItemInUse.Cast<PBChassisSO>();
        yield return new WaitUntil(() => robot.GetComponentInChildren<PBChassis>() != null);
        PBChassis pbChassis = robot.GetComponentInChildren<PBChassis>();

        for (int i = 0; i < botPartSlots.Count; i++)
        {
            //if (botPartSlots[i].PartVariableSO.value == null) { }
            if (botPartSlots[i].PartType == pbPartTypes)
            {
                PBPartSlot pbPartSlot = chassisSOInstance.AllPartSlots[i].PartSlotType;
                PBChassis.PartContainer partContainer = pbChassis.PartContainers.Find(v => v.PartSlotType.Equals(pbPartSlot));
                InfoBodyBot infoBodyBot = gameObject.GetComponentInChildren<InfoBodyBot>();
                if (infoBodyBot != null)
                {
                    if (infoBodyBot.InfoPart.ContainsKey(pbPartSlot))
                    {
                        GameObject gearSlotNumber = Instantiate(gearSlotNumberPrefab, pbChassis.transform);
                        gearSlotNumber.transform.localPosition = Vector3.zero;

                        GearSlotNumberUI gearSlotNumberUI = gearSlotNumber.GetComponent<GearSlotNumberUI>();
                        Vector3 posConvert = MainCameraFindCache.Get().WorldToScreenPoint(infoBodyBot.InfoPart[pbPartSlot].position);
                        gearSlotNumberUI.ValueSlot.SetText($"{GearSlotHelper.GetSlotIndex(pbPartSlot) + 1}");
                        gearSlotNumberUI.ObjectNumberSlot.transform.position = posConvert;

                        if (botPartSlots[i].PartVariableSO.value == null)
                            gearSlotNumberUI.PlayAnimation();

                        slotFollowingPart.Add(infoBodyBot.InfoPart[pbPartSlot]);
                        gearSlotNumberUIPros.Add(gearSlotNumberUI);
                        gearSlotNumbers.Add(gearSlotNumber);
                        partContainersPro.Add(partContainer);
                    }
                }
            }
        }
    }

    private void OnSwapPart()
    {
        GenerateBody(chassisManagerSO.currentItemInUse, true);
    }

    private void HandleMainSceneTabButtonClicked()
    {
        // if (_delayHideRobotPreviewCR != null)
        //     StopCoroutine(_delayHideRobotPreviewCR);
        // _delayHideRobotPreviewCR = DelayShowOrHideRobotPreview(0.5f, false);
        // StartCoroutine(_delayHideRobotPreviewCR);
    }
    private void HandleCharacterUITabButtonClicked()
    {
        // if (_delayHideRobotPreviewCR != null)
        //     StopCoroutine(_delayHideRobotPreviewCR);
        // _delayHideRobotPreviewCR = DelayShowOrHideRobotPreview(0, true);
        // StartCoroutine(_delayHideRobotPreviewCR);
    }

    private void BuildRobot(PBPartSO Part, PBPartSlot partSlot)
    {
        if (partSlot.GetPartTypeOfPartSlot() == PBPartType.Body)
        {
            if (Part == chassisManagerSO.currentItemInUse)
            {
                GenerateBody(chassisManagerSO.currentItemInUse, true);
                return;
            }
            GenerateBody(Part, false);
            return;
        }
        foreach (var slot in tempChassisSO.value.Cast<PBChassisSO>().AllPartSlots)
        {
            if (slot.PartSlotType.Equals(partSlot) == false) continue;
            slot.PartVariableSO.value = Part;
        }
        robot.BuildRobot();
        SendInfo();
    }


    private PBChassisSO _partSelect;
    private void GetIndexTab(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        if ((int)parameters[0] != 0)
            _partSelect = null;
    }
    void SelectGearSpecial(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        _partSelect = parameters[0] as PBChassisSO;
    }

    void SendInfo()
    {
        var TotalScore_1 = RobotStatsCalculator.CalStatsScore(robot.GetTotalHealth((PBChassisSO)tempChassisSO.value), robot.GetTotalATK((PBChassisSO)tempChassisSO.value));
        CallProperty((PBChassisSO)tempChassisSO.value, TotalScore_1);

        if (_partSelect != null)
        {
            if (_partSelect.IsSpecial)
            {
                var TotalScore_2 = RobotStatsCalculator.CalStatsScore(robot.GetTotalHealth(_partSelect), robot.GetTotalATK(_partSelect));
                //CallProperty(_partSelect, TotalScore_2);
            }
        }
    }
    private void CallProperty(PBChassisSO pbChassisSO, float totalScore)
    {
        GameEventHandler.Invoke(CharacterUIEventCode.OnSendBotInfo
        , robot.GetTotalHealth(pbChassisSO)
            , robot.GetTotalATK(pbChassisSO)
                , robot.GetAllPartPower(pbChassisSO)
                    , robot.GetChassisPower(pbChassisSO)
                        , totalScore);
    }

    public bool CheckEnoughPower(PBPartSO partSO, PBPartSlot partSlot)
    {
        var tempChassis = Instantiate((PBChassisSO)tempChassisSO.value);
        foreach (var slot in tempChassis.AllPartSlots)
        {
            if (slot.PartVariableSO.value == partSO && slot.PartSlotType != partSlot)
            {
                slot.PartVariableSO.value = GearSaver.Instance.GetCurrentPartSO(slot.PartSlotType);
            }
            if (slot.PartSlotType == partSlot && slot.PartVariableSO.value != partSO)
            {
                slot.PartVariableSO.value = partSO;
            }
        }
        bool isEnoughPower = robot.GetAllPartPower(tempChassis) <= robot.GetChassisPower(tempChassis);
        return isEnoughPower;
    }

    public bool CheckEnoughPower()
    {
        var tempChassis = (PBChassisSO)tempChassisSO.value;
        bool isEnoughPower = robot.GetAllPartPower(tempChassis) <= robot.GetChassisPower(tempChassis);
        return isEnoughPower;
    }

    private void OnBossUnlocking()
    {
        // if (_delayHideRobotPreviewCR != null)
        //     StopCoroutine(_delayHideRobotPreviewCR);
        // _delayHideRobotPreviewCR = DelayShowOrHideRobotPreview(0.5f, false);
        // StartCoroutine(_delayHideRobotPreviewCR);
    }

    private void OnUnlockBossSceneDisable()
    {
        // if (_delayHideRobotPreviewCR != null)
        //     StopCoroutine(_delayHideRobotPreviewCR);
        // _delayHideRobotPreviewCR = DelayShowOrHideRobotPreview(0, true);
        // StartCoroutine(_delayHideRobotPreviewCR);
    }

    // private IEnumerator DelayShowOrHideRobotPreview(float time, bool isShow)
    // {
    //     yield return new WaitForSeconds(time);
    //     if (robot != null)
    //     {
    //         //if (robot.ChassisInstance != null)
    //         //    robot.ChassisInstance.gameObject.SetActive(isShow);
    //     }
    // }

    private void OnClickTabButtonUI(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        pbPartTypeTabUI = (PBPartType)parameters[0];
    }
}
