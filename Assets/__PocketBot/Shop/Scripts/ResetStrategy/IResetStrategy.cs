using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResetStrategy
{
    public event Action onReset;
}