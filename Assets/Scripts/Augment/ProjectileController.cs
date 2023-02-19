using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ProjectileState
{
    // Used for lerping
    public Vector3 olderPosition;
    public Vector3 oldPosition;

    // Position of the bullet
    public Vector3 position;

    // Normalized direction of projectile
    public Vector3 direction;

    // Speed of the bullet
    public float speed;

    // Direction bullet is pointing
    public Quaternion rotation;

    // Current gravity
    public float gravity;

    // Set to false if bullet should no longer hit stuff
    public bool collisionActive;

    // Used for Lerping
    public float lastUpdateTime;
    public ProjectileState(Vector3 position, Quaternion rotation, Vector3 direction, float speed, float gravityModifier)
    {
        this.position = position;
        this.oldPosition = position;
        this.olderPosition = position;
        this.rotation = rotation;
        this.direction = direction;
        this.speed = speed;
        this.collisionActive = true;
        this.gravity = Physics.gravity.y * gravityModifier;
        this.lastUpdateTime = Time.fixedTime;
    }
}

public class ProjectileController : MonoBehaviour
{
    public ProjectileState state;


    [HideInInspector]
    public GunStats stats;
    // Delegates and Events

    // Used for describing how a projectile moves when asked to move a specific distance 
    [System.Serializable]
    public delegate void PathUpdateEvent(float distance, ref ProjectileState state, GunStats stats);
    [SerializeField]
    public PathUpdateEvent UpdateProjectileMovement;

    // Used for adding events when the projectile position is updated, like particle trails
    public delegate void PositionUpdateEvent(Vector3 oldPos, Vector3 newPos, ref ProjectileState state, GunStats stats);

    public PositionUpdateEvent OnBulletTravel;

    // Used whenever a projectile hits any hitbox
    public delegate void HitboxInteraction(HitboxController controller, ref ProjectileState state, GunStats stats);

    public HitboxInteraction OnHitboxCollision;

    // Used whenever a projectile hits any collider, though 
    public delegate void CollisionEvent(Collider other, ref ProjectileState state, GunStats stats);

    public CollisionEvent OnColliderHit;

    public float lifeTime = 2f;

    public virtual void Start()
    {
        state = new ProjectileState(transform.position, transform.rotation, transform.forward, stats.ProjectileSpeed, stats.ProjectileGravityModifier);
        Destroy(gameObject, lifeTime);
    }

    public Vector3 LerpPos()
    {
        var diff = (Time.time - state.lastUpdateTime) / Time.fixedDeltaTime;
        return Vector3.Lerp(state.oldPosition, state.position, diff);
    }


    //public Vector3 QerpPos()
    //{
    //    var diff = (Time.time - state.lastUpdateTime) / Time.fixedDeltaTime;
    //
    //    return ((state.olderPosition + state.position) * 0.5f - state.oldPosition) * diff * diff +
    //        (state.position - state.olderPosition) * diff + state.oldPosition;
    //}
}
