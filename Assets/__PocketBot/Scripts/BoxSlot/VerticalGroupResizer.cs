using System.Collections;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using UnityEngine;

public class VerticalGroupResizer : MonoBehaviour
{
    [SerializeField] RectTransform targetRectTransform;
    [SerializeField] SerializedDictionary<RectTransform, float> elements;

    float originalHeight = 0f;

    private void Awake()
    {
        originalHeight = targetRectTransform.sizeDelta.y;
    }

    public void UpdateSize()
    {
        var currentHeight = originalHeight;
        foreach (var element in elements)
        {
            if (!element.Key.gameObject.activeInHierarchy)
            {
                currentHeight -= element.Value;
            }
        }
        targetRectTransform.sizeDelta = new Vector2(targetRectTransform.sizeDelta.x, currentHeight);
    }
}
