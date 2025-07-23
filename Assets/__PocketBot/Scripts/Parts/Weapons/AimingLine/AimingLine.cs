using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingLine : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _rayDistance = 50f;
    [SerializeField] private LayerMask _hitLayers;

    private void Start()
    {
        _hitLayers = GetAllLayersExceptMask(1 << gameObject.layer);
    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _rayDistance, _hitLayers, QueryTriggerInteraction.Ignore))
        {
            Vector3 lenghtLine = new Vector3(0, 0, Vector3.Distance(transform.position, hit.point));
            _lineRenderer.SetPosition(1, hit.point == Vector3.zero ? transform.position * _rayDistance : lenghtLine);
        }
    }

    public void OnAimLine() => _lineRenderer.enabled = true;
    public void OffAimLine() => _lineRenderer.enabled = false;

    private LayerMask GetAllLayersExceptMask(LayerMask maskToExclude)
    {
        LayerMask allLayersMask = GetAllLayersMask();
        LayerMask invertedMask = ~maskToExclude;
        LayerMask resultMask = allLayersMask & invertedMask;

        return resultMask;
    }

    private LayerMask GetAllLayersMask()
    {
        LayerMask allLayersMask = 0;

        for (int i = 0; i < 32; i++)
        {
            if (IsLayerInUse(i))
            {
                allLayersMask |= 1 << i;
            }
        }

        return allLayersMask;
    }

    private bool IsLayerInUse(int layer)
    {
        // Check if the layer is in use in the project settings
        string layerName = LayerMask.LayerToName(layer);
        return !string.IsNullOrEmpty(layerName);
    }
}
