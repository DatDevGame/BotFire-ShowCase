using UnityEngine;
using DG.Tweening;

public class NoAdsIconAnimation : MonoBehaviour
{
    private void Start()
    {
        StartSwaying();
    }

    private void StartSwaying()
    {
        transform.DORotate(new Vector3(0, 0, 15), 0.4f)
            .SetEase(Ease.InOutSine)
            .SetLoops(4, LoopType.Yoyo)
            .OnComplete(StartBreathing);
    }

    private void StartBreathing()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(1.1f, 0.4f).SetEase(Ease.InOutQuad))
           .Append(transform.DOScale(1f, 0.4f).SetEase(Ease.InOutQuad))
           .SetLoops(1, LoopType.Yoyo)
           .OnComplete(StartSwaying);
    }
}
