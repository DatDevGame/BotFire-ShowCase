using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentGearHolder : Singleton<CurrentGearHolder>
{
    public PBPartSO currentSO;
    public PBPartSlot currentSlot;

    public PBPartSO swapSO;
    public PBPartSlot swapSlot;
}
