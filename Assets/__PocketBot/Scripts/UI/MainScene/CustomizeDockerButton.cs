using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;

public class CustomizeDockerButton : DockerButton
{
    [SerializeField] Vector2 selectedSize;
    [SerializeField] Vector2 unSelectedSize;
    [SerializeField] Vector2 buttonIconScale;
    [SerializeField] Vector2 buttonIconY;

    [SerializeField] RectTransform rectTransform;
    // [SerializeField] Sprite r16;
    // [SerializeField] Sprite whiteBox;
    [SerializeField] Sprite selectSprite;
    [SerializeField] Image mainImage;
    [SerializeField] Color firstColor;
    [SerializeField] Color selectedInsideColor;
    [SerializeField] Color unselectedInsideColor;
    // [SerializeField] Material selectMaterial;
    // [SerializeField] GameObject Screws;
    [SerializeField] private EZAnimSequence eZAnimSequence;

    public override void Select(bool isForceSelect = false)
    {
        displayText.gameObject.SetActive(true);
        layoutElement.DOMinSize(selectedSize, AnimationDuration.TINY);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, selectedSize.y);
        // mainImage.sprite = r16;
        mainImage.color = selectedColor;
        buttonIcon.sprite = selectedSprite;

        //buttonIcon.transform.DOScale(buttonIconScale.y, AnimationDuration.TINY);

        //buttonIcon.transform.DOLocalMoveY(buttonIconY.y, AnimationDuration.TINY);

        insideImage.DOColor(selectedInsideColor, AnimationDuration.TINY);
        insideImage.sprite = selectSprite;
        insideImage.type = Image.Type.Simple;
        insideImage.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 230);
        // insideImage.material = selectMaterial;

        // Screws.SetActive(true);

        onClickEvent?.Invoke();
        eZAnimSequence.Play();
    }

    public override void Deselect()
    {

        displayText.gameObject.SetActive(false);
        layoutElement.DOMinSize(unSelectedSize, AnimationDuration.TINY);
        rectTransform.sizeDelta = new Vector2(unSelectedSize.y, unSelectedSize.y);
        // mainImage.sprite = whiteBox;
        mainImage.color = firstColor;
        buttonIcon.sprite = normalSprite;

        //buttonIcon.transform.DOScale(buttonIconScale.x, AnimationDuration.TINY);

        //buttonIcon.transform.DOLocalMoveY(buttonIconY.x, AnimationDuration.TINY);

        insideImage.DOColor(unselectedInsideColor, AnimationDuration.TINY);
        insideImage.sprite = null;
        insideImage.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 220);
        // insideImage.material = null;

        // Screws.SetActive(false);

        onDeselectEvent?.Invoke();
        eZAnimSequence.InversePlay();
    }

    public void SetOverrideIcon(Sprite sprite)
    {
        normalSprite = sprite;
        selectedSprite = sprite;
        buttonIcon.sprite = normalSprite;
    }
}
