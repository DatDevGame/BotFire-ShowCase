using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GachaSystem.Core;

[CreateAssetMenu(menuName = "PocketBots/Gacha/Card/Part")]
public class GachaCard_Part : GachaCard_GachaItem
{
    public virtual PBPartSO PartSO => GachaItemSO.Cast<PBPartSO>();
}