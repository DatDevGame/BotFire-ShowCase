using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using DG.Tweening;
using Sirenix.Utilities;
using LatteGames.Template;
using Unity.AI.Navigation;

public enum ElectricState
{
    Inactive,
    Charge,
    Active
}

public class ElectricFloor : MonoBehaviour, IAttackable
{
    [SerializeField, BoxGroup("No Tweaks")]
    private bool _isControllerGroup = false;
    [SerializeField, BoxGroup("No Tweaks")]
    private NavMeshModifierVolume navMeshModifierVolume;
    [SerializeField, BoxGroup("No Tweaks")]
    private UnityEngine.AI.NavMeshObstacle navMeshObstacle;
    [SerializeField, BoxGroup("Color 3 Looped States Electic")] private Color _startColor;
    [SerializeField, BoxGroup("Color 3 Looped States Electic")] private Color _inactiveColor;
    [SerializeField, BoxGroup("Color 3 Looped States Electic")] private Color _activeColor;

    [SerializeField, BoxGroup("3 Looped States Electic")] private ElectricState _firstElectricState;
    [SerializeField, BoxGroup("3 Looped States Electic")] private float _startDelay = 0f;
    [SerializeField, BoxGroup("3 Looped States Electic")] private float _inactiveInSecond = 2f;
    [SerializeField, BoxGroup("3 Looped States Electic")] private float _chargeInSecond = 1f;
    [SerializeField, BoxGroup("3 Looped States Electic")] private float _activeInSecond = 3f;

    [SerializeField, BoxGroup("VFX")] private ParticleSystem _electricVFX;
    [SerializeField, BoxGroup("Material")] private Material _electricMaterial;
    [SerializeField] private float _damagePerWave = 6f;
    [SerializeField] private float _causeDamageSpeedPerSec = 0.2f;

    [SerializeField] private List<PBPart> _pbPartsTrigger;
    [SerializeField] private MeshRenderer _meshRendererOutlineObject;
    [SerializeField] private MeshRenderer _meshRendererThunderObject;

    private Material _materialOutlineTemp;
    private Material _materialThunderTemp;
    private Collider _zoneTrigger;
    private IEnumerator OnRunningElectricCR;
    private IEnumerator OnTakeDamageEverySecondCR;
    private IEnumerator OnControllerCR;

    private Dictionary<string, string> _saveObjectDead;
    private void Awake()
    {
        _zoneTrigger = gameObject.GetComponent<Collider>();

        _materialOutlineTemp = new Material(_electricMaterial);
        _materialThunderTemp = new Material(_electricMaterial);
        _pbPartsTrigger = new List<PBPart>();

        _meshRendererOutlineObject.material = _materialOutlineTemp;
        _meshRendererThunderObject.material = _materialThunderTemp;

        _saveObjectDead = new Dictionary<string, string>();
    }
    private void Start()
    {
        if (!_isControllerGroup)
            StartCoroutineHelper(OnRunningElectricCR, OnActiveAndInActiveElectic());
    }
    private void OnDestroy()
    {
        StopCoroutineHelper(OnRunningElectricCR);
        StopCoroutineHelper(OnTakeDamageEverySecondCR);
    }

    private IEnumerator OnActiveAndInActiveElectic()
    {
        //Step 1
        SoundManager.Instance.StopLoopSFX(gameObject);
        navMeshObstacle.enabled = false;
        _electricVFX.loop = false;
        _pbPartsTrigger.Clear();
        _materialOutlineTemp.color = _startColor;
        _materialThunderTemp.color = _startColor;
        _zoneTrigger.enabled = false;

        WaitForSeconds inactiveInSecond = new WaitForSeconds(_inactiveInSecond);
        WaitForSeconds chargeInSecond = new WaitForSeconds(_chargeInSecond);
        WaitForSeconds activeInSecond = new WaitForSeconds(_activeInSecond);

        yield return new WaitForSeconds(_startDelay);

        bool isFirstTimeRun = true;

        while (true)
        {
            if ((isFirstTimeRun && _firstElectricState == ElectricState.Inactive) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                //Step 1
                navMeshObstacle.enabled = false;
                _electricVFX.loop = false;
                SoundManager.Instance.StopLoopSFX(gameObject);
                _pbPartsTrigger.Clear();
                _materialOutlineTemp.DOColor(_inactiveColor, 1f).SetEase(Ease.Linear);
                _materialThunderTemp.DOColor(_inactiveColor, 1f).SetEase(Ease.Linear);
                _zoneTrigger.enabled = false;
                StopCoroutineHelper(OnTakeDamageEverySecondCR);

                yield return inactiveInSecond;
            }

            if ((isFirstTimeRun && _firstElectricState == ElectricState.Charge) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                //Step 2
                //Color changeColor = Color.white;
                navMeshObstacle.enabled = true;
                _materialThunderTemp.DOColor(_activeColor, _chargeInSecond);
                _materialOutlineTemp.DOColor(_activeColor, _chargeInSecond)
                    .SetEase(Ease.Linear);

                yield return chargeInSecond;
            }

            if ((isFirstTimeRun && _firstElectricState == ElectricState.Active) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                //Step 3
                navMeshObstacle.enabled = true;
                _materialThunderTemp.color = _activeColor;
                _materialOutlineTemp.color = _activeColor;
                _electricVFX.loop = true;
                SoundManager.Instance.PlayLoopSFX(SFX.ElectricFloor, 0.1f, true, true, gameObject);
                _electricVFX.Play();
                _zoneTrigger.enabled = true;
                StartCoroutineHelper(OnTakeDamageEverySecondCR, OnTakeDamageEverySecond());

                yield return activeInSecond;
            }
        }
    }
    private IEnumerator OnTakeDamageEverySecond()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(_causeDamageSpeedPerSec);
        while (true)
        {
            _pbPartsTrigger.ForEach((v) =>
            {
                if (v == null) return;

                if (v.RobotChassis.Robot.Health > 0)
                {
                    v.ReceiveDamage(this, Const.FloatValue.ZeroF);
                    return;
                }

                if (v.RobotChassis.Robot.Health <= 0 && !_saveObjectDead.ContainsKey(v.RobotChassis.CarPhysics.name))
                {
                    if (_electricVFX != null)
                    {
                        _saveObjectDead[v.RobotChassis.CarPhysics.name] = v.RobotChassis.CarPhysics.name;
                        var electricVFX = Instantiate(_electricVFX, transform).GetComponent<ParticleSystem>();
                        var electricVFXShape = electricVFX.shape;
                        electricVFXShape.scale = Vector3.one;
                        electricVFXShape.shapeType = ParticleSystemShapeType.MeshRenderer;

                        var meshRenderBody = v.GetComponentInChildren<MeshRenderer>();

                        if (meshRenderBody != null)
                        {
                            electricVFX.Stop();
                            var mainVfx = electricVFX.main;
                            mainVfx.duration = 0.1f;

                            electricVFXShape.meshRenderer = meshRenderBody;
                            electricVFX.loop = true;

                            var audioSourceElectric = electricVFX.GetComponent<AudioSource>();
                            if (audioSourceElectric != null)
                                Destroy(audioSourceElectric);

                            electricVFX.Play();
                        }
                    }
                }
            });
            yield return waitForSeconds;
        }
    }


    private void StartCoroutineHelper(IEnumerator enumerator, IEnumerator enumeratorFunc)
    {
        if (enumerator != null)
            StopCoroutine(enumerator);
        enumerator = enumeratorFunc;
        StartCoroutine(enumerator);
    }

    private void StopCoroutineHelper(IEnumerator enumerator)
    {
        if (enumerator != null)
            StopCoroutine(enumerator);
    }


    private void OnTriggerEnter(Collider hitInfo)
    {
        if (hitInfo.TryGetComponent(out PBPart part))
        {
            if (part.GetComponent<CarPhysics>() != null)
            {
                _pbPartsTrigger.Add(part);
            }
        }
    }
    private void OnTriggerExit(Collider outInfo)
    {
        if (outInfo.TryGetComponent(out PBPart part))
        {
            _pbPartsTrigger.Remove(part);
        }
    }

    public float GetDamage()
    {
        return _damagePerWave;
    }

    public void OnController()
    {
        StartCoroutineHelper(OnControllerCR, DelayOnController());
    }
    public void OffController()
    {
        navMeshObstacle.enabled = false;
        _electricVFX.loop = false;
        SoundManager.Instance.StopLoopSFX(gameObject);
        _pbPartsTrigger.Clear();
        _materialOutlineTemp.DOColor(_inactiveColor, 1f).SetEase(Ease.Linear);
        _materialThunderTemp.DOColor(_inactiveColor, 1f).SetEase(Ease.Linear);
        _zoneTrigger.enabled = false;
        StopCoroutineHelper(OnTakeDamageEverySecondCR);
    }

    private IEnumerator DelayOnController()
    {
        navMeshObstacle.enabled = true;
        _materialThunderTemp.DOColor(_activeColor, _chargeInSecond);
        _materialOutlineTemp.DOColor(_activeColor, _chargeInSecond)
            .SetEase(Ease.Linear);

        yield return new WaitForSeconds(_chargeInSecond);
        navMeshObstacle.enabled = true;
        _materialThunderTemp.color = _activeColor;
        _materialOutlineTemp.color = _activeColor;
        _electricVFX.loop = true;
        SoundManager.Instance.PlayLoopSFX(SFX.ElectricFloor, 0.1f, true, true, gameObject);
        _electricVFX.Play();
        _zoneTrigger.enabled = true;
        StartCoroutineHelper(OnTakeDamageEverySecondCR, OnTakeDamageEverySecond());
    }
}
