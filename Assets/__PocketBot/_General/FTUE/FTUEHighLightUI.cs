using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;

public class FTUEHighLightUI : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] bool isCard;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleOnCharacterOpen);
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleOnCharacterOpen);
    }

    public void HandleOnCharacterOpen()
    {
        if (isCard) return;
        if (!FTUEMainScene.Instance.FTUE_Equip)
        {
            canvas = gameObject.AddComponent(typeof(Canvas)) as Canvas;
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 11;
            }
        }
        else { Destroy(canvas); Destroy(this); }
        StartCoroutine(CR_HandleOnCharacterOpen());
    }

    public void AddCanvas()
    {
        if (!FTUEMainScene.Instance.FTUE_Equip)
        {
            canvas = gameObject.AddComponent(typeof(Canvas)) as Canvas;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 11;
        }
        else { Destroy(canvas); Destroy(this); }
        StartCoroutine(CR_HandleOnCharacterOpen());
    }
    IEnumerator CR_HandleOnCharacterOpen()
    {
        yield return new WaitForSeconds(2.5f);
        GetComponent<MultiImageButton>().interactable = false;
        Destroy(canvas);
    }
}
