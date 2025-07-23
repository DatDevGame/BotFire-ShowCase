using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using LatteGames.Template;

public class PBBadgeHandle : MonoBehaviour
{
    //Event In Animator
    public void HideEvent() => GameEventHandler.Invoke(ArenaUnlockEvent.CloseArenaUnlock);

    public void ShowTextAnimation() => GameEventHandler.Invoke(ArenaUnlockEvent.ShowTextAnimation);

    public void StartUnlockArenaAnimation() => GameEventHandler.Invoke(ArenaUnlockEvent.StartBadgeAnimation);

    public void EndUnlockArenaAnimation() => GameEventHandler.Invoke(ArenaUnlockEvent.EndUnBadgeAnimation);

    public void Callconfetti() => ConfettiParticleUI.Instance.PlayFX();

    public void ShowPartternArena() => GameEventHandler.Invoke(ArenaUnlockEvent.ShowParttern);

    public void HidePartternArena() => GameEventHandler.Invoke(ArenaUnlockEvent.HideParttern);

    public void ShowLightEffect() => GameEventHandler.Invoke(ArenaUnlockEvent.ShowLightEffect);

    public void EnableSoundBadge() => SoundManager.Instance.PlaySFX(PBSFX.UIUnlockArena);
}
