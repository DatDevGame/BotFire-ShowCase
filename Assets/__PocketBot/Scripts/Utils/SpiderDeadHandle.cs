using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FIMSpace.FProceduralAnimation;
using System.Linq;

public class SpiderDeadHandle : MonoBehaviour
{
    [SerializeField] private Collider _colliderBody;
    [SerializeField] private Collider _colliderModel;

    [SerializeField] private LegsAnimator _legsAnimator;
    [SerializeField] private List<Transform> legs;
    [SerializeField] private List<Collider> colliderLegs;
    [SerializeField] private CarPhysics _carPhysics;

    PBRobot _robot;

    private void Start()
    {
        _robot = GetComponentInParent<PBRobot>();
        if (_robot != null)
            _robot.OnHealthChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        if (_robot != null)
            _robot.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(Competitor.HealthChangedEventData eventData)
    {
        if (eventData.CurrentHealth > 0)
            return;
        Destroy(_legsAnimator);
        colliderLegs.ForEach((v) => { v.enabled = true; });
        for (int i = 0; i < legs.Count; i++)
        {
            legs[i].SetParent(_carPhysics.transform.parent);
            Rigidbody rbLeg = legs[i].gameObject.GetOrAddComponent<Rigidbody>();

            float randomX = UnityEngine.Random.Range(-1f, 1f);
            float randomY = UnityEngine.Random.Range(-1f, 1f);
            float randomZ = UnityEngine.Random.Range(-1f, 1f);

            Vector3 randomForce = new Vector3(randomX, randomY, randomZ).normalized * 5;
            rbLeg.AddExplosionForce(UnityEngine.Random.Range(5, 10), _carPhysics.transform.position, 10, UnityEngine.Random.Range(2, 5), ForceMode.VelocityChange);
        }

        _colliderBody.enabled = false;
        _colliderModel.enabled = true;
        Destroy(this);
    }
}
