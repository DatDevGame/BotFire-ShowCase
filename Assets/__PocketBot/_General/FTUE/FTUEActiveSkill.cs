using System.Collections;
using System.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class FTUEActiveSkill : MonoBehaviour
{
    // [SerializeField] FTUEDoubleTapToReverse _FTUEDoubleTapToReverse;
    // [SerializeField] CanvasGroupVisibility canvasGroupVisibility;
    // [SerializeField] ActiveSkillButton activeSkillButton;
    // [SerializeField] Transform hand;

    // private IEnumerator Start()
    // {
    //     yield return new WaitUntil(() => PBRobot.allFightingRobots.Count > 0);
    //     var player = PBRobot.allFightingRobots.ToList().Find(x => x.PersonalInfo.isLocal);
    //     // TODO: Update active skill FTUE
    //     // if (player.ChassisInstance.ChassisSO.ActiveSkillSO != null)
    //     // {
    //     //     FTUEDoubleTapToReverse.OnFinishedFTUE += OnFinishedReverseFTUE;
    //     //     activeSkillButton.transform.localScale = Vector3.zero;
    //     // }
    // }

    // private void OnDestroy()
    // {
    //     FTUEDoubleTapToReverse.OnFinishedFTUE -= OnFinishedReverseFTUE;
    // }

    // void OnFinishedReverseFTUE()
    // {
    //     StartCoroutine(CR_PlayTutorial());
    // }

    // IEnumerator CR_PlayTutorial()
    // {
    //     #region FTUE Event
    //     GameEventHandler.Invoke(LogFTUEEventCode.StartUseActiveSkill);
    //     #endregion

    //     bool hasPerformedSkill = false;
    //     activeSkillButton.IsCheckPerformSkillCondition = false;
    //     activeSkillButton.OnPerformSkill += OnPerformSkill;
    //     activeSkillButton.transform.DOScale(Vector3.one, AnimationDuration.SSHORT).SetUpdate(true).SetEase(Ease.OutBack).OnComplete(() =>
    //     {
    //         canvasGroupVisibility.Show();
    //         hand.transform.position = activeSkillButton.transform.position;
    //     });
    //     yield return new WaitForSeconds(AnimationDuration.TINY);
    //     _FTUEDoubleTapToReverse.DarkenScene(true);
    //     _FTUEDoubleTapToReverse.Pause(true);
    //     yield return new WaitUntil(() => hasPerformedSkill);

    //     #region FTUE Event
    //     GameEventHandler.Invoke(LogFTUEEventCode.EndUseActiveSkill);
    //     #endregion

    //     _FTUEDoubleTapToReverse.DarkenScene(false);
    //     _FTUEDoubleTapToReverse.Pause(false);
    //     canvasGroupVisibility.Hide();


    //     void OnPerformSkill()
    //     {
    //         activeSkillButton.OnPerformSkill -= OnPerformSkill;
    //         activeSkillButton.IsCheckPerformSkillCondition = true;
    //         hasPerformedSkill = true;
    //     }
    // }
}
