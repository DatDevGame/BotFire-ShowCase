using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LatteGames.Template;

public class SkibidiHeadBehaviour : GunBehaviour
{
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;

    Tweener tweener;

    protected override IEnumerator Start()
    {
        skinnedMeshRenderer.SetBlendShapeWeight(0, 0);
        yield return base.Start();
    }

    protected override IEnumerator CR_Shoot()
    {
        while (true)
        {
            //Open mouth and shoot
            OpenMouth(true);
            yield return new WaitForSeconds(0.5f);
            IsShooting = true;
            if (IsObstacle == false) SoundManager.Instance.PlayLoopSFX(SFX.Flamethrower, 0.25f, true, true, gameObject);

            //Stop shoot and close mouth
            yield return new WaitForSeconds(shootingTime);
            IsShooting = false;
            OpenMouth(false);
            if (IsObstacle == false) SoundManager.Instance.PauseLoopSFX(gameObject);

            yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(attackCycleTime - 0.5f);
        }
    }

    void OpenMouth(bool isOpen)
    {
        if (tweener != null) DOTween.Kill(tweener);
        tweener = DOTween.To(() => skinnedMeshRenderer.GetBlendShapeWeight(0),
            x => skinnedMeshRenderer.SetBlendShapeWeight(0, x), isOpen ? 100 : 0, 0.5f);
    }
}
