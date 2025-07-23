using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableMultipleTouch : MonoBehaviour
{
    void Awake()
    {
        Input.multiTouchEnabled = false;
    }
}
