using System.Collections;
using System.Collections.Generic;
using LatteGames;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class EZAnimClampVector3 : EZAnim<float>
{
    [SerializeField, BoxGroup("Specific")]
    protected Transform go;
    protected override void SetAnimationCallBack()
    {
        AnimationCallBack = t =>
        {
            go.transform.localScale = Vector3.one * Mathf.Max(Mathf.LerpUnclamped(from, to, t), 0);
        };
        base.SetAnimationCallBack();
    }
}
