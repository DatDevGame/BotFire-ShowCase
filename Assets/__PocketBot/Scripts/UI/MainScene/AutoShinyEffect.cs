using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;
using Sirenix.OdinInspector;

public class AutoShinyEffect : ShinyEffectForUGUI
{
    public bool IsAutoShiny
    {
        get => isAutoShiny;
        set
        {
            isAutoShiny = value;
        }
    }

    [SerializeField, BoxGroup("Property")] private bool isReverse;
    [SerializeField, BoxGroup("Property")] private float timeShinyDuration;
    [SerializeField, BoxGroup("Property")] private float timeBetweenDuration;
    [SerializeField, BoxGroup("Property")] private bool isAutoShiny;

    [SerializeField, BoxGroup("Object")] private Graphic graphicItem;

    private bool m_IsWaitingNextShiny = false;
    private IEnumerator _shinyCR;
    private Sequence _shinyEffectSequence;

    protected override void OnDestroy()
    {
        _shinyEffectSequence.Kill();

        if (_shinyCR != null)
            StopCoroutine(_shinyCR);
    }

    protected override void OnEnable()
    {
        if (graphicItem != null)
            graphicItem.material = effectMaterial;

        if (_shinyCR != null)
            StopCoroutine(_shinyCR);
        _shinyCR = AutoShiny();
        StartCoroutine(_shinyCR);
    }

    protected override void Start()
    {
        _shinyCR = AutoShiny();
        StartCoroutine(_shinyCR);
    }

    public void PlayShiny()
    {
        m_IsWaitingNextShiny = true;
        _shinyEffectSequence?.Kill();
        if (isReverse)
        {
            TweenCallback<float> tweenCallbackReverse = (value) =>
            {
                location = value;
            };

            _shinyEffectSequence = DOTween.Sequence();
            _shinyEffectSequence.Append
                (
                     DOVirtual.Float(0, 1f, timeShinyDuration, tweenCallbackReverse)
                    .OnComplete(() =>
                    {
                        DOVirtual.Float(1, 0, timeShinyDuration, tweenCallbackReverse)
                           .OnComplete(() =>
                           {
                               m_IsWaitingNextShiny = false;
                               location = 0;
                           });
                    })
                );
            return;
        }

        TweenCallback<float> tweenCallback = (value) =>
        {
            location = value;
        };

        _shinyEffectSequence = DOTween.Sequence();
        _shinyEffectSequence.Append
            (
                DOVirtual.Float(0, 1f, timeShinyDuration, tweenCallback)
                .OnComplete(() =>
                {
                    m_IsWaitingNextShiny = false;
                    location = 0;
                })
            );
    }

    private IEnumerator AutoShiny()
    {
        WaitForSeconds waitForSecondsNextAction = new WaitForSeconds(timeShinyDuration + timeBetweenDuration);
        while (isAutoShiny)
        {
            if (!m_IsWaitingNextShiny)
                PlayShiny();

            yield return waitForSecondsNextAction;
        }
    }
}
