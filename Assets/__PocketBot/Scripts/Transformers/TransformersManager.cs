using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class TransformersManager : Singleton<TransformersManager>
{
    //[SerializeField, BoxGroup("Data Select")] public PlayerInfoVariable select_InfoOfBossSelect;
    [SerializeField, BoxGroup("Ref")] private GameObject _transformerHolder;
    [SerializeField, BoxGroup("Ref")] private GameObject _contentRoot;
    [SerializeField, BoxGroup("Assets")] private PBModelRenderer _modelRenderer;
    [SerializeField, BoxGroup("Config")] private AIBotInfo _placeHolderBotInfo;
    [SerializeField, BoxGroup("Data Select")] private PB_AIProfile _placeHolderAIProfile;
    [SerializeField, BoxGroup("Data Select")] private ItemSOVariable _transformerChassisPreview;
    [SerializeField, BoxGroup("Data Select")] private PBRobotStatsSO _transformerRobotStatsSO; // Only need to give current chassisSO
    [SerializeField, BoxGroup("Data Select")] private PlayerInfoVariable _selectInfoTransformerVariable;

    private GarageSO _currentGarageSO;
    private GarageModelHandle _loadedGarageModel;
    private GameObject _previewTransformerGO;

    private void Awake()
    {
        HideTransformerPreview();
        _contentRoot.SetActive(false);
    }
    
    public void ShowTransformerPreview(GarageSO garageSO, PBChassisSO transformerChassisSO)
    {
        GameEventHandler.Invoke(TransformerPreviewEvent.OnPreviewShowed);        
        SpawnGarage(garageSO);
        SpawnTransformer(transformerChassisSO);
        _contentRoot.SetActive(false);
    }

    private void SpawnGarage(GarageSO garageSO)
    {
        if (garageSO == null) return;
        if (_currentGarageSO != garageSO)
        {
            if ( _loadedGarageModel != null)
            {
                Destroy(_loadedGarageModel.gameObject);
                _loadedGarageModel = null;
            }
            _currentGarageSO = garageSO;
            _loadedGarageModel = Instantiate(_currentGarageSO.Room, _contentRoot.transform);
        }
    }

    private void SpawnTransformer(PBChassisSO transformerChassisSO)
    {
        if (_previewTransformerGO != null)
        {
            Destroy(_previewTransformerGO);
            _previewTransformerGO = null;
        }
        if (transformerChassisSO == null) return;
        _transformerChassisPreview.value = transformerChassisSO;
        _selectInfoTransformerVariable.value = new PBBotInfo(_placeHolderBotInfo, _transformerRobotStatsSO, _placeHolderAIProfile);
        PBModelRenderer instance = Instantiate(_modelRenderer, _transformerHolder.transform);
        instance.SetInfo(_selectInfoTransformerVariable);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        instance.BuildRobot(false); //True or false is not important just a placeholder
        //instance.ChassisInstance.RobotBaseBody.isKinematic = true;
        _previewTransformerGO = instance.gameObject;
    }

    public void HideTransformerPreview()
    {
        GameEventHandler.Invoke(TransformerPreviewEvent.OnPreviewHiden);
        _contentRoot.SetActive(false);
    }
}



[EventCode]
public enum TransformerPreviewEvent
{ 
    OnPreviewShowed,
    OnPreviewHiden,
}