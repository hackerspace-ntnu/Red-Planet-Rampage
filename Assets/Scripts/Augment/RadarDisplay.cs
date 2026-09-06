using UnityEngine;
using VectorExtensions;

// Kinda stupid to have it as proj modifier but low effort goes...
public class RadarDisplay : MonoBehaviour, ProjectileModifier
{
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private int materialIndex = 1;

    private Material material;
    private PlayerManager player;

    private static readonly int[] PlayerPositionProps =
    {
        Shader.PropertyToID("_Player1Pos"),
        Shader.PropertyToID("_Player2Pos"),
        Shader.PropertyToID("_Player3Pos"),
        Shader.PropertyToID("_Player4Pos"),
    };

    private void Start()
    {
        mesh.materials[materialIndex] = Instantiate(mesh.materials[materialIndex]);
        material = mesh.materials[materialIndex];
    }


    public void Attach(ProjectileController projectile)
    {
        if (!projectile.player)
            return;
        player = projectile.player;
    }

    public void Detach(ProjectileController projectile)
    {
        // nada haha
    }

    private void Update()
    {
        if (!player || !MatchController.Singleton)
            return;
        for (var i = 0; i < MatchController.Singleton.Players.Count; i++)
        {
            var enemy = MatchController.Singleton.Players[i];
            if (enemy == player || !enemy.IsAlive)
            {
                material.SetVector(PlayerPositionProps[i], new Vector2(-1000, -1000));
                continue;
            }
            var playerPos = enemy.transform.position;
            var radarPos = playerPos - player.transform.position;
            radarPos = (Quaternion.AngleAxis(90, Vector3.up) * Quaternion.Inverse(player.transform.rotation)) * radarPos;
            // radarPos = Quaternion.AngleAxis(-90, Vector3.up) * player.transform.rotation * radarPos;
            material.SetVector(PlayerPositionProps[i], radarPos.xz() / 50f);
        }
        for (var i = MatchController.Singleton.Players.Count; i < 4; i++)
        {
            material.SetVector(PlayerPositionProps[i], new Vector2(-1000, -1000));
        }
    }
}
