using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConnectedPart : PBPart
{
    [SerializeField] List<Collider> detachedColliders = new List<Collider>();
    [SerializeField] List<Component> componentsToBeRemoved = new List<Component>();

    private PBChassis pbChassis;

    public virtual PBRobot Robot
    {
        get
        {
            if (RobotChassis == null)
                return null;
            return RobotChassis.Robot;
        }
    }
    public override Rigidbody RobotBaseBody
    {
        get
        {
            return RobotChassis.RobotBaseBody;
        }
    }
    [ShowInInspector]
    public override PBChassis RobotChassis
    {
        get
        {
            if (pbChassis == null)
            {
                pbChassis = GetComponentInParent<PBChassis>();
            }
            return pbChassis;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (RobotChassis == null) return;
        if (TryGetComponent<CarPhysics>(out _)) return; //If chassis body -> return
        if (Robot != null)
        {
            Robot.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDestroy()
    {
        if (Robot)
        {
            Robot.OnHealthChanged -= HandleHealthChanged;
        }
    }

    void HandleHealthChanged(Competitor.HealthChangedEventData eventData)
    {
        if (eventData.MaxHealth <= 0) return; //Not yet setup
        if (eventData.CurrentHealth <= 0) //Bot died
        {
            Robot.OnHealthChanged -= HandleHealthChanged;
            DetachFromChassis();
        }
    }

    void DetachFromChassis()
    {
        RemoveJoints();
        RemoveColliders();
        RemovePartBehaviours();
        RemoveRigidbodys();
        RemoveComponents();

        void RemoveJoints()
        {
            var joints = GetComponentsInChildren<Joint>();
            foreach (var joint in joints)
            {
                Destroy(joint);
            }
        }

        void RemoveColliders()
        {
            if (detachedColliders == null || detachedColliders.Count <= 0)
                return;
            var removeColliders = new List<Collider>(GetComponentsInChildren<Collider>());

            foreach (var collider in detachedColliders)
            {
                if (collider != null)
                {
                    removeColliders.Remove(collider);
                    collider.enabled = true;
                }
            }

            foreach (var collider in removeColliders)
            {
                if(collider != null)
                    Destroy(collider);
            }
        }

        void RemovePartBehaviours()
        {
            var behaviours = GetComponentsInChildren<PartBehaviour>();
            foreach (var behaviour in behaviours)
            {
                Destroy(behaviour);
            }
        }

        void RemoveRigidbodys()
        {
            var rigidbodies = GetComponentsInChildren<Rigidbody>();
            if (TryGetComponent<Rigidbody>(out var rb) == false)
                rb = gameObject.AddComponent<Rigidbody>();
            foreach (var rigidbody in rigidbodies)
            {
                if (rigidbody == rb) continue;
                Destroy(rigidbody);
            }
            rb.useGravity = true;
            rb.drag = 1;
            rb.mass = 1;
            var forceDir = transform.position - RobotBaseBody.transform.position;
            rb.AddForce(forceDir.normalized * 10f, ForceMode.Impulse);
        }

        void RemoveComponents()
        {
            foreach (var component in componentsToBeRemoved)
            {
                Destroy(component);
            }
        }
    }
}
