using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class ArenaItem : MonoBehaviour
{
    [SerializeField] private Image _avatar;
    [SerializeField] private Image _outLine;


    public void SetUp(Sprite avatar)
    {
        _avatar.sprite = avatar;
    }
    public Sprite GetAvatar() => _avatar.sprite;
    public void ShowOutline()
    {
        _outLine.enabled = true;
        _outLine.color = new Color(_outLine.color.r, _outLine.color.g, _outLine.color.b, 0);
        _outLine.DOFade(1, AnimationDuration.TINY);
        _outLine.transform.localScale = Vector3.one * 1.5f;
        _outLine.transform.DOScale(1, AnimationDuration.TINY);
    }
}
