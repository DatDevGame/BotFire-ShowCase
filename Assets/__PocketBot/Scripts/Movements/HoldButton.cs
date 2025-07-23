using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : Button, IPointerDownHandler, IPointerUpHandler
{
    public event Action OnButtonHold = delegate { };
    public event Action OnButtonRelease = delegate { };

    private bool isHolding;

    protected override void OnDisable()
    {
        base.OnDisable();
        if (isHolding)
        {
            isHolding = false;
            OnButtonRelease();
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        isHolding = true;
        OnButtonHold();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        isHolding = false;
        OnButtonRelease();
    }
}
