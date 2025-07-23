using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarouselPanel : MonoBehaviour
{
    public Action<CarouselPanel, int> OnPurchased;
    public virtual bool isAvailable => true;
    public int index;
}
