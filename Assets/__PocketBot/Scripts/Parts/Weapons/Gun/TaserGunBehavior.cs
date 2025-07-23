using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using UnityEngine;

public class TaserGunBehavior : PartBehaviour
{
    [SerializeField] private float stunTime = 1.5f;
    [SerializeField] private ParticleSystem _beamVFX;
    [SerializeField] private ParticleSystem _triggerVFX;
    [SerializeField] private Collider _triggerCollider;
    [SerializeField] private OnTriggerCallback _triggerCallback;

    private Dictionary<PBRobot, ParticleSystem> _vfxTriggerSave;

    private void Awake()
    {
        _vfxTriggerSave = new Dictionary<PBRobot, ParticleSystem>();
        _triggerCallback.isFilterByTag = false;
        _triggerCallback.onTriggerEnter += OnTriggerEnterCallback;
        // _triggerCallback.onTriggerExit += OnTriggerExitCallback;
    }

    private void OnDestroy()
    {
        _triggerCallback.onTriggerEnter -= OnTriggerEnterCallback;
    }

    private void OnTriggerEnterCallback(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out PBPart part))
        {
            var carPhysics = part.RobotChassis.CarPhysics;
            if (!_vfxTriggerSave.ContainsKey(part.RobotChassis.Robot))
            {
                if (carPhysics.TryGetComponentInChildren(out SkinnedMeshRenderer _))
                {
                    AddParticleSystem(part.RobotChassis.Robot, ParticleSystemShapeType.SkinnedMeshRenderer);
                }
                else
                {
                    AddParticleSystem(part.RobotChassis.Robot, ParticleSystemShapeType.MeshRenderer);
                }
            }

            pbPart.RobotChassis.Robot.StartCoroutine(TriggerDamageAndStun_CR(other, part));
        }
    }

    private IEnumerator TriggerDamageAndStun_CR(Collider other, PBPart part)
    {
        ParticleSystem stunVfx = _vfxTriggerSave[part.RobotChassis.Robot];
        stunVfx.Play();
        _beamVFX.Stop();
        _triggerCollider.enabled = false;
        PBRobot affectedRobot = part.RobotChassis.Robot;
        affectedRobot.CombatEffectController.ApplyEffect(new StunEffect(stunTime, false, PbPart.RobotChassis.Robot));
        affectedRobot.CombatEffectController.ApplyEffect(new DisarmEffect(stunTime, false, PbPart.RobotChassis.Robot));
        var collisionInfo = new CollisionInfo(other.attachedRigidbody, Vector3.zero, other.ClosestPoint(_triggerCollider.transform.position));
        GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, PbPart, collisionInfo, 1);
        yield return new WaitForSeconds(stunTime);
        stunVfx.Stop();
        yield return new WaitForSeconds(attackCycleTime - stunTime);
        _beamVFX.Play();
        _triggerCollider.enabled = true;
    }

    private void AddParticleSystem(PBRobot robot, ParticleSystemShapeType shapeType)
    {
        ParticleSystem vfxTrigger = Instantiate(_triggerVFX.gameObject, transform).GetComponent<ParticleSystem>();
        var electricVFXShape = vfxTrigger.shape;
        electricVFXShape.shapeType = shapeType;

        _vfxTriggerSave.Add(robot, vfxTrigger);

        if (shapeType == ParticleSystemShapeType.MeshRenderer)
        {
            MeshRenderer meshRendererTrigger = robot.ChassisInstance.CarPhysics.GetComponentInChildren<MeshRenderer>();
            if (meshRendererTrigger != null)
                electricVFXShape.meshRenderer = meshRendererTrigger;
        }
        else if (shapeType == ParticleSystemShapeType.SkinnedMeshRenderer)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = robot.ChassisInstance.CarPhysics.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
                electricVFXShape.skinnedMeshRenderer = skinnedMeshRenderer;
        }
    }

    // private void OnTriggerExitCallback(Collider other)
    // {
    //     if (other.attachedRigidbody.TryGetComponent(out PBPart part))
    //     {
    //         var carPhysics = part.RobotChassis.CarPhysics;
    //         if (_vfxTriggerSave.ContainsKey(carPhysics.name))
    //         {
    //             ParticleSystem vfxTrigger = _vfxTriggerSave[carPhysics.name];
    //             if (vfxTrigger != null)
    //                 vfxTrigger.Stop();
    //         }
    //     }
    // }
}
