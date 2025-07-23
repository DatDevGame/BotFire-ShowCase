using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescriptionItemModule : ItemModule
{
    [SerializeField]
    protected string description;

    public virtual string Description
    {
        get => description;
        set => description = value;
    }
}