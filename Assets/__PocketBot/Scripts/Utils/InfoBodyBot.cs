using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;

public class InfoBodyBot : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<PBPartSlot, Transform> infoPart;
    public SerializedDictionary<PBPartSlot, Transform> InfoPart => infoPart;
}
