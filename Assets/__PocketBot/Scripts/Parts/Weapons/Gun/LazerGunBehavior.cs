using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LatteGames.GameManagement;
using LatteGames.Template;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class LazerGunBehavior : PartBehaviour, IBoostFireRate
{
    [SerializeField, BoxGroup("Rotating Angle")] private float _rotateDuration = 2;
    [SerializeField, BoxGroup("Rotating Angle")] private float _angleEnds = 10;
    [SerializeField, BoxGroup("Rotating Angle")] private float _angleStart = 30f;

    [SerializeField, BoxGroup("Property")] private float _nextBehavior = 2;
    [SerializeField, BoxGroup("Property")] private float _inactiveInSecond = 2f;
    [SerializeField, BoxGroup("Property")] private float _activeInSecond = 5f;

    [SerializeField, BoxGroup("Object")] private GameObject _aimingLine;
    [SerializeField, BoxGroup("Object")] private Transform _collider;
    [SerializeField, BoxGroup("Object")] private Transform _axis;
    [SerializeField, BoxGroup("Object")] private Transform _pointFire;
    [SerializeField, BoxGroup("Object")] private LineRenderer _lineLazer;

    [SerializeField, BoxGroup("VFX")] private ParticleSystem _triggerWarningVFX;
    [SerializeField, BoxGroup("VFX")] private ParticleSystem _triggerVFX;

    [SerializeField] private float _lengthLazer = 100;

    private LayerMask _layerMaskIgnoreThisLayer;
    private bool m_IsOnLazer = false;
    private IEnumerator _behaviorLazerGunCR;

    //Speed Up Handle
    private bool m_IsSpeedUp = false;
    private int m_StackSpeedUp;
    private float m_ObjectTimeScale;
    private float m_TimeScaleOrginal => TimeManager.Instance.originalScaleTime;
    private float m_BoosterPercent;

    [SerializeField, BoxGroup("Visual")] private List<MeshRenderer> m_MeshRendererBooster;
    private void Awake()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;
    }

    protected override IEnumerator Start()
    {
        yield return base.Start();
        _layerMaskIgnoreThisLayer = GetAllLayersExceptMask(1 << gameObject.layer);
        OnStartLazerGun();
    }
    
    private void OnDestroy()
    {
        if (_behaviorLazerGunCR != null)
        {
            if (_lineLazer != null)
                _lineLazer.enabled = false;

            if (_collider != null)
                _collider.gameObject.SetActive(false);

            if (_triggerWarningVFX != null)
                _triggerWarningVFX.gameObject.SetActive(false);

            m_IsOnLazer = false;
            OffVFXTrigger();
            StopCoroutine(_behaviorLazerGunCR);
        }
    }

    private void FixedUpdate()
    {
        LazerFire();
    }

    private void OnStartLazerGun()
    {
        if (_behaviorLazerGunCR != null)
            StopCoroutine(_behaviorLazerGunCR);
        _behaviorLazerGunCR = StartBehavior();
        StartCoroutine(_behaviorLazerGunCR);
    }

    private IEnumerator StartBehavior()
    {
        if(_angleStart > 0)
            transform.DOLocalRotate(new Vector3(_angleStart, 0, 0), 1);

        WaitForSeconds inactiveInSecond = new WaitForSeconds(_inactiveInSecond);
        WaitForSeconds activeInSecond = new WaitForSeconds(_activeInSecond);
        var waitUntilEnable = new WaitUntil(() => { return this.enabled; });
        while (true)
        {
            yield return waitUntilEnable;
            if (enabled)
            {
                SoundManager.Instance.PlayLoopSFX(SFX.LazerChangeActive, PBSoundUtility.IsOnSound() ? 0.5f : 0, false, true, gameObject);
                _triggerWarningVFX.gameObject.SetActive(true);
                _triggerWarningVFX.Play();
                _triggerWarningVFX.transform.DOScale(0.5f, _inactiveInSecond).From(0);
            }

            yield return inactiveInSecond;
            if (enabled)
            {
                SoundManager.Instance.PlayLoopSFX(SFX.LazerFire, PBSoundUtility.IsOnSound() ? 0.5f : 0, false, true, gameObject);
                _triggerWarningVFX.transform.DOScale(0, 0.2f).OnComplete(() => { _triggerWarningVFX.gameObject.SetActive(false); });
                ActiveVFXLazer(true);
                if (_angleStart > 0)
                    transform.DOLocalRotate(new Vector3(_angleEnds, 0, 0), _rotateDuration);

                if (_aimingLine != null)
                    _aimingLine.SetActive(false);
            }

            yield return activeInSecond;
            if (enabled)
            {
                _collider.transform.position = transform.position;
                ActiveVFXLazer(false);
                if (_angleStart > 0)
                    transform.DOLocalRotate(new Vector3(_angleStart, 0, 0), _rotateDuration);
                if (_aimingLine != null)
                    _aimingLine.SetActive(true);
            }

            yield return CustomWaitForSeconds(_nextBehavior);
        }
    }

    private void ActiveVFXLazer(bool isActive)
    {
        m_IsOnLazer = isActive;
        _lineLazer.enabled = m_IsOnLazer;
        _triggerVFX.gameObject.SetActive(m_IsOnLazer);
    }

    private void LazerFire()
    {
        if (!m_IsOnLazer)
        {
            _collider.gameObject.SetActive(false);
            return;
        }
        _collider.gameObject.SetActive(true);

        RaycastHit hit;
        if (Physics.Raycast(_pointFire.position, _pointFire.forward, out hit, _lengthLazer, _layerMaskIgnoreThisLayer, QueryTriggerInteraction.Ignore))
        {
            Vector3 triggerHitPoint = hit.point;
            float lengthLineLazer = Vector3.Distance(_pointFire.position, hit.point);
            _lineLazer.SetPosition(0, Vector3.zero);
            _lineLazer.SetPosition(1, new Vector3(0, 0, lengthLineLazer + 1));

            PBRobot pbRobot = hit.collider.GetComponentInParent<PBRobot>();
            if (pbRobot != null && _collider != null)
            {
                _collider.transform.position = pbRobot.ChassisInstance.CarPhysics.transform.position;
                _collider.gameObject.SetActive(true);
            }
            else
                _collider.transform.position = transform.position;

            if(hit.collider.gameObject.GetComponent<IBreakObstacle>() != null)
                _collider.transform.position = hit.point;

            OnVFXTrigger(triggerHitPoint);
        }
        else
        {
            _lineLazer.SetPosition(1, new Vector3(0, 0, _lengthLazer));
            OffVFXTrigger();
        }
    }
    private void OnVFXTrigger(Vector3 triggerPosition)
    {
        if (_triggerVFX == null) return;

        _triggerVFX.transform.position = triggerPosition;
        if (!_triggerVFX.gameObject.activeSelf)
            _triggerVFX.gameObject.SetActive(true);
    }
    private void OffVFXTrigger()
    {
        if (_collider != null)
            _collider.gameObject.SetActive(false);

        if (_triggerVFX != null)
        {
            if (_triggerVFX.gameObject.activeSelf)
                _triggerVFX.gameObject.SetActive(false);
        }
    }
    private LayerMask GetAllLayersExceptMask(LayerMask maskToExclude)
    {
        LayerMask allLayersMask = GetAllLayersMask();

        // Use bitwise AND NOT to exclude the specified mask
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

#if UNITY_EDITOR
    [SerializeField, BoxGroup("Editor")] private Transform _pointStart;
    [SerializeField, BoxGroup("Editor")] private Transform _pointEnd;
    private void OnDrawGizmos()
    {
        if(_pointStart == null || _pointEnd == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(_pointFire.position, _pointFire.forward * _lengthLazer);
        _pointStart.eulerAngles = new Vector3(_angleEnds, 0, 0);
        _pointEnd.eulerAngles = new Vector3(_angleStart, 0, 0);

        _pointStart.position = new Vector3(_pointStart.position.x, _pointFire.position.y, _pointStart.position.z);
        _pointEnd.position = new Vector3(_pointEnd.position.x, _pointFire.position.y, _pointEnd.position.z);
        _pointStart.eulerAngles = new Vector3(_angleStart, 0, 0);
        _pointEnd.eulerAngles = new Vector3(_angleEnds, 0, 0);

        var pointStart = _pointStart.GetChild(0).position;
        var pointEnd = _pointEnd.GetChild(0).position;

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(pointStart, 0.2f);
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(pointEnd, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(pointStart, pointEnd);

        Gizmos.color = Color.cyan;
        Vector3 a = new Vector3(_pointStart.position.x, _pointFire.position.y, _pointStart.position.z);
        Gizmos.DrawLine(pointStart, a);
        Gizmos.DrawLine(pointEnd, a);
    }
#endif

    private void OnDisable()
    {
        ActiveVFXLazer(false);
        _triggerWarningVFX.gameObject.SetActive(false);
    }

    public void BoostSpeedUpFire(float boosterPercent)
    {
        m_BoosterPercent += boosterPercent;

        //Enable VFX
        if (m_MeshRendererBooster != null && m_MeshRendererBooster.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(pbPart, m_MeshRendererBooster);

        m_StackSpeedUp++;
        m_IsSpeedUp = true;
        m_ObjectTimeScale += m_TimeScaleOrginal * boosterPercent;
    }
    public void BoostSpeedUpStop(float boosterPercent)
    {
        m_StackSpeedUp--;
        if (m_StackSpeedUp <= 0)
        {
            //Disable VFX
            PBBoosterVFXManager.Instance.StopBoosterSpeedUpAttackWeapon(pbPart);

            m_IsSpeedUp = false;
            m_ObjectTimeScale = m_TimeScaleOrginal;
            m_BoosterPercent = 0;
        }
        else
        {
            m_BoosterPercent -= boosterPercent;
            m_ObjectTimeScale -= m_TimeScaleOrginal * boosterPercent;
        }
    }
    public bool IsSpeedUp() => m_IsSpeedUp;
    public int GetStackSpeedUp() => m_StackSpeedUp;
    public float GetPercentSpeedUp() => m_BoosterPercent;

    /// <summary>
    /// Custom wait method to respect the object's time scale.
    /// </summary>
    /// <param name="time">Time to wait in seconds.</param>
    /// <returns>Yield instruction with custom time scale.</returns>
    private IEnumerator CustomWaitForSeconds(float time)
    {
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime * m_ObjectTimeScale; // Apply custom time scale
            yield return null;
        }
    }
}
