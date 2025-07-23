using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarLink : MonoBehaviour
{
    [SerializeField] private Joint _jointThis;
    [SerializeField] private Joint _jointOther;

    private void Start()
    {
        StartCoroutine(ConnectJoint());
    }

    private IEnumerator ConnectJoint()
    {
        while (_jointThis.connectedBody == null)
        {
            if (_jointThis.connectedBody == null)
            {
                if (_jointOther != null)
                {
                    if (_jointOther.connectedBody != null)
                    {
                        _jointThis.connectedBody = _jointOther.connectedBody;
                    }
                }
            }
            yield return null;
        }

        Destroy(this);
    }
}
