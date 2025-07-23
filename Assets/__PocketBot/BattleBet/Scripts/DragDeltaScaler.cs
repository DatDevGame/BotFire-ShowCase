using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragDeltaScaler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _viewport;
    [SerializeField] private float _multiplier = 1;
    [SerializeField] private UnityEvent<PointerEventData> _onBeginDragEvents;
    [SerializeField] private UnityEvent<PointerEventData> _onDragEvents;
    [SerializeField] private UnityEvent<PointerEventData> _onDragEndEvents;
    private Vector2 _pointerStartLocalCursor;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _pointerStartLocalCursor = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport, eventData.position, eventData.pressEventCamera, out _pointerStartLocalCursor);
        _onBeginDragEvents?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localCursor;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport, eventData.position, eventData.pressEventCamera, out localCursor);
        var pointerDelta = localCursor - _pointerStartLocalCursor;
        // Re-calculate current pointer position for scaling drag-delta
        localCursor = _pointerStartLocalCursor + _multiplier * pointerDelta;
        eventData.position = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, _viewport.transform.TransformPoint(localCursor));

        _onDragEvents?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _onDragEndEvents?.Invoke(eventData);
    }
}
