using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class EarnBoxProgressUI : MonoBehaviour
{
    static int previousCumulatedWinMatch;
    [SerializeField]
    PPrefIntVariable cumulatedWinMatch;
    [SerializeField]
    Image boxImg;
    [SerializeField]
    Sprite disableBoxSprite, enableBoxSprite;
    [SerializeField]
    ParticleSystem earnBoxFX;
    [SerializeField] List<Image> progressNodes;
    [SerializeField] bool isPlayAnim;

    private void Awake()
    {
        if (!isPlayAnim)
        {
            for (int i = 0; i < progressNodes.Count; i++)
            {
                var item = progressNodes[i];
                item.transform.localScale = i < cumulatedWinMatch.value ? Vector3.one : Vector3.zero;
            }
            return;
        }
        if (previousCumulatedWinMatch == progressNodes.Count - 1 && cumulatedWinMatch.value == 0)
        {
            StartCoroutine(CR_PlayEarnBoxFX());
            previousCumulatedWinMatch = cumulatedWinMatch.value;
        }
        else
        {
            for (int i = 0; i < progressNodes.Count; i++)
            {
                var item = progressNodes[i];
                item.transform.localScale = i < cumulatedWinMatch.value ? Vector3.one : Vector3.zero;
            }
            previousCumulatedWinMatch = cumulatedWinMatch.value;
        }
    }

    IEnumerator CR_PlayEarnBoxFX()
    {
        foreach (var item in progressNodes)
        {
            item.transform.localScale = Vector3.one;
        }
        boxImg.sprite = enableBoxSprite;
        yield return new WaitForSeconds(AnimationDuration.TINY);
        earnBoxFX.Play();
        yield return new WaitForSeconds(0.1f);
        boxImg.sprite = disableBoxSprite;
        for (int i = progressNodes.Count - 1; i >= 0; i--)
        {
            var item = progressNodes[i];
            item.transform.DOScale(Vector3.zero, AnimationDuration.TINY);
            yield return new WaitForSeconds(0.2f);
        }
    }
#if UNITY_EDITOR
    [Button]
    void PlayAnimation()
    {
        StartCoroutine(CR_PlayEarnBoxFX());
    }
#endif
}
