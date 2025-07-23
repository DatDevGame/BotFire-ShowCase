using System.Collections;

using HyrphusQ.Events;

using LatteGames.Template;

using UnityEngine;

public enum HarpoonBulletState
{
    OnHome,
    Launching,
    Revoking,
    Pulling
}
public class HarpoonBulletCollider : ImpactCollider
{
    [SerializeField] protected float revokeDuration = 0.5f;
    [SerializeField] protected float launchForce = 20;
    [SerializeField] protected float pullStrength = 20;
    [SerializeField] protected float pullDuration = 5;
    [SerializeField] protected float maxDistance = 10;
    [SerializeField] protected AnimationCurve revokeCurve;
    [SerializeField] protected AnimationCurve pullStrengthCurve;
    [SerializeField] protected Transform bezierMiddlePoint;
    [SerializeField] protected LineRenderer ropeLineRenderer;

    protected Rigidbody rb;
    protected Coroutine coroutine;
    protected Transform parent, chassis;
    protected Collider m_Collider;
    protected CarPhysics targetCarPhysics = null;
    protected PBRobot targetRobot = null;
    protected HarpoonBulletState state;
    protected float maxTime => pullDuration + 2;
    protected float launchTimeStamp;
    protected PBPartSkin partSkin;

    public HarpoonBulletState State => state;
    public float MaxDistance => maxDistance;

    protected void Start()
    {
        m_Collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        parent = transform.parent;
        chassis = GetComponentInParent<PBChassis>().transform;
        StartCoroutine(CR_Update());
        partSkin = GetComponent<PBPartSkin>();
        partSkin.part = partBehaviour.PbPart;
    }

    public void Launch(float bodySpeed)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        launchTimeStamp = Time.time;
        transform.SetParent(chassis);

        rb.isKinematic = false;
        rb.useGravity = true;
        m_Collider.enabled = true;
        state = HarpoonBulletState.Launching;
        rb.AddForce(launchForce * bodySpeed * parent.transform.up, ForceMode.Impulse);
    }

    public void PullBack(CarPhysics targetCarPhysics)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(CR_PullBack(targetCarPhysics));
    }

    public void Revoke()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(CR_Revoke());
    }

    protected override void CollisionEnterBehaviour(Collision collision)
    {
        if (state != HarpoonBulletState.Launching)
        {
            return;
        }
        // Match all condition
        if (impactVFX != null)
        {
            impactVFX.transform.position = collision.GetContact(0).point;
            impactVFX.Play();
        }
        SoundManager.Instance.PlaySFX(SFX.AxeHit, PBSoundUtility.IsOnSound() ? 1 : 0);
        if (collision.collider.CompareTag("Ground") || partBehaviour == null)
        {
            Revoke();
            return;
        }
        if (collision.rigidbody == null)
        {
            Revoke();
            return;
        }
        var damageableComponent = collision.rigidbody.GetComponent<IDamagable>();
        if (damageableComponent == null)
        {
            Revoke();
            return;
        }
        if (!partBehaviour.IsAbleToDealDamage(collision, out var queryResult))
        {
            Revoke();
            return;
        }

        if (damageableComponent is ConnectedPart)
        {

            targetCarPhysics = ((ConnectedPart)damageableComponent).RobotChassis.CarPhysics;
            targetRobot = ((ConnectedPart)damageableComponent).RobotChassis.Robot;
        }
        else if (damageableComponent is PBChassis)
        {
            targetCarPhysics = ((PBChassis)damageableComponent).CarPhysics;
            targetRobot = ((PBChassis)damageableComponent).Robot;

        }

        if (targetRobot != null && !targetRobot.IsDead)
        {
            PullBack(targetCarPhysics);
        }
        else
        {
            Revoke();
        }

        partBehaviour.PbPart.DamageMultiplier = queryResult.damageMultiplier;
        GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collision, collisionForce, ApplyCollisionForceCallback);
    }

    IEnumerator CR_Revoke()
    {
        targetCarPhysics = null;
        targetRobot = null;
        state = HarpoonBulletState.Revoking;
        // yield return new WaitForSeconds(AnimationDuration.TINY);
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.SetParent(parent);
        m_Collider.enabled = false;
        var startPos = transform.position;
        var previousPos = transform.position;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / revokeDuration;

            var bezierPos_1 = Vector3.Lerp(startPos, bezierMiddlePoint.position, revokeCurve.Evaluate(t));
            var bezierPos_2 = Vector3.Lerp(bezierMiddlePoint.position, parent.position, revokeCurve.Evaluate(t));
            transform.position = Vector3.Lerp(bezierPos_1, bezierPos_2, revokeCurve.Evaluate(t));
            if (t < 0.9f)
            {
                transform.LookAt(previousPos);
                transform.Rotate(90f, 0f, 0f, Space.Self);
            }
            else
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Mathf.InverseLerp(0.9f, 1f, revokeCurve.Evaluate(t)));
            }
            previousPos = transform.position;
            yield return new WaitForFixedUpdate();
        }
        state = HarpoonBulletState.OnHome;
    }

    IEnumerator CR_PullBack(CarPhysics targetCarPhysics)
    {
        state = HarpoonBulletState.Pulling;
        var targetRB = targetCarPhysics.CarRb;
        // targetCarPhysics.enabled = false;
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.SetParent(chassis);
        m_Collider.enabled = false;
        var localPosInTarget = targetRB.transform.InverseTransformPoint(transform.position);
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / pullDuration;
            Vector3 direction = parent.position - transform.position;
            targetRB.AddForceAtPosition(direction.normalized * pullStrength * pullStrengthCurve.Evaluate((parent.position - transform.position).magnitude), transform.position, ForceMode.Acceleration);
            transform.position = targetRB.transform.TransformPoint(localPosInTarget);
            yield return new WaitForFixedUpdate();
        }
        // targetCarPhysics.enabled = true;
        Revoke();
    }

    IEnumerator CR_Update()
    {
        while (true)
        {
            ropeLineRenderer.SetPosition(0, ropeLineRenderer.transform.InverseTransformPoint(parent.position));
            ropeLineRenderer.SetPosition(1, ropeLineRenderer.transform.InverseTransformPoint(transform.position));
            if (state == HarpoonBulletState.Pulling || state == HarpoonBulletState.Launching)
            {
                if ((parent.position - transform.position).magnitude > maxDistance)
                {
                    Revoke();
                }
                else
                if (Time.time - launchTimeStamp >= maxTime)
                {
                    Revoke();
                }
                else
                if (targetRobot != null && targetRobot.IsDead)
                {
                    Revoke();
                }
                else
                if (!partBehaviour.PbPart.RobotChassis.Robot.IsPreview && partBehaviour.PbPart.RobotChassis.Robot.IsDead)
                {
                    Revoke();
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnDestroy()
    {
        if (ropeLineRenderer != null && ropeLineRenderer != null && parent != null)
        {
            ropeLineRenderer.SetPosition(0, ropeLineRenderer.transform.InverseTransformPoint(parent.position));
            ropeLineRenderer.SetPosition(1, ropeLineRenderer.transform.InverseTransformPoint(parent.position));
        }
    }
}