using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class HookRopeBehavior : MonoBehaviour
{
    [SerializeField] private HeadHookRope _headHookRope;
    [SerializeField] private GameObject _holderHookRope;
    [SerializeField] private List<GameObject> _hookRope;

    private Action _fireAction;
    private Action _goodFire;
    private Action _badFire;

    private void Start()
    {
        _fireAction += FireAction;
        _goodFire += GoodFireAction;
        _badFire += BadFireAction;

        _headHookRope.SetAction(_fireAction, _goodFire, _badFire);
    }
    private void OnDestroy()
    {
        _fireAction -= FireAction;
        _goodFire -= GoodFireAction;
        _badFire -= BadFireAction;
    }
    private void FireAction()
    {
        _hookRope.ForEach(v => v.gameObject.SetActive(true));
    }
    private void GoodFireAction()
    {
        // Debug.Log("Good Fire");
    }
    private void BadFireAction()
    {
        _holderHookRope.GetComponent<HingeJoint>().connectedBody = null;
    }
}
