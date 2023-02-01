using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class BulletController : ProjectileController
{
    
    public static void MoveWithGravity(float distance, ref ProjectileState state, GunStats stats)
    {
        //Update the position of the projectile
        state.position += state.direction * distance;

        //Update the velocity of the projectile
        float time = distance / state.speed;
        Vector3 velocity = state.direction * state.speed;
        velocity += Vector3.up * state.gravity * time;
        state.speed = velocity.magnitude;
        state.direction = velocity.normalized;
        
    }

    public override void Start()
    {
        base.Start();
        UpdateProjectileMovement += MoveWithGravity;
    }

    private void FixedUpdate()
    {
        state.lastUpdateTime = Time.time;
        state.olderPosition = state.oldPosition;
        state.oldPosition = state.position;

        Collider[] collisions;

        UpdateProjectileMovement?.Invoke(state.speed * Time.fixedDeltaTime, ref state, stats);

        var direction = state.position - state.oldPosition;
        RaycastHit[] rayCasts;
        if (stats.ProjectileSize > 0)
        {
            rayCasts = Physics.SphereCastAll(transform.position, stats.ProjectileSize, direction, direction.magnitude);
        }
        else
        {
            rayCasts = Physics.RaycastAll(transform.position, direction, direction.magnitude);
        }
        // Ordered by distance along path hit, so that things are hit in the correct order
        collisions = rayCasts.OrderBy(x => x.distance).Select(x => x.collider).ToArray();
        foreach(var collision in collisions)
        {

            OnColliderHit?.Invoke(collision, ref state, stats);
            HitboxController hitbox = collision.GetComponent<HitboxController>();

            if (hitbox != null){
                OnHitboxCollision?.Invoke(hitbox, ref state, stats);
            }
        }
        
    }
    public void Update()
    {
        transform.position = LerpPos();
    }

}

