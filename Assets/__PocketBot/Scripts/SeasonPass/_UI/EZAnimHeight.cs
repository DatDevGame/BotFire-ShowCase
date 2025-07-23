using System.Collections;
using System.Collections.Generic;
using LatteGames;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EZAnimHeight : EZAnim<float>
{
    [SerializeField, BoxGroup("Specific")]
    public UnityEvent updateEvent;
    [SerializeField, BoxGroup("Specific")]
    protected RectTransform rectTransform;

    protected override void SetAnimationCallBack()
    {
        AnimationCallBack = t =>
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Lerp(from, to, t));
            updateEvent?.Invoke();
        };
        base.SetAnimationCallBack();
    }
}
