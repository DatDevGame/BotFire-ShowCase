using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIOptimizer : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private List<Canvas> _allCanvasInChildren = new();
    [SerializeField, BoxGroup("Ref")] private List<RectMask2D> _allRectMask2DInChildren = new();

    private bool _isEnabled = true;

    [Button]
    public void Init()
    {
        //Don't call this function if your UI is not dynamically created. If those performance-intense object is static in yout UI, bind it in editor is ok.
        _allRectMask2DInChildren.Clear();
        foreach(RectMask2D rectMask2D in GetComponentsInChildren<RectMask2D>(true))
            if (rectMask2D.enabled)
                _allRectMask2DInChildren.Add(rectMask2D);
        _allCanvasInChildren.Clear();
        foreach(Canvas canvas in GetComponentsInChildren<Canvas>(true))
            if (canvas.enabled)
                _allCanvasInChildren.Add(canvas);
    }

    [Button]
    public void DisableUnnecessary()
    {
        if (!_isEnabled)
            return;
        foreach(Canvas canvas in _allCanvasInChildren)
            canvas.enabled = false;
        foreach(RectMask2D rectMask2D in _allRectMask2DInChildren)
            rectMask2D.enabled = false;
        _isEnabled = false;
    }

    [Button]
    public void EnableAll()
    {   
        if (_isEnabled)
            return;
        foreach(Canvas canvas in _allCanvasInChildren)
            canvas.enabled = true;
        foreach(RectMask2D rectMask2D in _allRectMask2DInChildren)
            rectMask2D.enabled = true;
        _isEnabled = true;
    }
}
