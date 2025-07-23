using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class HandleFTUECharacterUI : MonoBehaviour
{
    public static PBPartType partType_Equip_1 = PBPartType.Front;
    public static PBPartType partType_Equip_2 = PBPartType.Upper;
    public static PBPartSO partEquip1_FTUE;
    public static PBPartSO partEquip2_FTUE;

    [SerializeField, BoxGroup("Ref")] private GearListSelection gearListSelection;
    [SerializeField, BoxGroup("Ref")] private GearTabSelection gearTabSelection;
    [SerializeField, BoxGroup("Ref")] private ScrollRect gearListSelectionScrollRect;

    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable PPrefBoolVariable_Equip_1FTUE;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable PPrefBoolVariable_UpgradeFTUE;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable PPrefBoolVariable_BuildTabFTUE;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable PPrefBoolVariable_EquipUpperFTUE;
    [SerializeField, BoxGroup("FTUE")] private PBPartSO partEquipFront_FTUE;
    [SerializeField, BoxGroup("FTUE")] private PBPartSO partEquipUpper_FTUE;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickButtonCharacter);
        GameEventHandler.AddActionEvent(StateBlockBackGroundFTUE.Start, StartBlockBackGround);
        GameEventHandler.AddActionEvent(StateBlockBackGroundFTUE.End, EndBlockBackGround);
        GameEventHandler.AddActionEvent(HandleFTUECharacterUIEvent.ScrollToElement, ScrollToElement);

        partType_Equip_1 = !InitialWeaponsABTest.isActive ? PBPartType.Front : PBPartType.Upper;
        partType_Equip_2 = !InitialWeaponsABTest.isActive ? PBPartType.Upper : PBPartType.Front;
        partEquip1_FTUE = !InitialWeaponsABTest.isActive ? partEquipFront_FTUE : partEquipUpper_FTUE;
        partEquip2_FTUE = !InitialWeaponsABTest.isActive ? partEquipUpper_FTUE : partEquipFront_FTUE;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickButtonCharacter);
        GameEventHandler.RemoveActionEvent(StateBlockBackGroundFTUE.Start, StartBlockBackGround);
        GameEventHandler.RemoveActionEvent(StateBlockBackGroundFTUE.End, EndBlockBackGround);
        GameEventHandler.RemoveActionEvent(HandleFTUECharacterUIEvent.ScrollToElement, ScrollToElement);
    }

    private void OnClickButtonCharacter()
    {
        if (!PPrefBoolVariable_Equip_1FTUE.value)
            StartCoroutine(CheckEquipFTUE_1());
        else if (!PPrefBoolVariable_UpgradeFTUE.value && PPrefBoolVariable_Equip_1FTUE.value)
            StartCoroutine(CheckEquipFTUE_1());

        if (PPrefBoolVariable_BuildTabFTUE.value && !PPrefBoolVariable_EquipUpperFTUE.value)
        {
            StartCoroutine(CheckEquipFTUE_2());
        }
    }

    private IEnumerator CheckEquipFTUE_1()
    {
        GearTabButton gearTabButton_Equip_1 = gearTabSelection.GetAllSubTabButtons().Find(v => v.PartType == partType_Equip_1);
        gearTabSelection.DefaultReloadTabButton = gearTabButton_Equip_1;

        yield return new WaitUntil(() => gearListSelection.gearButtons.Count > 0);
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < gearListSelection.gearButtons.Count; i++)
        {
            if (gearListSelection.gearButtons[i].PartSO != partEquip1_FTUE)
                gearListSelection.gearButtons[i].Button.interactable = false;
            else
                gearListSelection.gearButtons[i].Button.interactable = true;
        }
    }

    private IEnumerator CheckEquipFTUE_2()
    {
        GearTabButton gearTabButton_Upper = gearTabSelection.GetAllSubTabButtons().Find(v => v.PartType == partType_Equip_2);
        gearTabSelection.DefaultReloadTabButton = gearTabButton_Upper;
        GameEventHandler.Invoke(LogFTUEEventCode.StartEquip_2);
        GameEventHandler.Invoke(FTUEEventCode.OnWaitEquipUpperFTUE, gearTabButton_Upper.gameObject);

        yield return new WaitUntil(() => gearListSelection.UnlockearButtons.Count > 0);
        GameEventHandler.Invoke(FTUEEventCode.OnWaitEquipUpperFTUE);
        GearButton gearButtonUnlockTheFirst = gearListSelection.UnlockearButtons[0];
        gearButtonUnlockTheFirst.upgradeBtn.interactable = false;
        GameEventHandler.Invoke(FTUEEventCode.OnEquipUpperFTUE, gearButtonUnlockTheFirst.gameObject);
        gearButtonUnlockTheFirst.StartFTUEEquipUpper();

        yield return new WaitUntil(() => PPrefBoolVariable_EquipUpperFTUE.value);
        gearButtonUnlockTheFirst.upgradeBtn.interactable = true;
        GameEventHandler.Invoke(LogFTUEEventCode.EndEquip_2);
        GameEventHandler.Invoke(FTUEEventCode.OnEquipUpperFTUECompleted);
        yield return null;
        RectMask2D rectMask2D = gearButtonUnlockTheFirst.GetComponentInParent<RectMask2D>();
        rectMask2D.enabled = false;
        rectMask2D.enabled = true;
    }

    private void StartBlockBackGround()
    {
        gearListSelectionScrollRect.enabled = false;
    }

    private void EndBlockBackGround()
    {
        if (PPrefBoolVariable_UpgradeFTUE.value)
            gearListSelectionScrollRect.enabled = true;
    }

    private void ScrollToElement(params object[] eventData)
    {
        GameObject element = eventData[0] as GameObject;
        var rectTransform = element.GetComponent<RectTransform>();
        gearListSelectionScrollRect.FocusOnItem(rectTransform);
        StartCoroutine(CommonCoroutine.WaitUntil(() => gearListSelectionScrollRect.content.rect.height > 0.1f, () =>
        {
            gearListSelectionScrollRect.FocusOnItem(rectTransform);
        }));
    }
}