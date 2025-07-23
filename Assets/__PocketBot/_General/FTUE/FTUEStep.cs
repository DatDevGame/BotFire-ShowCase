using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class FTUEStep : MonoBehaviour
{
    [SerializeField] private PPrefBoolVariable _ftuesStep_1;
    [SerializeField] private PPrefBoolVariable _ftuesStep_2;
    [SerializeField] private GameObject _handFtue;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if(_ftuesStep_2 != null)
                _ftuesStep_2.value = true;
        });
    }

    private void Start()
    {
        _handFtue.SetActive(false);
        if (_ftuesStep_1.value && !_ftuesStep_2.value)
            _handFtue.SetActive(true);
    }
}
