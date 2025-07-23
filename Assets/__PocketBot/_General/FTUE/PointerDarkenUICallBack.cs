using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDarkenUICallBack : MonoBehaviour
{
    private Action callBackAction;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            callBackAction.Invoke();
            Destroy(gameObject);
        }
    }

    public void SetupPointerAction(Action callBack)
    {
        callBackAction = callBack;
    }
}
