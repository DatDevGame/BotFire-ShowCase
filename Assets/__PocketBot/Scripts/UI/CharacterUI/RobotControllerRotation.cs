using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class RobotControllerRotation : MonoBehaviour
{
    [SerializeField, BoxGroup("Property")] private float rotationSpeed = 5.0f;
    [SerializeField, BoxGroup("Property")] private Vector3 rotationStart;

    private RobotPreviewSpawner robotPreviewSpawner;

    private bool canDrag = false;
    private Vector3 lastMousePosition;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, HandleMainSceneTabButtonClicked);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, HandleMainSceneTabButtonClicked);
    }

    private void Start()
    {
        robotPreviewSpawner = FindObjectOfType<RobotPreviewSpawner>();
        if (robotPreviewSpawner != null)
            rotationStart = robotPreviewSpawner.transform.eulerAngles;
    }

    private void HandleMainSceneTabButtonClicked()
    {
        if(robotPreviewSpawner != null)
            robotPreviewSpawner.transform.DORotate(rotationStart, 0.7f);
    }

    private void Update()
    {
        if (canDrag)
        {
            RotateObject(Input.mousePosition);
            lastMousePosition = Input.mousePosition;
        }
    }

    private void RotateObject(Vector3 currentPosition)
    {
        Vector3 deltaPosition = currentPosition - lastMousePosition;
        float rotationY = deltaPosition.x * rotationSpeed * Time.deltaTime;
        robotPreviewSpawner.transform.Rotate(Vector3.up, -rotationY);
    }

    public void EnableCanDrag()
    {
        lastMousePosition = Input.mousePosition;
        canDrag = true;
    }
    public void DisableCanDrag() => canDrag = false;
}
