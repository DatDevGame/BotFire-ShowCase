using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InitialWeaponsABTest : MonoBehaviour
{
    public static bool isActive;

    [SerializeField] private ShopProductSO pack_FTUE;

    private void Awake()
    {
        isActive = pack_FTUE.generalItems.Keys.ToList().Find(x => x is PBPartSO part && part.PartType == PBPartType.Front) == null;
    }
}
