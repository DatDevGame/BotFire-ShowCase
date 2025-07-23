using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PBAntiSlidingBox : MonoBehaviour
{
    public Action<PBAntiSlidingBox> OnDisableBox;
    PBChassis chassis;
    BoxCollider boxCollider;

    public BoxCollider BoxCollider { get => boxCollider; }

    private void Start()
    {
        chassis = GetComponentInParent<PBChassis>();
        boxCollider = GetComponent<BoxCollider>();
        chassis.Robot.OnHealthChanged += OnHealthChanged;

        CalculateCombinedCollider();
    }

    void CalculateCombinedCollider()
    {
        Bounds combinedBounds = new Bounds();

        var colliders = chassis.CarPhysics.GetComponentsInChildren<BoxCollider>(true);
        foreach (var collider in colliders)
        {
            Bounds bounds = collider.bounds;
            bounds.center = boxCollider.transform.InverseTransformPoint(bounds.center);
            combinedBounds.Encapsulate(bounds);
        }

        var frontContainer = chassis.PartContainers.Find(item => item.PartSlotType == PBPartSlot.Front_1);
        if (frontContainer.Containers != null && frontContainer.Containers[0] != null)
        {
            var frontColliders = frontContainer.Containers[0].GetComponentsInChildren<BoxCollider>(true);
            foreach (var collider in frontColliders)
            {
                Bounds bounds = collider.bounds;
                bounds.center = boxCollider.transform.InverseTransformPoint(bounds.center);
                combinedBounds.Encapsulate(bounds);
            }
        }

        // Set the position and size of the combined BoxCollider
        var center = combinedBounds.center;
        boxCollider.center = new Vector3(center.x, boxCollider.center.y, center.z);
        boxCollider.size = new Vector3(combinedBounds.size.x * 0.95f, boxCollider.size.y, combinedBounds.size.z * 0.95f);
        // DrawCombinedBounds(combinedBounds);
    }

    void DrawCombinedBounds(Bounds combinedBounds)
    {
        Vector3 center = combinedBounds.center;
        Vector3 size = combinedBounds.size;

        Vector3 min = center - size * 0.5f;
        Vector3 max = center + size * 0.5f;

        // Draw wireframe box
        Debug.DrawLine(min, new Vector3(min.x, min.y, max.z), Color.red, 5);
        Debug.DrawLine(min, new Vector3(min.x, max.y, min.z), Color.red, 5);
        Debug.DrawLine(min, new Vector3(max.x, min.y, min.z), Color.red, 5);

        Debug.DrawLine(max, new Vector3(max.x, max.y, min.z), Color.red, 5);
        Debug.DrawLine(max, new Vector3(max.x, min.y, max.z), Color.red, 5);
        Debug.DrawLine(max, new Vector3(min.x, max.y, max.z), Color.red, 5);

        Debug.DrawLine(new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z), Color.red, 5);
        Debug.DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z), Color.red, 5);
        Debug.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z), Color.red, 5);

        Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), Color.red, 5);
        Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), Color.red, 5);
        Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, min.y, min.z), Color.red, 5);
    }

    private void OnDestroy()
    {
        if (chassis != null)
        {
            chassis.Robot.OnHealthChanged -= OnHealthChanged;
        }
    }

    void OnHealthChanged(Competitor.HealthChangedEventData data)
    {
        if (data.CurrentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        OnDisableBox?.Invoke(this);
    }
}
