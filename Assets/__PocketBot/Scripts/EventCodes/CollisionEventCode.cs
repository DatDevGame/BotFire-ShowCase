using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;

[EventCode]
public enum CollisionEventCode
{
    /// <summary>
    /// Raised when a part collide with another part
    /// <para> <typeparamref name="PBPart"/>: Part that collide (Attacker)</para>
    /// <para> <typeparamref name="Collision/Collider"/>: Collision Info</para>
    /// <para> <typeparamref name="Float"/>: Force that part apply to body</para>
    /// <para> <typeparamref name="Action(Rigidbody, CollisionInfo)"/>: Apply collision force callback</para>
    /// </summary>
    OnPartCollide,
}
