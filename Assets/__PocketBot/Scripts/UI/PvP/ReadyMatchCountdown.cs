using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames.Template;
using System;

public class ReadyMatchCountdown : MonoBehaviour
{
    [SerializeField] TMP_Text readyTMP;
    [SerializeField] string readyString;
    [SerializeField] string setString;
    [SerializeField] string fightString;
    [SerializeField] bool turnOffSFX;
    [SerializeField] bool isFTUE;

    private void Start()
    {
        StartCountdownLevel();
    }

    IEnumerator CR_CountdownLevel(bool isIgnoreTimeScale = false, bool raiseEventLevelStart = true, Action callback = null)
    {
        var textIndex = 3;
        var waitForSeconds = new WaitForSeconds(Const.PvPValue.StartMatchCountdownTime / 3f);
        var waitForSecondsRealtime = new WaitForSecondsRealtime(Const.PvPValue.StartMatchCountdownTime / 3f);
        if (!turnOffSFX)
            SoundManager.Instance.PlaySFX(PBSFX.UIReadySetFight);
        StartCoroutine(CR_CountdownToFightSound(isIgnoreTimeScale));
        do
        {
            readyTMP.transform.localScale = Vector3.one * 0.5f;
            readyTMP.text = GetString(textIndex);
            readyTMP.transform.DOScale(Vector3.one, 0.5f).SetUpdate(isIgnoreTimeScale);
            yield return isIgnoreTimeScale ? waitForSecondsRealtime : waitForSeconds;
            textIndex--;
        } while (textIndex > 0);
        readyTMP.enabled = false;
        if (isFTUE)
        {
            yield break;
        }
        callback?.Invoke();
        if (raiseEventLevelStart)
            GameEventHandler.Invoke(PBLevelEventCode.OnLevelStart);
    }

    IEnumerator CR_CountdownToFightSound(bool isIgnoreTimeScale)
    {
        yield return isIgnoreTimeScale ? new WaitForSecondsRealtime(2f) : new WaitForSeconds(2f);
        GameEventHandler.Invoke(PBPvPEventCode.OnCountToFightSFX);
    }

    string GetString(int textIndex)
    {
        if (textIndex == 2)
        {
            GameEventHandler.Invoke(PBPvPEventCode.OnSwitchFromReadyCamToFightCam);
        }
        return textIndex switch
        {
            3 => readyString,
            2 => setString,
            1 => fightString,
            _ => ""
        };
    }

    public void StartCountdownLevel(bool isIgnoreTimeScale = false, bool raiseEventLevelStart = true, Action callback = null)
    {
        readyTMP.enabled = true;
        StartCoroutine(CR_CountdownLevel(isIgnoreTimeScale, raiseEventLevelStart, callback));
    }
}
