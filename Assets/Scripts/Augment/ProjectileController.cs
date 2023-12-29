using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    // Stat modifier of the projectile speed
    public float speedFactor = 1f;

    // Rotation of the bullet, typically used for vfx, such as aligning a rocket
    public Quaternion rotation = Quaternion.identity;

    // Current gravity
    public float gravity = 0f;

    // Set to false if bullet should no longer hit stuff
    public bool collisionActive = true;

    // Used for Lerping
    public float lastUpdateTime = 0f;

    // If the projectile is being used or not
    public bool active = false;

    public float damage = 0f;

    // TODO: Make this to anything
    public float size = 0f;

    // Dictionary for storing properties that a projectile modifier might need, see SpiralPathModifier for an example
    public Dictionary<string, object> additionalProperties = new Dictionary<string, object>();

    // Used to keep track of the healthControllers currently damaged
    public HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();

    public ProjectileState(GunStats stats, Transform output)
    {
        initializationTime = Time.fixedTime;
        lastUpdateTime = Time.fixedTime;

        position = output.position;
        oldPosition = output.position;
        direction = output.forward;
        rotation = output.rotation;

        speedFactor = stats.ProjectileSpeedFactor;
        gravity = stats.ProjectileGravityModifier * 9.81f;
        damage = stats.ProjectileDamage;
    }
    public ProjectileState() { }
}

public abstract class ProjectileController : MonoBehaviour
{
    [HideInInspector]
    public Transform projectileOutput;
    public Quaternion projectileRotation = Quaternion.identity;

    // Used for muzzle-flashes and other effects, is not where the projectile path actually starts
    protected Transform effectOutput;

    // Projectiles should hit Default and HitBox
    protected LayerMask collisionLayers;

    [HideInInspector]
    public GunStats stats;
    // Delegates and Events

    // The player shooting the projectile
    [HideInInspector]
    public PlayerManager player;

    // PLEASE READ
    // This is how the event-system of the guns work, all of these delegate are "hooks" that additional effects can be applied to
    // Each implementation of a projectile type must also describe when these events are triggered
    // This base class never actually TRIGGERES the events, subclasses have to trigger them, ( See BulletController )

    // Used for describing how a projectile moves when asked to move a specific distance 
    [System.Serializable]
    public delegate void PathUpdateEvent(float distance, ref ProjectileState state);

    [SerializeField]
    public PathUpdateEvent UpdateProjectileMovement;

    // Used for modifications done to the projectile upon creation
    public delegate void ProjectileInitializationEvent(ref ProjectileState state, GunStats stats);
    public ProjectileInitializationEvent OnProjectileInit;

    // Used for adding events when the projectile position is updated, like particle trails
    public delegate void PositionUpdateEvent(ref ProjectileState state);
    public PositionUpdateEvent OnProjectileTravel;

    // Used whenever a projectile hits any hitbox
    public delegate void HitboxInteraction(HitboxController controller, ref ProjectileState state);
    public HitboxInteraction OnHitboxCollision;

    // Used whenever a projectile hits any collider, though 
    public delegate void CollisionEvent(Collider other, ref ProjectileState state);
    public CollisionEvent OnColliderHit;
    public CollisionEvent OnRicochet;

    protected GunController gunController;

    protected virtual void Awake()
    {
        collisionLayers = LayerMask.GetMask("Default", "HitBox");

        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Barrel not attached to gun parent!");
            return;
        }
        if (!gunController.Player)
        {
            return;
        }
        gunController.onInitializeGun += OnInitialize;
        gunController.onReload += OnReload;
    }

    protected virtual void OnDestroy()
    {
        if (!gunController) return;
        gunController.onInitializeGun -= OnInitialize;
        gunController.onReload -= OnReload;
    }


    protected abstract void OnInitialize(GunStats stats);

    protected abstract void OnReload(GunStats stats);

    // The meat and potatoes of the gun, this is what initializes a "bullet", whatever the fuck that is supposed to mean
    // Again, subclasses decide for themselves what initializing a bullet does
    public abstract void InitializeProjectile(GunStats stats);
}



public class ProjectileMotions
{
    public static void MoveWithGravity(float distance, ref ProjectileState state)
    {
        //Update the position of the projectile
        state.position += state.direction * distance;

        //Update the velocity of the projectile
        float time = distance / state.speed;
        Vector3 velocity = state.direction * state.speed;
        velocity += Vector3.down * state.gravity * time;
        state.speed = velocity.magnitude;
        state.direction = velocity.normalized;
        state.distanceTraveled += distance;
    }

    public static RaycastHit[] GetPathCollisions(ProjectileState state, LayerMask collisionLayers)
    {
        var direction = state.position - state.oldPosition;
        RaycastHit[] rayCasts;

        if (state.size > 0)
        {
            rayCasts = Physics.SphereCastAll(state.oldPosition, state.size, direction, direction.magnitude, collisionLayers);
        }
        else
        {
            rayCasts = Physics.RaycastAll(state.oldPosition, direction, direction.magnitude, collisionLayers);
        }

        return rayCasts.OrderBy(x => x.distance).ToArray();
    }

}
