using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityPhysics : MonoBehaviour
{
    [SerializeField] CarConfigSO carConfigSO;
    Rigidbody carRb;

    public CarConfigSO CarConfigSO
    {
        get => carConfigSO;
        set => carConfigSO = value;
    }

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
    }

    // private void FixedUpdate()
    // {
    //     GravityPhysic();
    // }

    void GravityPhysic()
    {
        carRb.AddForce(Physics.gravity * carConfigSO.GravityFactor, ForceMode.Acceleration);
    }
}
