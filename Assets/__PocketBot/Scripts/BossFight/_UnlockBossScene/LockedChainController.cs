using System.Collections.Generic;
using LatteGames;
using LatteGames.Monetization;
using UnityEngine;

public class LockedChainController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] ParticleSystem sparkFX;

    public void Play()
    {
        animator.SetTrigger("Unlock");
    }

    public void OnBreakTheLock()
    {
        sparkFX.Play();
    }
}
