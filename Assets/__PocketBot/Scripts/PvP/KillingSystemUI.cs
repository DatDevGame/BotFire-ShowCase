using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LatteGames;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.SerializedDataStructure;

public class KillingSystemUI : MonoBehaviour
{
    [SerializeField] TMP_Text titleTxt;
    [SerializeField] float showingTime = 2;
    [SerializeField] private SerializedDictionary<string, ShakeKillingSystemUI> _shakeKillingSystemUIs;

    Coroutine waitToHideCoroutine;
    private GameObject _killTitleOld;
    public void Show(string title)
    {
        if (_killTitleOld != null)
            _killTitleOld.SetActive(false);

        if (_shakeKillingSystemUIs[title] == null && !_shakeKillingSystemUIs[title].gameObject.activeSelf) return;

        _shakeKillingSystemUIs[title].gameObject.SetActive(true);
        _shakeKillingSystemUIs[title].gameObject.transform.localScale = Vector3.zero;
        _shakeKillingSystemUIs[title].gameObject.transform.DOKill();
        _killTitleOld = _shakeKillingSystemUIs[title].gameObject;

        if (waitToHideCoroutine != null)
        {
            StopCoroutine(waitToHideCoroutine);
        }
        _shakeKillingSystemUIs[title].gameObject.transform.DOScale(Vector3.one * 0.7f, AnimationDuration.SSHORT).SetEase(Ease.OutBack).OnComplete(() =>
        {
            if (transform.gameObject.activeInHierarchy)
                _shakeKillingSystemUIs[title].Shake();

            if (transform.gameObject.activeInHierarchy)
            {
                waitToHideCoroutine = StartCoroutine(CommonCoroutine.Delay(showingTime, false, () =>
                {
                    if(transform.gameObject.activeInHierarchy)
                        _shakeKillingSystemUIs[title].gameObject.SetActive(false);
                }));
            }
        });
    }
}
