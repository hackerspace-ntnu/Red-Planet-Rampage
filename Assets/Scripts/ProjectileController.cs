using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ProjectileController : MonoBehaviour
{
    public GunBaseStats stats;

    private Vector3 velocity;
    // Start is called before the first frame update

    private void FixedUpdate()
    {
        Collider[] collisions;
        if (stats.projectileSize > 0)
        {
            // Both of these need to be ordered
            collisions = Physics.OverlapCapsule(transform.position, transform.position + velocity * Time.fixedDeltaTime, stats.projectileSize);
        }
        else
        {
            // Both of these need to be ordered
            RaycastHit[] rayCasts = Physics.RaycastAll(transform.position, velocity * Time.fixedDeltaTime);
            collisions = rayCasts.Select(x => x.collider).ToArray();
        }
        if(collisions.Length > 0)
        {
            // DO shit here with the bullets
            print("HIT!");
        }
        transform.position += velocity * Time.fixedDeltaTime;
        velocity += Physics.gravity * Time.fixedDeltaTime * stats.bulletGravityModifier;
    }
}
