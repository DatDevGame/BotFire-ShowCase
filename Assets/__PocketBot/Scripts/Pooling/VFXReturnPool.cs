using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXReturnPool : MonoBehaviour
{
    public ParticleSystem ps;
    VFXReturnPool original;
    Coroutine coroutine;

    public void PlayThenReturn(VFXReturnPool original)
    {
        this.original = original;
        ps.Play();
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(WaitUntilDead());
    }

    IEnumerator WaitUntilDead()
    {
        yield return new WaitUntil(() => !ps.IsAlive(true));
        BulletPoolManager.Instance.Release(original, this);
    }
}
