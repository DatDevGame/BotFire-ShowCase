using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CustomInspectorName("SkinUnlockableItemModule")]
public class SkinUnlockableItemModule : UnlockableItemModule
{
    public static bool IsUnlockAll
    {
        get
        {
            return PlayerPrefs.GetInt("UNLOCK_ALL_SKIN", 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("UNLOCK_ALL_SKIN", value ? 1 : 0);
        }
    }
    public override bool isUnlocked
    {
        get
        {
            if (IsUnlockAll)
            {
                return true;
            }
            else
            {
                return base.isUnlocked;
            }
        }
        protected set
        {
            base.isUnlocked = value;
        }
    }
}