using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GearSlotNumberUI : MonoBehaviour
{
    [SerializeField] private DOTweenAnimation badgeAnimation;
    [SerializeField] private RectTransform objectNumberSlot;
    [SerializeField] private TMP_Text valueSlot;

    public RectTransform ObjectNumberSlot => objectNumberSlot;
    public TMP_Text ValueSlot => valueSlot;

    public void PlayAnimation()
    {
        badgeAnimation.DOPlay();
    }
}
