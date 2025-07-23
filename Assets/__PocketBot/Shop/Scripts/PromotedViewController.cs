using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PromotedViewController : MonoBehaviour
{
    [SerializeField] List<GameObject> promotedElements;
    [SerializeField] RectTransform rewardTxt;
    [SerializeField] Vector2 rewardTxtYPos;

    [Button]
    public void EnablePromotedView(bool isEnable)
    {
        if (isEnable)
        {
            foreach (var item in promotedElements)
            {
                item.SetActive(true);
            }
            rewardTxt.anchoredPosition = new Vector2(rewardTxt.anchoredPosition.x, rewardTxtYPos.y);
        }
        else
        {
            foreach (var item in promotedElements)
            {
                item.SetActive(false);
            }
            rewardTxt.anchoredPosition = new Vector2(rewardTxt.anchoredPosition.x, rewardTxtYPos.x);
        }
    }
}
