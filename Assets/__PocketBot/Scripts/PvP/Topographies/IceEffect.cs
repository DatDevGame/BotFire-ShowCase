using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IceEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _iceVFX;
    private Rigidbody _rigidbodyTarget;

    private IEnumerator RunningEffectCR;


    public void SetUpEffect(Rigidbody rigidbodyTarget)
    {
        _rigidbodyTarget = rigidbodyTarget;
        StartCoroutineHelper(RunningEffectCR, RunningEffect());
    }

    private void OnDestroy()
    {
        StopCoroutineHelper(RunningEffectCR);
    }

    private IEnumerator RunningEffect()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);
        while (true)
        {
            if (_rigidbodyTarget == null) yield break;
            if (_rigidbodyTarget.velocity.magnitude >= 1)
            {
                if (_iceVFX != null)
                {
                    if (!_iceVFX.isPlaying)
                        _iceVFX.Play();
                }    
            }
            else
            {
                if (_iceVFX != null)
                {
                    if (!_iceVFX.isStopped)
                        _iceVFX.Stop();
                }
            }
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
}
