using System;
using System.Linq;
using UnityEngine;

public enum CosmeticType
{
    SantaHat,
}

[Serializable]
public struct Cosmetic
{
    public string Name;
    public CosmeticType Type;
    public GameObject Mesh;
    public GameObject Root;
}

public class PlayerCosmetics : MonoBehaviour
{
    [SerializeField]
    private Cosmetic[] headwear;

    private void Start()
    {
        foreach (var cosmetic in headwear)
            cosmetic.Root.SetActive(false);

        var now = DateTime.Now;
        var isChristmas = new DateTime(now.Year, 12, 23) <= now && now <= new DateTime(now.Year + 1, 1, 2);
        if (isChristmas)
            headwear.Single(c => c.Type is CosmeticType.SantaHat).Root.SetActive(true);
    }

    public void SetLayer(int layer)
    {
        foreach (var cosmetic in headwear)
            cosmetic.Mesh.layer = layer;
    }
}
