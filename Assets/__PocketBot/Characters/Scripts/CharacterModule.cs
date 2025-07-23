using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModule : ItemModule
{
    [SerializeField] protected string gender;
    [SerializeField] protected int age;
    [SerializeField] protected string height;

    public virtual string Gender
    {
        get => gender;
        set => gender = value;
    }

    public virtual int Age
    {
        get => age;
        set => age = value;
    }

    public virtual string Height
    {
        get => height;
        set => height = value;
    }
}
