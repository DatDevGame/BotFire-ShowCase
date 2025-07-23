using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using UnityEngine.UI;


public enum ArrowRotate
{
    Left,
    Right,
    Stop
}
public class ArrowRotateCharacterUI : MonoBehaviour
{
    public void RotateLeft() => GameEventHandler.Invoke(ArrowRotate.Left);
    public void RotateRight() => GameEventHandler.Invoke(ArrowRotate.Right);
    public void StopRotate() => GameEventHandler.Invoke(ArrowRotate.Stop);
}
