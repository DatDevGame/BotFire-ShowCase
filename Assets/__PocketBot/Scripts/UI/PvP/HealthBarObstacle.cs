using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HealthBarObstacle : MonoBehaviour
{
    [SerializeField] private float _timeDisableEveryCollider = 5;
    [SerializeField] private CanvasGroup canvasGroupFilled;
    [SerializeField] private SlicedFilledImage _filler;
    [SerializeField] private float speed;
    [SerializeField] private Vector3 _offset;

    private float _timeDelayShow;
    private Transform _obstacle;
    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = MainCameraFindCache.Get();
    }

    public void SetUp(Transform obstacle)
    {
        _obstacle = obstacle;
    }

    public void SetFilledAmount(float fillAmount)
    {
        if (_filler == null) return;

        if (canvasGroupFilled.alpha < 1)
            canvasGroupFilled.DOFade(1, 0.3f);

        _timeDelayShow = _timeDisableEveryCollider;
        _filler.fillAmount = fillAmount;
    }

    private void LateUpdate()
    {
        if (_obstacle == null) return;

        _timeDelayShow -= Time.deltaTime;
        if (_timeDelayShow <= 0 && canvasGroupFilled.alpha >= 1)
            canvasGroupFilled.DOFade(0, 0.3f);

        var targetPos = _mainCam.WorldToScreenPoint(_obstacle.position + _offset);
        transform.position = Vector3.Lerp(transform.position, targetPos, speed);
    }
}
