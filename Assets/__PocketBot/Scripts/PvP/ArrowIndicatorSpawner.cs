using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using Unity.VisualScripting;
using Cinemachine;

public class ArrowIndicatorSpawner : MonoBehaviour
{
    [SerializeField] GameObject arrowIndicatorPrefab;
    [SerializeField] Transform playerTransform;
    [SerializeField] List<ArrowIndicator> arrowList = new();
    [SerializeField] PBRobot playerRobot;
    [SerializeField] private Camera _mainCamera;
    void Awake()
    {
        _mainCamera = ObjectFindCache<CinemachineBrain>.Get().GetComponent<Camera>();

        //TODO: Hide IAP & Popup
        //GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
    }

    void OnDestroy()
    {
        //TODO: Hide IAP & Popup
        //GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
    }

    void HandleBotModelSpawned(params object[] parameters)
    {
        if (this != null && !gameObject.activeInHierarchy)
            return;
        if (parameters[0] is PBRobot pbBot)
        {
            if (pbBot.PersonalInfo.isLocal)
            {
                playerRobot = pbBot;
                StartCoroutine(CR_SetFollowObject());
            }
            else
            {
                GameObject arrowClone = Instantiate(arrowIndicatorPrefab);
                ArrowIndicator _arrowIndicator = arrowClone.GetComponent<ArrowIndicator>();
                if (_mainCamera != null)
                    _arrowIndicator.SetCamera(_mainCamera);
                PBChassis chassis = pbBot.ChassisInstance;
                Transform playerChild_1 = chassis.transform.GetChild(0);
                Transform playerChild_2 = playerChild_1.transform.GetChild(0);

                playerChild_2.AddComponent<NotifyOnVisible>();


                arrowList.Add(_arrowIndicator);
                _arrowIndicator.followObject = playerTransform;
                _arrowIndicator.target = playerChild_2.transform;
                _arrowIndicator.hasInit = true;
            }
        }
    }

    IEnumerator CR_SetFollowObject()
    {
        yield return new WaitUntil(() => arrowList.Count > 0);
        foreach (var item in arrowList)
        {
            if (item == null) yield break;
            PBChassis chassis = playerRobot.ChassisInstance;
            Transform playerChild_1 = chassis.transform.GetChild(0);
            Transform playerChild_2 = playerChild_1.transform.GetChild(0);
            item.transform.position = playerChild_2.transform.position;
            item.followObject = playerChild_2.transform;
        }
    }
}
