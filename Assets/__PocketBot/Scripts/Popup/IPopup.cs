using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPopup
{
    public event Action<IPopup> onPopupClosed;

    public abstract void Open();
}