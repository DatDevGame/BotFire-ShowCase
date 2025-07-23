using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class SpikeTrap : PartBehaviour
{
    enum SpikeState
    {
        Inactive,
        Active
    }

    [SerializeField, BoxGroup("Object", CenterLabel = true)] private Transform _spikes;
    [SerializeField, BoxGroup("Object", CenterLabel = true)] private GameObject _wallFake;

    [SerializeField, BoxGroup("Component", CenterLabel = true)] private NavMeshObstacle _navMeshObstacle;
    [SerializeField, BoxGroup("Component", CenterLabel = true)] private OnCollisionCallback _spikeOnCollisionCallback;
    [SerializeField, BoxGroup("Component", CenterLabel = true)] private Collider _triggerBox;

    [SerializeField, BoxGroup("Property", CenterLabel = true)] private SpikeState _firstState;
    [SerializeField, BoxGroup("Property", CenterLabel = true)] private ForceMode _forceMode;
    [SerializeField, BoxGroup("Property", CenterLabel = true)] private float _force = 5;
    [SerializeField, BoxGroup("Property", CenterLabel = true)] private float _startDelay = 0f;
    [SerializeField, BoxGroup("Property", CenterLabel = true)] private float _inactiveInSecond = 2f;
    [SerializeField, BoxGroup("Property", CenterLabel = true)] private float _activeInSecond = 3f;

    private PBPartSO _spikeTrapSO;
    private bool _isDamage = false;
    private IEnumerator _startBehaviorCR;

    private void Awake()
    {
        _spikeOnCollisionCallback.onCollisionEnter += OnColisionCallBack;

        PBObstaclePart pBObstaclePart = gameObject.GetComponent<PBObstaclePart>();
        if (pBObstaclePart != null)
        {
            _spikeTrapSO = pBObstaclePart.PartSO;
        }
    }

    private void OnDestroy()
    {
        if (_startBehaviorCR != null)
            StopCoroutine(_startBehaviorCR);
    }

    private void Start()
    {
        if (_startBehaviorCR != null)
            StopCoroutine(_startBehaviorCR);
        _startBehaviorCR = StartBehavior();
        StartCoroutine(_startBehaviorCR);
    }


    private IEnumerator StartBehavior()
    {
        WaitForSeconds inactiveInSecond = new WaitForSeconds(_inactiveInSecond);
        WaitForSeconds activeInSecond = new WaitForSeconds(_activeInSecond);

        yield return new WaitForSeconds(_startDelay);

        bool isFirstTimeRun = true;

        while (true)
        {
            if ((isFirstTimeRun && _firstState == SpikeState.Inactive) || !isFirstTimeRun)
            {
                _navMeshObstacle.enabled = false;
                _isDamage = false;
                isFirstTimeRun = false;
                _spikes.DOLocalMoveY(-0.45f, 0.08f).SetEase(Ease.Linear);
                _wallFake.SetActive(false);
                yield return inactiveInSecond;
            }

            if ((isFirstTimeRun && _firstState == SpikeState.Active) || !isFirstTimeRun)
            {
                _isDamage = true;
                isFirstTimeRun = false;
                _navMeshObstacle.enabled = true;
                _triggerBox.enabled = _isDamage;
                _spikes
                    .DOLocalMoveY(0, 0.1f)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        _isDamage = false;
                        _triggerBox.enabled = _isDamage;
                        _wallFake.SetActive(true);
                    });

                yield return activeInSecond;
            }
        }
    }

    private void OnColisionCallBack(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PBPart part))
        {
            if (part.GetComponent<CarPhysics>() != null)
            {
                Rigidbody rigidbody = part.GetComponent<Rigidbody>();
                if (rigidbody != null && _spikeTrapSO != null)
                {
                    if (part.RobotChassis.Robot.Health > _spikeTrapSO.CalCurrentAttack())
                        rigidbody.AddForce(Vector3.up * _force, _forceMode);
                }
            }
        }
    }
}
