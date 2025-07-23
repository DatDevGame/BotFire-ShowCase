using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatUnlockAllParts : MonoBehaviour
{
    [SerializeField]
    private Button unlockAllButton;
    [SerializeField]
    private List<PBPartManagerSO> partManagerSOs;

    private void Awake()
    {
        unlockAllButton.onClick.AddListener(() =>
        {
            foreach (var partManagerSO in partManagerSOs)
            {
                foreach (var partSO in partManagerSO.items)
                {
                    partSO.TryUnlockIgnoreRequirement();
                }
            }
        });
    }

    private void Start()
    {
        PBRemoteConfigManager.NotifyEventRemoteConfigReady();
    }
}