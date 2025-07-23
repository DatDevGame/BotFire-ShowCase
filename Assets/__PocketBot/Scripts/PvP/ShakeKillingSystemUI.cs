using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using HyrphusQ.Events;
using System;

public class ShakeKillingSystemUI : MonoBehaviour
{
    public KillType KillType => _killType;

    [SerializeField] private KillType _killType;
    [SerializeField] private float _timeShakeDuration = 0.2f;
    [SerializeField] private List<RectTransform> _shakeGroupAObjects;
    [SerializeField] private List<RectTransform> _shakeGroupBObjects;

    private IEnumerator _shakeCR;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnRoundCompleted, DisableShake);
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnRoundCompleted, DisableShake);
    }

    public void Shake()
    {
        int indexGroup = _shakeGroupAObjects.Count == _shakeGroupBObjects.Count ? _shakeGroupAObjects.Count : 0;
        if (indexGroup == 0)
            return;
        if (!gameObject.activeSelf)
            return;
    
        if (_shakeCR != null)
            StopCoroutine(_shakeCR);
        _shakeCR = EnumeratorShake(indexGroup);
        StartCoroutine(_shakeCR);
    }

    private IEnumerator EnumeratorShake(int index)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);
        for (int i = 0; i < index; i++)
        {
            if (gameObject.activeSelf)
            {
                _shakeGroupAObjects[i].DOShakeAnchorPos(_timeShakeDuration, 10);
                _shakeGroupBObjects[i].DOShakeAnchorPos(_timeShakeDuration, 10);
            }
            yield return waitForSeconds;
        }
    }

    private void DisableShake()
    {
        int indexGroup = _shakeGroupAObjects.Count == _shakeGroupBObjects.Count ? _shakeGroupAObjects.Count : 0;
        for (int i = 0; i < indexGroup; i++)
        {
            _shakeGroupAObjects[i].DOKill();
            _shakeGroupBObjects[i].DOKill();
        }
    }

    private void OnDisable()
    {
        if (_shakeCR != null)
            StopCoroutine(_shakeCR);
    }
}
