using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using LatteGames.GameManagement;
using GachaSystem.Core;
using LatteGames;
using LatteGames.UnpackAnimation;

public class FTUESpawnGacha : MonoBehaviour
{
    [SerializeField] GachaPack FTUEGacha;
    [SerializeField] PPrefBoolVariable FTUE_Lootbox;
    [SerializeField] GameObject eventSystem;
    [SerializeField] CanvasGroupVisibility textBox;
    [SerializeField] OpenPackAnimationSM openPackAnimationSM;

    private void Awake()
    {
        openPackAnimationSM = FindObjectOfType<OpenPackAnimationSM>(); 
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, HandleUnpackDone);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, HandleOnUnpackStart);
        openPackAnimationSM.OnMouseClicked += HandleOnUnpackStart;
        openPackAnimationSM.OnSkipButtonClicked += HandleOnUnpackStart;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, HandleUnpackDone);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, HandleOnUnpackStart);
        openPackAnimationSM.OnMouseClicked -= HandleOnUnpackStart;
        openPackAnimationSM.OnSkipButtonClicked -= HandleOnUnpackStart;
    }

    void Start()
    {
        FTUE_Lootbox.value = true;
        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, null, new List<GachaPack>() { FTUEGacha }, null);
        StartCoroutine(CR_Start());
    }

    IEnumerator CR_Start()
    {
        textBox.Show();
        yield return new WaitForSeconds(0.5f);
        eventSystem.SetActive(true);
    }

    void HandleOnUnpackStart()
    {
        textBox.Hide();
    }

    void HandleUnpackDone()
    {
        textBox.Hide();
        SceneManager.LoadScene(SceneName.MainScene, isPushToStack: false);
    }
}
