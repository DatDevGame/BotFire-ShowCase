using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public class TankMaterialWheel
{
    public Renderer Renderer;
    public int Index;
}
public class WheelTankHandle : MonoBehaviour
{
    [SerializeField]
    private int dirWheelTank = -1;

    [SerializeField]
    private int wheelsSubMeshIndex;
    [SerializeField]
    private List<TankMaterialWheel> wheelsRenderers;

    private IEnumerator _wheelTankCR;
    private IEnumerator _aiWheelTankCR;
    private bool _isRun = false;
    private float _offsetTilling = 0;
    private AIBotController _aiBotController;
    [ShowInInspector] private List<Material> _wheelTankMaterials;

    private void Start()
    {
        StartCoroutine(StartObj());
    }

    private IEnumerator StartObj()
    {
        _aiBotController = gameObject.GetComponentInParent<AIBotController>();
        if (_aiBotController != null)
        {
            _aiWheelTankCR = RunWheelTankAI();
            StartCoroutine(_aiWheelTankCR);
        }
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, EndRound);

        yield return new WaitForSeconds(3f);
        _wheelTankMaterials = wheelsRenderers
            .Where(r => r != null && r.Renderer.materials.Length > wheelsSubMeshIndex)
            .Select(r => r.Renderer.materials[r.Index])
            .ToList();
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, EndRound);
    }

    private void EndRound(params object[] parameters)
    {
        if (_aiWheelTankCR != null)
            StopCoroutine(_aiWheelTankCR);

        StopWheel();
    }

    private Coroutine slowdownCoroutine;
    private float currentVelocity = 0f;

    public void RunWheelJoyStick(float offset)
    {
        if (offset != 0)
        {
            offset = InverseProximityToZero(offset);
            _offsetTilling += (Mathf.Abs(offset));

            if (_wheelTankMaterials != null && _wheelTankMaterials.Count > 0)
                _wheelTankMaterials.ForEach((v) =>
                {
                    v.mainTextureOffset = new Vector2(0, _offsetTilling * dirWheelTank);
                }); 

            if (slowdownCoroutine != null)
            {
                StopCoroutine(slowdownCoroutine);
                slowdownCoroutine = null;
            }
        }
        else
        {
            if (slowdownCoroutine == null)
            {
                slowdownCoroutine = StartCoroutine(SlowDownWheel());
            }
        }
    }

    private IEnumerator SlowDownWheel()
    {
        float slowdownTime = 0.2f;
        float targetValue = 0f;

        while (Mathf.Abs(_offsetTilling) > 0.01f)
        {
            _offsetTilling = Mathf.SmoothDamp(_offsetTilling, targetValue, ref currentVelocity, slowdownTime);
            if (_wheelTankMaterials != null && _wheelTankMaterials.Count > 0)
            {
                _wheelTankMaterials.ForEach((v) =>
                {
                    v.mainTextureOffset = new Vector2(0, _offsetTilling * -dirWheelTank);
                });
            }

            yield return null;
        }

        _offsetTilling = 0f;
        if (_wheelTankMaterials != null && _wheelTankMaterials.Count > 0)
        {
            _wheelTankMaterials.ForEach((v) =>
            {
                v.mainTextureOffset = new Vector2(0, _offsetTilling);
            });
        }

        slowdownCoroutine = null;
    }


    public void RunWheelBack(float multiplier = 5)
    {
        RunWheel(false, multiplier);
    }
    private void RunWheel(bool isForward, float multiplier = 5)
    {
        if (_isRun) return;
        _isRun = true;

        if (_wheelTankCR != null)
            StopCoroutine(_wheelTankCR);
        _wheelTankCR = RunWheelTank(isForward, multiplier);
        StartCoroutine(_wheelTankCR);
    }
    public void StopWheel()
    {
        if (!_isRun) return;
        _isRun = false;
        if (_wheelTankCR != null)
            StopCoroutine(_wheelTankCR);
    }

    private IEnumerator RunWheelTank(bool isForward, float multiplier)
    {
        float offsetTilling = 0;
        while (true)
        {
            offsetTilling += Mathf.Abs(Time.deltaTime * multiplier);
            if (_wheelTankMaterials != null && _wheelTankMaterials.Count > 0)
            {
                _wheelTankMaterials.ForEach((v) =>
                {
                    v.mainTextureOffset = new Vector2(0, isForward ? -offsetTilling : offsetTilling);
                });
            }
            yield return null;
        }
    }

    private IEnumerator RunWheelTankAI()
    {
        float offsetTilling = 0;
        while (true)
        {
            if (_aiBotController.IsRunning)
            {
                offsetTilling += Mathf.Abs(Time.deltaTime * 5);
                if (_wheelTankMaterials != null && _wheelTankMaterials.Count > 0)
                {
                    _wheelTankMaterials.ForEach((v) =>
                    {
                        v.mainTextureOffset = new Vector2(0, -offsetTilling);
                    });
                }
            }
            yield return null;
        }
    }

    public float InverseProximityToZero(float value)
    {
        if (value == 0) return 0;
        return 0.1f / Math.Abs(value);
    }

}
