using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimController : MonoBehaviour
{
    static List<AimController> allAimTargets = new();
    [SerializeField] Transform body;
    [SerializeField] float faceToAngleThreshold = 2f;
    [SerializeField] float rotateSpeed = 360f;
    [SerializeField] CircleLineRenderer rangeCircleRenderer;
    [SerializeField] Color farthestColor;
    [SerializeField] Color normalColor;
    [ReadOnly]
    public AimController aimTarget;

    CircleLineRenderer farthestRangeCircleRenderer;
    List<CircleLineRenderer> circleLineRenderers = new List<CircleLineRenderer>();
    List<GunBase> guns = new List<GunBase>();
    float detectRadius = 0;
    bool isActive = true;
    PBRobot robot;
    public PBRobot Robot
    {
        get
        {
            if (robot == null)
            {
                robot = GetComponentInParent<PBRobot>();
            }
            return robot;
        }
    }
    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
        }
    }
    public bool isFacingToTarget => aimTarget != null && Vector3.Angle(transform.forward, transform.position.DiffOnPlane(aimTarget.transform.position)) < faceToAngleThreshold;
    public List<GunBase> Guns => guns;
    public List<CircleLineRenderer> CircleLineRenderers => circleLineRenderers;

    void Start()
    {
        rangeCircleRenderer.gameObject.SetActive(false);
        allAimTargets.Add(this);
        Robot.OnHealthChanged += (eventData) =>
        {
            if (Robot.Health <= 0)
            {
                allAimTargets.Remove(this);
            }
        };
    }

    void OnDestroy()
    {
        allAimTargets.Remove(this);
    }

    private void FixedUpdate()
    {
        if (Robot.IsPreview)
        {
            return;
        }
        if (!IsActive)
        {
            return;
        }
        if (Robot == null || Robot.IsDead)
        {
            return;
        }
        FindTarget();
        if (aimTarget != null)
        {
            if (aimTarget.Robot.IsDead)
            {
                aimTarget = null;
                return;
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.position.DiffOnPlane(aimTarget.transform.position)), rotateSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, body.rotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }

    void FindTarget()
    {
        var nearestDistance = Mathf.Infinity;
        aimTarget = null;
        foreach (AimController head in allAimTargets)
        {
            if (head.Robot.Health > 0 && head.Robot.TeamId != Robot.TeamId)
            {
                var distance = Vector3.Distance(transform.position, head.transform.position);
                if (distance <= detectRadius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    aimTarget = head;
                }
            }
        }
    }

    public void RegisterGun(GunBase gun)
    {
        if (Robot.IsPreview)
        {
            return;
        }
        if (!guns.Contains(gun))
        {
            guns.Add(gun);
            if (detectRadius < gun.aimRange)
            {
                detectRadius = gun.aimRange;
                if (Robot.PersonalInfo.isLocal)
                {
                    if (farthestRangeCircleRenderer != null)
                    {
                        farthestRangeCircleRenderer.SetColor(normalColor);
                    }
                    farthestRangeCircleRenderer = Instantiate(rangeCircleRenderer);
                    farthestRangeCircleRenderer.gameObject.SetActive(true);
                    farthestRangeCircleRenderer.transform.SetParent(rangeCircleRenderer.transform.parent);
                    farthestRangeCircleRenderer.transform.localPosition = rangeCircleRenderer.transform.localPosition;
                    farthestRangeCircleRenderer.transform.localRotation = rangeCircleRenderer.transform.localRotation;
                    farthestRangeCircleRenderer.SetRadius(gun.aimRange);
                    farthestRangeCircleRenderer.SetColor(farthestColor);
                    circleLineRenderers.Add(farthestRangeCircleRenderer);
                }
            }
            else
            {
                if (Robot.PersonalInfo.isLocal)
                {
                    var circleRenderer = Instantiate(rangeCircleRenderer);
                    circleRenderer.gameObject.SetActive(true);
                    circleRenderer.transform.SetParent(rangeCircleRenderer.transform.parent);
                    circleRenderer.transform.localPosition = rangeCircleRenderer.transform.localPosition;
                    circleRenderer.transform.localRotation = rangeCircleRenderer.transform.localRotation;
                    circleRenderer.SetRadius(gun.aimRange);
                    circleRenderer.SetColor(normalColor);
                    circleLineRenderers.Add(circleRenderer);
                }
            }
        }
    }

    public static AimController FindATargetInRange(Vector3 position, float range, int teamId)
    {
        foreach (AimController target in allAimTargets)
        {
            if (target.Robot.Health > 0 && target.Robot.TeamId != teamId)
            {
                var distance = position.DiffOnPlane(target.transform.position).magnitude;
                if (distance <= range)
                {
                    return target;
                }
            }
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
