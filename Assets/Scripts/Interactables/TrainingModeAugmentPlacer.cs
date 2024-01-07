using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class TrainingModeAugmentPlacer : MonoBehaviour
{
    [SerializeField]
    private Vector3 columnSpace = 4 * Vector3.left;

    [SerializeField]
    private Vector3 rowSpace = 2 * Vector3.forward;

    [SerializeField]
    private TrainingModeAugment pedestal;

    private void Start()
    {
        var bodies = new List<Item>();
        bodies.Add(StaticInfo.Singleton.StartingBody);
        bodies.AddRange(StaticInfo.Singleton.Bodies);
        SpawnLine(transform.position, bodies);

        var barrels = new List<Item>();
        barrels.Add(StaticInfo.Singleton.StartingBarrel);
        barrels.AddRange(StaticInfo.Singleton.Barrels);
        SpawnLine(transform.position + transform.rotation * columnSpace, barrels);

        var extensions = StaticInfo.Singleton.Extensions.ToList();
        SpawnLine(transform.position + transform.rotation * columnSpace * 2, extensions);
    }

    private void SpawnLine(Vector3 position, List<Item> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            SpawnPedestal(position + transform.rotation * rowSpace * i, items[i]);
        }
    }

    private void SpawnPedestal(Vector3 position, Item item)
    {
        var instance = Instantiate(pedestal, position, transform.rotation, transform);
        instance.Item = item;
    }
}
