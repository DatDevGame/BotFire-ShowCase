using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NodeEnergy : MonoBehaviour
{
    [SerializeField] private GameObject cell;
    [SerializeField] private Image cellImage;

    public void SetColor(Color color) => cellImage.color = color;
    public void OnEnergy() => cell.gameObject.SetActive(true);
    public void OffEnergy() => cell.gameObject.SetActive(false);
    public void OnAnimation()
    {
        transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutSine);
            });
    }
}
