using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ProjectileState
{
    // When the projectile was started
    public float initializationTime = 0f;
    
    // How long the projectile should live
    public float lifeTime = 0f;

    // How far the projectile can travel
    public float maxDistance = 0f;

    // How far the projectile has traveled
    public float distanceTraveled = 0f;

    // Used for lerping
    public Vector3 oldPosition = Vector3.zero;

    // Position of the bullet
    public Vector3 position = Vector3.zero;

    // Normalized direction of projectile
    public Vector3 direction = Vector3.zero;

    // Speed of the bullet
    public float speed = 0f;

    // Rotation of the bullet, typically used for vfx, such as aligning a rocket
    public Quaternion rotation = Quaternion.identity;

    // Current gravity
    public float gravity = 0f;

    // Set to false if bullet should no longer hit stuff
    public bool collisionActive = false;

    // Used for Lerping
    public float lastUpdateTime = 0f;

    // If the projectile is being used or not
    public bool active = true;

    // Dictionary for storing properties that a projectile modifier might need, see SpiralPathModifier for an example
    public Dictionary<string, object> additionalProperties = new Dictionary<string, object>();

    // Used to keep track of the healthControllers currently damaged
    public HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();
}

public abstract class ProjectileController : MonoBehaviour
{
    [SerializeField]
    public Transform projectileOutput;

    // Used for muzzle-flashes and other effects, is not where the projectile path actually starts
    protected Transform effectOutput;

    [HideInInspector]
    public GunStats stats;
    // Delegates and Events


    // PLEASE READ
    // This is how the event-system of the guns work, all of these delegate are "hooks" that additional effects can be applied to
    // Each implementation of a projectile type must also describe when these events are triggered
    // This base class never actually TRIGGERES the events, subclasses have to trigger them, ( See BulletController )
    

    // Used for describing how a projectile moves when asked to move a specific distance 
    [System.Serializable]
    public delegate void PathUpdateEvent(float distance, ref ProjectileState state, GunStats stats);
    [SerializeField]
    public PathUpdateEvent UpdateProjectileMovement;
    
    // Used for modifications done to the projectile upon creation
    public delegate void ProjectileInitializationEvent(ref ProjectileState state, GunStats stats);
    public ProjectileInitializationEvent OnProjectileInit;

    // Used for adding events when the projectile position is updated, like particle trails
    public delegate void PositionUpdateEvent(Vector3 oldPos, Vector3 newPos, ref ProjectileState state, GunStats stats);
    public PositionUpdateEvent OnBulletTravel;

    // Used whenever a projectile hits any hitbox
    public delegate void HitboxInteraction(HitboxController controller, ref ProjectileState state, GunStats stats);
    public HitboxInteraction OnHitboxCollision;

    // Used whenever a projectile hits any collider, though 
    public delegate void CollisionEvent(Collider other, ref ProjectileState state, GunStats stats);
    public CollisionEvent OnColliderHit;


    // The meat and potatoes of the gun, this is what initializes a "bullet", whatever the fuck that is supposed to mean
    // Again, subclasses decide for themselves what initializing a bullet does
    public abstract void InitializeProjectile(GunStats stats);
}
