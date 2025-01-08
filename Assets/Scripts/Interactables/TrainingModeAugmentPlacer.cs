using System.Linq;
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
        SpawnAugments();
    }

    private void SpawnAugments()
    {
        SpawnLine(transform.position, StaticInfo.Singleton.Bodies);
        SpawnLine(transform.position + transform.rotation * columnSpace, StaticInfo.Singleton.Barrels);
        SpawnLine(transform.position + transform.rotation * columnSpace * 2, StaticInfo.Singleton.Extensions);
    }

    private void SpawnLine(Vector3 position, ReadOnlyArray<Item> items)
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
