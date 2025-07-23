using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class ColorGearButton : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    public void Show()
    {
        if (!FTUEMainScene.Instance.FTUEUpgrade.value) return;
        rectTransform.DOLocalMoveY(0, 0.5f);
    }
    public void Hide()
    {
        rectTransform.DOLocalMoveY(-2000f, 0.5f);
    }
}
