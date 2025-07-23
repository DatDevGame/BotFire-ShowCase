using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class ABTestGroupDataSO : ScriptableObject
{
    public string group_name = "Group1";

    public string GroupName => group_name;
}