using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayBuildVersionUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_BuildVersionText;

    private void Awake()
    {
        m_BuildVersionText.SetText($"v{Application.version}_{PBRemoteConfigManager.GetGroupName()}");
    }
}