using System.Collections;
using Cinemachine;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using LatteGames.GameManagement;
using LatteGames.PvP;
using System;
using UnityEngine.UI;
using LatteGames.Template;
using System.Linq;
using GachaSystem.Core;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;

public class FTUEDoubleTapToReverse : MonoBehaviour
{
    public static Action OnFinishedFTUE;
    [SerializeField] List<MeshRenderer> darkMeshRenderers;
    [SerializeField] Color darkColor;
    [SerializeField] CanvasGroupVisibility canvasGroupVisibility;
    [SerializeField] CanvasGroup characterEmotionCanvasGroup;
    [SerializeField] float timeToForceTutorial = 12f;
    [SerializeField] float percentEnemyHealthToEnterFTUE = 0.5f;

    Dictionary<MeshRenderer, Color> originalColors = new();

    Coroutine WaitToForceTutorialCoroutine;

    void Awake()
    {
        GameEventHandler.AddActionEvent(CompetitorStatusEventCode.OnHealthChanged, HandleCompetitorHealthChanged);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnFinalRoundCompleted);
        foreach (var meshRenderer in darkMeshRenderers)
        {
            originalColors.Add(meshRenderer, meshRenderer.sharedMaterial.color);
        }
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        StopAllCoroutines();
    }

    void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnHealthChanged, HandleCompetitorHealthChanged);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnFinalRoundCompleted);
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.V))
    //     {
    //         StartCoroutine(CR_PlayTutorial());
    //     }
    // }

    public void StartFTUEDoubleTapToReverse()
    {
        WaitToForceTutorialCoroutine = StartCoroutine(CommonCoroutine.Delay(timeToForceTutorial, false, () =>
        {
            StartCoroutine(CR_PlayTutorial());
            GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnHealthChanged, HandleCompetitorHealthChanged);
        }));
    }

    void HandleCompetitorHealthChanged(params object[] parameters)
    {
        var robot = parameters[0] as PBRobot;
        if (!robot.PersonalInfo.isLocal)
        {
            if (!robot.IsDead && robot.HealthPercentage <= percentEnemyHealthToEnterFTUE)
            {
                StartCoroutine(CR_PlayTutorial());
                StopCoroutine(WaitToForceTutorialCoroutine);
                GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnHealthChanged, HandleCompetitorHealthChanged);
            }
        }
    }

    Coroutine HoldHammerUpCoroutine;

    IEnumerator CR_HoldHammerUp(Rigidbody playerRB, Rigidbody enemyRB)
    {
        while (true)
        {
            playerRB?.AddTorque(-playerRB.transform.right * 10f, ForceMode.VelocityChange);
            enemyRB?.AddTorque(-enemyRB.transform.right * 10f, ForceMode.VelocityChange);
            yield return null;
        }
    }

    Coroutine CheckToFlipCoroutine;

    IEnumerator CR_CheckToFlip(CarPhysics enemyCarPhysics)
    {
        while (true)
        {
            if (enemyCarPhysics.IsImmobilized)
            {
                enemyCarPhysics.Flip();
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator CR_PlayTutorial()
    {
        GameEventHandler.Invoke(LogFTUEEventCode.StartReverse);

        var robots = PBRobot.allFightingRobots;
        var enemy = robots.ToList().Find(x => !x.PersonalInfo.isLocal);
        var player = robots.ToList().Find(x => x.PersonalInfo.isLocal);
        var playerCarPhysics = player.ChassisInstance.CarPhysics;
        var enemyCarPhysics = enemy.ChassisInstance.CarPhysics;
        enemy.GetComponentInChildren<AIBotController>().enabled = false;
        enemy.EnabledAllParts(false);
        player.EnabledAllParts(false);
        enemyCarPhysics.AccelInput = 0;
        playerCarPhysics.AccelInput = 0;
        var playerController = FindObjectOfType<PlayerController>(true);
        var flipButton = FindObjectOfType<FlipButtonController>(true);
        var doubleTapToReverse = FindObjectOfType<DoubleTapToReverse>(true);
        var levelController = FindObjectOfType<PBLevelController>(true);
        playerController?.SetActive(false);
        flipButton?.gameObject.SetActive(false);
        doubleTapToReverse.CancelReversing();
        doubleTapToReverse.enabled = false;
        levelController.IsStopCountdownMatchTime = true;

        var playerUpperRB = player.ChassisInstance.PartContainers.Find(x => x.PartSlotType == PBPartSlot.Upper_1).Containers[0].GetComponentInChildren<Rigidbody>();
        var playerFrontRB = player.ChassisInstance.PartContainers.Find(x => x.PartSlotType == PBPartSlot.Front_1).Containers[0].GetComponentInChildren<Rigidbody>();
        var enemyUpperRB = enemy.ChassisInstance.PartContainers.Find(x => x.PartSlotType == PBPartSlot.Upper_1).Containers[0].GetComponentInChildren<Rigidbody>();
        var playerUpperLocalPos = playerCarPhysics.transform.InverseTransformPoint(playerUpperRB.transform.position);
        var playerFrontLocalPos = playerCarPhysics.transform.InverseTransformPoint(playerFrontRB.transform.position);
        var enemyUpperLocalPos = enemyCarPhysics.transform.InverseTransformPoint(enemyUpperRB.transform.position);

        //Hold both hammers
        HoldHammerUpCoroutine = StartCoroutine(CR_HoldHammerUp(playerUpperRB, enemyUpperRB));

        var centerXBound = 5;
        var centerZBound = 9;
        var carTopSpeedMultiplier = 3f;
        var rotationMultiplier = 0.03f;
        var distanceFromPlayer = 5.5f;
        var limitTimeToTeleport = 3f;
        var attackableRange = 4.3f;
        var accectedDistanceToTarget = 3f;
        var bonusDistance = 5f;

        var startFTUETimeStamp = Time.time;

        var currentEnemySpeed = carTopSpeedMultiplier;
        playerCarPhysics.CarTopSpeedMultiplier *= carTopSpeedMultiplier;
        enemyCarPhysics.CarTopSpeedMultiplier *= currentEnemySpeed;

        DarkenScene(true);

        CheckToFlipCoroutine = StartCoroutine(CR_CheckToFlip(enemyCarPhysics));

        //Move to center of the stage
        while (Mathf.Abs(transform.InverseTransformPoint(playerCarPhysics.transform.position).x) > centerXBound || Mathf.Abs(transform.InverseTransformPoint(playerCarPhysics.transform.position).z) > centerZBound)
        {
            if (Time.time - startFTUETimeStamp > limitTimeToTeleport)
            {
                break;
            }
            var direction = transform.position - playerCarPhysics.transform.position;
            var sameDirectionMultiplier = Vector3.Dot(playerCarPhysics.transform.forward, direction) < 0 ? -1 : 1;
            playerCarPhysics.AccelInput = 1 * sameDirectionMultiplier;
            playerCarPhysics.RotationInput = Vector3.SignedAngle(playerCarPhysics.transform.forward * sameDirectionMultiplier, direction, Vector3.up) * rotationMultiplier;

            var enemyDirection = GetDestination() - enemyCarPhysics.transform.position;
            var enemySameDirectionMultiplier = Vector3.Dot(enemyCarPhysics.transform.forward, enemyDirection) < 0 ? -1 : 1;
            enemyCarPhysics.AccelInput = 1 * 0.02f * enemySameDirectionMultiplier;
            enemyCarPhysics.RotationInput = Vector3.SignedAngle(enemyCarPhysics.transform.forward * enemySameDirectionMultiplier, enemyDirection, Vector3.up) * rotationMultiplier;
            yield return null;
        }

        //Stop player's bot
        playerCarPhysics.AccelInput = 0;
        playerCarPhysics.RotationInput = 0;
        playerCarPhysics.Brake();

        Vector3 GetDestination()
        {
            var playerLocalEnemyPos = playerCarPhysics.transform.InverseTransformPoint(enemyCarPhysics.transform.position);
            var direction = enemyCarPhysics.transform.position - playerCarPhysics.transform.position;
            if (Vector3.SignedAngle(direction, playerCarPhysics.transform.forward, Vector3.up) > 10)
            {
                return playerCarPhysics.CarRb.transform.position + (Quaternion.AngleAxis(20, Vector3.up) * direction).normalized * distanceFromPlayer;
            }
            else if (Vector3.SignedAngle(direction, playerCarPhysics.transform.forward, Vector3.up) < -10)
            {
                return playerCarPhysics.CarRb.transform.position + (Quaternion.AngleAxis(-20, Vector3.up) * direction).normalized * distanceFromPlayer;
            }
            return playerCarPhysics.CarRb.transform.position + playerCarPhysics.CarRb.transform.forward * distanceFromPlayer;
        }

        void DecayEnemySpeed(float distance, float threshold)
        {
            enemyCarPhysics.CarTopSpeedMultiplier /= currentEnemySpeed;
            currentEnemySpeed = Mathf.Clamp(distance / threshold, 0.2f, 1f) * carTopSpeedMultiplier;
            enemyCarPhysics.CarTopSpeedMultiplier *= currentEnemySpeed;
        }

        //Move to front of player's bot 
        while (Vector3.ProjectOnPlane(playerCarPhysics.CarRb.transform.position + playerCarPhysics.CarRb.transform.forward * distanceFromPlayer - enemyCarPhysics.transform.position, Vector3.up).magnitude > accectedDistanceToTarget)
        {
            if (Time.time - startFTUETimeStamp > limitTimeToTeleport)
            {
                break;
            }
            if (Vector3.Angle(enemyCarPhysics.transform.forward, playerCarPhysics.transform.position - enemyCarPhysics.transform.position) < 15f &&
                Vector3.ProjectOnPlane(playerCarPhysics.transform.position - enemyCarPhysics.transform.position, Vector3.up).magnitude < attackableRange &&
                playerCarPhysics.transform.InverseTransformPoint(enemyCarPhysics.transform.position).z > 2.5f)
            {
                break;
            }

            DecayEnemySpeed(Vector3.ProjectOnPlane(playerCarPhysics.CarRb.transform.position + playerCarPhysics.CarRb.transform.forward * distanceFromPlayer - enemyCarPhysics.transform.position, Vector3.up).magnitude, accectedDistanceToTarget + bonusDistance);
            var destination = GetDestination();
            var enemyDirection = destination - enemyCarPhysics.transform.position;
            var enemyToPlayerDirection = playerCarPhysics.transform.position - enemyCarPhysics.transform.position;
            var enemySameDirectionMultiplier = Vector3.Dot(enemyCarPhysics.transform.forward, enemyDirection) < 0 ? -1 : 1;
            enemyCarPhysics.AccelInput = 1 * enemySameDirectionMultiplier;
            enemyCarPhysics.RotationInput = Vector3.SignedAngle(enemyCarPhysics.transform.forward * enemySameDirectionMultiplier, enemyDirection, Vector3.up) * rotationMultiplier;
            yield return null;
        }

        //Move close to the player's bot
        while (Vector3.Angle(enemyCarPhysics.transform.forward, playerCarPhysics.transform.position - enemyCarPhysics.transform.position) > 15f ||
                Vector3.ProjectOnPlane(playerCarPhysics.transform.position - enemyCarPhysics.transform.position, Vector3.up).magnitude > attackableRange)
        {
            if (Time.time - startFTUETimeStamp > limitTimeToTeleport)
            {
                break;
            }
            DecayEnemySpeed(Vector3.ProjectOnPlane(playerCarPhysics.transform.position - enemyCarPhysics.transform.position, Vector3.up).magnitude, attackableRange + bonusDistance);
            var enemyDirection = playerCarPhysics.transform.position - enemyCarPhysics.transform.position;
            enemyCarPhysics.AccelInput = 1;
            enemyCarPhysics.RotationInput = Vector3.SignedAngle(enemyCarPhysics.transform.forward, enemyDirection, Vector3.up) * rotationMultiplier;
            yield return null;
        }

        enemyCarPhysics.AccelInput = 0;
        enemyCarPhysics.RotationInput = 0;
        enemyCarPhysics.Brake();

        //Check time to teleport to FTUE position
        if (Time.time - startFTUETimeStamp > limitTimeToTeleport)
        {
            playerCarPhysics.CarRb.isKinematic = true;
            enemyCarPhysics.CarRb.isKinematic = true;

            var playerLocalPos = transform.InverseTransformPoint(playerCarPhysics.transform.position);
            playerLocalPos.x = Mathf.Clamp(playerLocalPos.x, -centerXBound, centerXBound);
            playerLocalPos.z = Mathf.Clamp(playerLocalPos.z, -centerZBound, centerZBound);
            playerCarPhysics.CarRb.transform.position = transform.TransformPoint(playerLocalPos);
            playerCarPhysics.CarRb.transform.rotation = Quaternion.LookRotation(playerCarPhysics.CarRb.transform.forward, Vector3.up);
            enemyCarPhysics.CarRb.transform.position = playerCarPhysics.CarRb.transform.position + playerCarPhysics.CarRb.transform.forward * attackableRange;
            enemyCarPhysics.CarRb.transform.rotation = Quaternion.LookRotation(-playerCarPhysics.CarRb.transform.forward);
            var upperForward = playerCarPhysics.transform.TransformDirection(new Vector3(0, 0.6f, 0.4f));
            var upperUp = Vector3.Cross(upperForward, playerCarPhysics.transform.right);
            var enemyUpperForward = enemyCarPhysics.transform.TransformDirection(new Vector3(0, 0.6f, 0.4f));
            var enemyUpperUp = Vector3.Cross(enemyUpperForward, enemyCarPhysics.transform.right);

            playerUpperRB.transform.position = playerCarPhysics.transform.TransformPoint(playerUpperLocalPos);
            enemyUpperRB.transform.position = enemyCarPhysics.transform.TransformPoint(enemyUpperLocalPos);
            playerFrontRB.transform.position = playerCarPhysics.transform.TransformPoint(playerFrontLocalPos);
            playerFrontRB.transform.rotation = playerCarPhysics.CarRb.transform.rotation;
            playerUpperRB.transform.rotation = Quaternion.LookRotation(upperForward, upperUp);
            enemyUpperRB.transform.rotation = Quaternion.LookRotation(enemyUpperForward, enemyUpperUp);
            yield return new WaitForSeconds(0.15f);
        }

        StopCoroutine(HoldHammerUpCoroutine);

        var forward = enemyCarPhysics.transform.TransformDirection(new Vector3(0, -0.1f, 0.9f));
        var up = Vector3.Cross(forward, enemyCarPhysics.transform.right);

        //Move down the hammer
        enemyUpperRB.transform.DORotateQuaternion(Quaternion.LookRotation(forward, up), AnimationDuration.TINY).OnComplete(() =>
        {
            Pause(true);
        });
        canvasGroupVisibility.Show();
        doubleTapToReverse.enabled = true;
        playerCarPhysics.CarTopSpeedMultiplier /= carTopSpeedMultiplier;
        enemyCarPhysics.CarTopSpeedMultiplier /= currentEnemySpeed;

        yield return new WaitForSecondsRealtime(0.25f);
        var hasDoubleTap = false;
        doubleTapToReverse.OnStartReversing += OnStartReversing;
        doubleTapToReverse.CancelReversing();
        yield return new WaitUntil(() => hasDoubleTap);

        GameEventHandler.Invoke(LogFTUEEventCode.EndReverse);

        void OnStartReversing()
        {
            doubleTapToReverse.OnStartReversing -= OnStartReversing;
            hasDoubleTap = true;
        }

        levelController.IsStopCountdownMatchTime = false;
        Pause(false);
        playerCarPhysics.CarRb.isKinematic = false;
        enemyCarPhysics.CarRb.isKinematic = false;
        DarkenScene(false);
        canvasGroupVisibility.Hide();
        playerController?.SetActive(true);

        forward = enemyCarPhysics.transform.TransformDirection(new Vector3(0, -1f, 0));
        up = Vector3.Cross(forward, enemyCarPhysics.transform.right);
        enemyUpperRB.transform.DORotateQuaternion(Quaternion.LookRotation(forward, up), 0.2f);
        var originalLayer = enemyUpperRB.gameObject.layer;
        enemyUpperRB.gameObject.layer = LayerMask.NameToLayer("PlayerPart");

        yield return new WaitForSeconds(AnimationDuration.TINY);

        enemy.GetComponentInChildren<AIBotController>().enabled = true;
        enemy.EnabledAllParts(true);
        player.EnabledAllParts(true);
        OnFinishedFTUE?.Invoke();

        yield return new WaitForSeconds(AnimationDuration.TINY);
        enemyUpperRB.gameObject.layer = originalLayer;
    }

    public void DarkenScene(bool isDarken)
    {
        foreach (var meshRenderer in darkMeshRenderers)
        {
            meshRenderer.material.DOColor(isDarken ? darkColor : originalColors[meshRenderer], AnimationDuration.TINY).SetUpdate(true);
        }
    }

    public void Pause(bool isPause)
    {
        Time.timeScale = isPause ? 0 : 1;
        characterEmotionCanvasGroup.alpha = isPause ? 0 : 1;
    }
}