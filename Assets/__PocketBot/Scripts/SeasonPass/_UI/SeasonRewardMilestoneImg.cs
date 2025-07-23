using System.Collections;
using System.Collections.Generic;
using LatteGames.Template;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonRewardMilestoneImg : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text unreachedIndexTxt;
    [SerializeField] TMP_Text reachedIndexTxt;
    [SerializeField] EZAnimBase imageAnim;
    [SerializeField] Sprite unreachedSprite;
    [SerializeField] Sprite reachedSprite;

    MileStoneState currentMileStoneState;
    int index;

    public int Index => index;

    public void Init(int index)
    {
        this.index = index;
        unreachedIndexTxt.text = index.ToString();
        reachedIndexTxt.text = index.ToString();
    }

    public void SetState(MileStoneState mileStoneState)
    {
        if (currentMileStoneState == mileStoneState)
        {
            return;
        }

        currentMileStoneState = mileStoneState;
        imageAnim.StopAllCoroutines();
        if (mileStoneState == MileStoneState.Reached)
        {
            image.sprite = reachedSprite;
            reachedIndexTxt.gameObject.SetActive(true);
            unreachedIndexTxt.gameObject.SetActive(false);
            imageAnim.SetToStart();
        }
        else if (mileStoneState == MileStoneState.Current)
        {
            image.sprite = reachedSprite;
            reachedIndexTxt.gameObject.SetActive(true);
            unreachedIndexTxt.gameObject.SetActive(false);
            if (gameObject.activeInHierarchy)
            {
                SoundManager.Instance.PlaySFX(PBSFX.UIClaimed);
                imageAnim.Play();
            }
            else
            {
                imageAnim.SetToEnd();
            }
        }
        else if (mileStoneState == MileStoneState.Unreached)
        {
            image.sprite = unreachedSprite;
            reachedIndexTxt.gameObject.SetActive(false);
            unreachedIndexTxt.gameObject.SetActive(true);
            imageAnim.SetToStart();
        }
    }
}

public enum MileStoneState : byte
{
    None,
    Reached,
    Current,
    Unreached
}
