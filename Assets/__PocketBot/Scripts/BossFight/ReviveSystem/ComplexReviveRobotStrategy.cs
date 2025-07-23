using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FIMSpace.FProceduralAnimation;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames.Utils;
using UnityEngine;

public class ComplexReviveRobotStrategy : MonoBehaviour, IReviveRobotStrategy
{
    [Serializable]
    public struct FragmentedPart
    {
        #region Constructor
        public FragmentedPart(PBPartSlot slotType)
        {
            this.slotType = slotType;
            this.blueprintParts = new List<GameObject>();
            this.instanceParts = new List<GameObject>();
        }
        #endregion

        public PBPartSlot slotType;
        public List<GameObject> blueprintParts;
        public List<GameObject> instanceParts;

        public void CreateAnimatedParts(Transform parent)
        {
            foreach (var blueprintPart in blueprintParts)
            {
                var clonedPart = Instantiate(blueprintPart, parent);
                if (clonedPart.TryGetComponent(out CarPhysics carPhysics))
                {
                    DestroyImmediate(carPhysics);
                }
                var components = clonedPart.GetComponentsInChildren(typeof(Component), true).ToList();
                components.Sort((a, b) => SortComponent(a, b));
                foreach (var component in components)
                {
                    if (component is Transform)
                        continue;
                    if (component is MeshFilter)
                        continue;
                    if (component is Renderer && component is not ParticleSystemRenderer)
                        continue;
                    DestroyImmediate(component);
                }
                clonedPart.transform.CopyTransfrom(blueprintPart.transform);
                instanceParts.Add(clonedPart);
            }
            blueprintParts.Clear();

            int SortComponent(Component a, Component b)
            {
                if (a is Rigidbody)
                {
                    return 1;
                }
                else if (b is Rigidbody)
                {
                    return -1;
                }
                return 0;
            }
        }
    }

    [SerializeField]
    private float animTimeScale = 1f;
    [SerializeField]
    private ParticleSystem constructPartParticlePrefab;

    private GameObject animatedRobot;
    private List<FragmentedPart> fragmentedParts;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(CompetitorStatusEventCode.OnCompetitorBeforeDied, OnCompetitorBeforeDied);
    }

    private void Start()
    {
        animatedRobot = new GameObject("AnimatedRobot");
        animatedRobot.transform.SetParent(this.transform);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnCompetitorBeforeDied, OnCompetitorBeforeDied);
    }

    private void OnCompetitorBeforeDied(object[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return;
        if (parameters[0] is not PBRobot robot)
            return;
        fragmentedParts = new List<FragmentedPart>();
        if (robot.ChassisInstance.PartSO.IsTransformBot)
            return;
        foreach (var partInstance in robot.PartInstances)
        {
            var fragmentedPart = new FragmentedPart(partInstance.PartSlotType);
            fragmentedPart.blueprintParts.AddRange(partInstance.Parts.Select(part =>
            {
                var partType = partInstance.PartSlotType.GetPartTypeOfPartSlot();
                if (partType == PBPartType.Body)
                    part = part.transform.GetChild(0).GetComponent<PBPart>();
                else if (partType == PBPartType.Front || partType == PBPartType.Upper)
                    part = part.GetComponentInChildren<Rigidbody>().GetComponent<PBPart>();
                return part.gameObject;
            }));
            fragmentedParts.Add(fragmentedPart);
        }
    }

    public void ReviveRobot(ReviveData data, Action callback)
    {
        CreateAnimatedParts();
        PBRobot robot = data.robot.BuildRobot(false);
        PBChassis chassisInstance = robot.ChassisInstance;
        List<Vector3> localPositions = new List<Vector3>();
        List<Quaternion> localRotations = new List<Quaternion>();
        foreach (var part in robot.PartInstances)
        {
            if (part.PartSlotType.GetPartTypeOfPartSlot() == PBPartType.Body)
                continue;
            if (part.PartSlotType.GetPartTypeOfPartSlot() == PBPartType.Wheels)
                continue;
            foreach (var pbPart in part.Parts)
            {
                localPositions.Add(chassisInstance.CarPhysics.transform.InverseTransformPoint(pbPart.transform.position));
                localRotations.Add(Quaternion.Inverse(chassisInstance.CarPhysics.transform.rotation) * pbPart.transform.rotation);
            }
        }
        chassisInstance.CarPhysics.enabled = false;
        chassisInstance.CarPhysics.transform.SetPositionAndRotation(data.position, data.rotation);
        var index = 0;
        foreach (var part in robot.PartInstances)
        {
            if (part.PartSlotType.GetPartTypeOfPartSlot() == PBPartType.Body)
                continue;
            if (part.PartSlotType.GetPartTypeOfPartSlot() == PBPartType.Wheels)
                continue;
            foreach (var pbPart in part.Parts)
            {
                pbPart.transform.SetPositionAndRotation(chassisInstance.CarPhysics.transform.TransformPoint(localPositions[index]), chassisInstance.CarPhysics.transform.rotation * localRotations[index]);
                index++;
            }
        }
        StartCoroutine(PlayBuildAnimation_CR());

        void CreateAnimatedParts()
        {
            if (fragmentedParts.Count <= 0)
                return;
            foreach (var fragmentedPart in fragmentedParts)
            {
                fragmentedPart.CreateAnimatedParts(animatedRobot.transform);
            }
            // For boss mega bug only
            PBChassisSO chassisSO = data.robot.ChassisInstance.PartSO.Cast<PBChassisSO>();
            if (chassisSO.IsSpecial && chassisSO.GetInternalName().Equals("Special_MechaBug"))
            {
                var fragmentedBodyPart = fragmentedParts.FirstOrDefault(part => part.slotType == PBPartSlot.Body);
                var skinnedMeshRenderer = fragmentedBodyPart.instanceParts[0].GetComponentInChildren<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null)
                {
                    var oldBones = skinnedMeshRenderer.bones;
                    var newBones = new Transform[oldBones.Length];
                    for (int i = 0; i < oldBones.Length; i++)
                    {
                        var oldBone = oldBones[i];
                        var newBone = FindNewBone(oldBone);
                        newBones[i] = newBone;
                    }
                    skinnedMeshRenderer.bones = newBones;
                    skinnedMeshRenderer.rootBone = newBones[0];

                    Transform FindNewBone(Transform oldBone)
                    {
                        if (oldBone == null)
                            return oldBone;
                        var bone = fragmentedBodyPart.instanceParts[0].transform.FindRecursive(oldBone.name);
                        if (bone == null)
                        {
                            bone = fragmentedParts.Find(fragmentedPart => fragmentedPart.instanceParts.Exists(go => go.name.Equals($"{oldBone.name}(Clone)") || go.transform.FindRecursive(oldBone.name) != null)).instanceParts.Find(go => go.name.Equals($"{oldBone.name}(Clone)") || go.transform.FindRecursive(oldBone.name) != null).transform;
                            if (!bone.name.Replace("(Clone)", "").Equals(oldBone.name))
                            {
                                bone = bone.FindRecursive(oldBone.name);
                            }
                        }
                        return bone;
                    }
                }
            }
        }

        IEnumerator PlayBuildAnimation_CR()
        {
            // Disable renderers of real robot
            var renderers = robot.ChassisInstance.GetComponentsInChildren<Renderer>().Where(renderer => renderer.enabled).ToArray();
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }

            yield return BuildRobotWithAnimation_CR();

            // Enable renderers of real robot
            foreach (var renderer in renderers)
            {
                renderer.enabled = true;
            }
            // Destroy fragment parts
            DestroyFragmentParts();
            callback?.Invoke();

            void DestroyFragmentParts()
            {
                if (fragmentedParts.Count <= 0)
                    return;
                var chassisGO = fragmentedParts.FirstOrDefault(fragmentedPart => fragmentedPart.slotType.GetPartTypeOfPartSlot() == PBPartType.Body).instanceParts[0].gameObject;
                chassisGO.SetActive(false);
                Destroy(chassisGO, 1f);
            }

            IEnumerator BuildRobotWithAnimation_CR()
            {
                if (robot.ChassisInstance.PartSO.IsTransformBot)
                    yield break;
                yield return null;
                yield return null;
                int completedCount = 0;
                bool isSpecialChassis = robot.ChassisInstance.PartSO.Cast<PBChassisSO>().IsSpecial;
                StartCoroutine(BuildRobotWithAnimationByPartTypeP1_CR(PBPartType.Wheels, GetWheelDirection() * 3f, Space.Self, () => completedCount++));
                StartCoroutine(BuildRobotWithAnimationByPartTypeP1_CR(PBPartType.Front, Vector3.forward * 3.5f, Space.Self, () => completedCount++));
                StartCoroutine(BuildRobotWithAnimationByPartTypeP1_CR(PBPartType.Upper, Vector3.up * 4f, Space.World, () => completedCount++));
                StartCoroutine(BuildRobotWithAnimationByPartTypeP1_CR(PBPartType.Body, Vector3.zero, Space.Self, () => completedCount++));
                yield return new WaitUntil(() => completedCount >= 4);
                yield return BuildRobotWithAnimationByPartTypeP2_CR(PBPartType.Body, Vector3.zero, Space.Self);
                yield return BuildRobotWithAnimationByPartTypeP2_CR(PBPartType.Wheels, GetWheelDirection() * 3f, Space.Self);
                yield return BuildRobotWithAnimationByPartTypeP2_CR(PBPartType.Front, Vector3.forward * 3.5f, Space.Self);
                yield return BuildRobotWithAnimationByPartTypeP2_CR(PBPartType.Upper, Vector3.up * 4f, Space.World, true);

                Vector3 GetWheelDirection()
                {
                    if (isSpecialChassis && robot.ChassisInstance.GetComponentInChildren<LegsAnimator>() != null)
                    {
                        return Vector3.up;
                    }
                    return Vector3.right;
                }

                IEnumerator BuildRobotWithAnimationByPartTypeP1_CR(PBPartType partType, Vector3 direction, Space space, Action callback)
                {
                    bool isCompleted = true;
                    foreach (var fragmentedPart in fragmentedParts)
                    {
                        if (fragmentedPart.slotType.GetPartTypeOfPartSlot() != partType)
                            continue;
                        isCompleted = false;
                        Vector3 originDirection = direction;
                        Transform animatedChasssisTransform = animatedRobot.transform.GetChild(0);
                        Transform targetChassisTrasnform = robot.PartInstances.Find(item => item.PartSlotType == PBPartSlot.Body).Parts[0].transform.GetChild(0);
                        List<PBPart> targetSimulatedParts = robot.PartInstances.Find(item => item.PartSlotType == fragmentedPart.slotType).Parts;
                        for (int i = 0; i < fragmentedPart.instanceParts.Count; i++)
                        {
                            GameObject animatedPartGO = fragmentedPart.instanceParts[i];
                            GameObject targetPartGO = targetSimulatedParts[i].gameObject;
                            if (partType == PBPartType.Body)
                                targetPartGO = targetPartGO.transform.GetChild(0).gameObject;
                            if (partType == PBPartType.Front || partType == PBPartType.Upper)
                                targetPartGO = targetPartGO.GetComponentInChildren<Rigidbody>().gameObject;
                            Sequence sequence = DOTween.Sequence();
                            if (partType == PBPartType.Body)
                            {
                                Vector3 originPosition = animatedPartGO.transform.position;
                                if (animatedPartGO.transform.position.y <= 3f)
                                {
                                    originPosition.y = Mathf.Min(originPosition.y + 5f, 5f);
                                    sequence.Join(animatedPartGO.transform.DOMoveY(Mathf.Min(animatedPartGO.transform.position.y + 5f, 5f), AnimationDuration.SSHORT / animTimeScale).SetEase(Ease.InOutBack).SetUpdate(true));
                                }
                                float animDuration = Mathf.Min(0.1f * Vector3.Distance(originPosition, targetPartGO.transform.position), AnimationDuration.SHORT) / animTimeScale;
                                sequence
                                    .AppendInterval(AnimationDuration.SSHORT / animTimeScale)
                                    .Append(animatedPartGO.transform.DOMove(targetPartGO.transform.position, animDuration).SetEase(Ease.InOutBack).SetUpdate(true))
                                    .Join(animatedPartGO.transform.DORotate((targetPartGO.transform.rotation * Quaternion.Euler(0f, 0f, 180f)).eulerAngles, animDuration).SetUpdate(true))
                                    .OnComplete(() =>
                                    {
                                        isCompleted = true;
                                    });
                            }
                            else
                            {
                                if (partType == PBPartType.Wheels)
                                    direction = targetPartGO.transform.parent.name[^1] == 'L' ? -originDirection : originDirection;
                                Vector3 position = targetPartGO.transform.position + (space == Space.Self ? targetPartGO.transform.rotation * direction : targetChassisTrasnform.rotation * direction);
                                float delayTime = UnityEngine.Random.Range(0.1f, 0.5f) / animTimeScale;
                                float animDuration = Mathf.Min(0.1f * Vector3.Distance(animatedPartGO.transform.position, position), AnimationDuration.SHORT) / animTimeScale;
                                sequence
                                    .AppendInterval(delayTime)
                                    .Append(animatedPartGO.transform.DOMove(position, animDuration).SetEase(Ease.InOutBack).SetUpdate(true))
                                    .Join(animatedPartGO.transform.DORotate(targetPartGO.transform.eulerAngles, animDuration).SetUpdate(true))
                                    .Join(CreateChildReposition(animatedPartGO, targetPartGO))
                                    .OnComplete(() =>
                                    {
                                        isCompleted = true;
                                    });

                                Sequence CreateChildReposition(GameObject animatedPartGO, GameObject targetPartGO, Space space = Space.World)
                                {
                                    var sequence = DOTween.Sequence();
                                    for (int j = 0; j < targetPartGO.transform.childCount; j++)
                                    {
                                        sequence
                                            .Join(CreateChildReposition(animatedPartGO.transform.GetChild(j).gameObject, targetPartGO.transform.GetChild(j).gameObject, Space.Self));
                                    }
                                    if (space == Space.World)
                                    {
                                        sequence
                                            .Join(animatedPartGO.transform.DOMove(targetPartGO.transform.position, 0.1f))
                                            .Join(animatedPartGO.transform.DORotate(targetPartGO.transform.eulerAngles, 0.1f));
                                    }
                                    else
                                    {
                                        sequence
                                            .Join(animatedPartGO.transform.DOLocalMove(targetPartGO.transform.localPosition, 0.1f))
                                            .Join(animatedPartGO.transform.DOLocalRotate(targetPartGO.transform.localEulerAngles, 0.1f));
                                    }
                                    return sequence;
                                }
                            }
                            sequence
                                .SetUpdate(true)
                                .Play();
                        }
                    }
                    if (!isCompleted)
                        yield return new WaitUntil(() => isCompleted);
                    callback?.Invoke();
                }

                IEnumerator BuildRobotWithAnimationByPartTypeP2_CR(PBPartType partType, Vector3 direction, Space space, bool isTheLast = false)
                {
                    Transform chassisTransform = fragmentedParts.FirstOrDefault(fragmentedPart => fragmentedPart.slotType.GetPartTypeOfPartSlot() == PBPartType.Body).instanceParts[0].transform;
                    List<bool> isCompletedList = new List<bool>();
                    foreach (var fragmentedPart in fragmentedParts)
                    {
                        if (fragmentedPart.slotType.GetPartTypeOfPartSlot() != partType)
                            continue;
                        Vector3 originDirection = direction;
                        List<PBPart> targetSimulatedParts = robot.PartInstances.Find(item => item.PartSlotType == fragmentedPart.slotType).Parts;
                        isCompletedList = new List<bool>(new bool[fragmentedPart.instanceParts.Count].FillAll(false));
                        for (int i = 0; i < fragmentedPart.instanceParts.Count; i++)
                        {
                            int index = i;
                            GameObject animatedPartGO = fragmentedPart.instanceParts[i];
                            GameObject targetPartGO = targetSimulatedParts[i].gameObject;
                            if (partType == PBPartType.Body)
                                targetPartGO = targetPartGO.transform.GetChild(0).gameObject;
                            if (partType == PBPartType.Front || partType == PBPartType.Upper)
                                targetPartGO = targetPartGO.GetComponentInChildren<Rigidbody>().gameObject;
                            if (partType == PBPartType.Wheels)
                                direction = (targetPartGO.name.ToUpper()[^1] == 'L' || targetPartGO.transform.parent.name.ToUpper()[^1] == 'L') ? -originDirection : originDirection;
                            var sequence = DOTween.Sequence();
                            if (partType == PBPartType.Body)
                            {
                                sequence
                                    .Append(animatedPartGO.transform.DORotate(targetPartGO.transform.eulerAngles, 0.125f / animTimeScale).SetEase(Ease.OutCubic).SetUpdate(true))
                                    .OnComplete(() =>
                                    {
                                        isCompletedList[index] = true;
                                    });
                            }
                            else
                            {
                                animatedPartGO.transform.SetParent(chassisTransform);
                                if (isSpecialChassis && chassisInstance.GetComponentInChildren<LegsAnimator>() == null && animatedPartGO.GetComponentsInChildren<Renderer>().Where(renderer => renderer is not ParticleSystemRenderer && renderer.enabled).ToArray().Length <= 0)
                                {
                                    isCompletedList[index] = true;
                                    continue;
                                }
                                sequence
                                    .Append(animatedPartGO.transform.DOLocalMove(animatedPartGO.transform.parent.InverseTransformPoint(targetPartGO.transform.position), AnimationDuration.TINY / animTimeScale).SetEase(Ease.InBack).SetUpdate(true)
                                        .OnComplete(() =>
                                        {
                                            if (!isTheLast)
                                                isCompletedList[index] = true;
                                            var particleInstance = Instantiate(constructPartParticlePrefab, animatedRobot.transform);
                                            particleInstance.transform.position = targetPartGO.transform.position;
                                            particleInstance.Play();
                                            Destroy(particleInstance.gameObject, 2f);
                                        }))
                                    .AppendInterval(0f)
                                    .Join(chassisTransform.DOPunchPosition((partType == PBPartType.Wheels ? Vector3.up : -direction.normalized) * 0.15f, AnimationDuration.SSHORT / animTimeScale, 2, 1).SetUpdate(true))
                                    .OnComplete(() =>
                                    {
                                        if (isTheLast)
                                            isCompletedList[index] = true;
                                    });
                            }
                            sequence
                                .SetUpdate(true)
                                .Play();
                        }
                    }
                    if (!IsCompleted())
                        yield return new WaitUntil(() => IsCompleted());

                    bool IsCompleted()
                    {
                        foreach (var isCompleted in isCompletedList)
                        {
                            if (!isCompleted)
                                return false;
                        }
                        return true;
                    }
                }
            }
        }
    }
}