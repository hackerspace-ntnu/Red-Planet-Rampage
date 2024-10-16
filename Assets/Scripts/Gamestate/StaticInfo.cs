using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using System.Linq;

/// <summary>
/// This class holds static data to act as the single source of constant resources
/// Examples of constant data would be "Which augments is able to be found in the game".
/// This solves problems related to accessing this data in a static way without resorting to Resoure.Load() or similar even worse solutions.
/// This allows us to only have to update the game with a new item (or similar) in a single place, making the development proccess a bit smoother.
/// </summary>
public class StaticInfo : MonoBehaviour
{
    public static StaticInfo Singleton { get; private set; }

    [Header("Available Items")]
    // All items that should populate the game
    [SerializeField]
    private Item[] bodies;
    [SerializeField]
    private Item[] barrels;
    [SerializeField]
    private Item[] extensions;

    [SerializeField]
    private WeightedRandomisedAuctionStage bodyAuction;
    public WeightedRandomisedAuctionStage BodyAuction => bodyAuction;
    [SerializeField]
    private WeightedRandomisedAuctionStage barrelAuction;
    public WeightedRandomisedAuctionStage BarrelAuction => barrelAuction;
    [SerializeField]
    private WeightedRandomisedAuctionStage extensionAuction;
    public WeightedRandomisedAuctionStage ExtensionAuction => extensionAuction;

    [SerializeField]
    private WeightedRandomisedAuctionStage everythingAuction;
    public WeightedRandomisedAuctionStage EverythingAuction => everythingAuction;

    [SerializeField]
    private Item startingBody;
    public Item StartingBody => startingBody;

    [SerializeField]
    private Item startingBarrel;
    public Item StartingBarrel => startingBarrel;

    [SerializeField]
    private Item startingExtension;
    public Item StartingExtension => startingExtension;

    [Header("Override for certain item combinations")]
    // Overrides of secret names given a specific combination
    [SerializeField]
    private SecretNamesStaticStorage secretNames;

    public ReadOnlyArray<Item> Bodies;
    public ReadOnlyArray<Item> Barrels;
    public ReadOnlyArray<Item> Extensions;
    public ReadOnlyArray<OverrideName> SecretNames;
    public ReadOnlyDictionary<string, Item> ItemsById;

    [Header("Settings")]
    [SerializeField]
    private float cameraFov;
    public float CameraFov => cameraFov;

    private void Awake()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate

        Bodies = new ReadOnlyArray<Item>(bodies);
        Barrels = new ReadOnlyArray<Item>(barrels);
        Extensions = new ReadOnlyArray<Item>(extensions);
        SecretNames = new ReadOnlyArray<OverrideName>(secretNames.Overrides);

        var bodiesWithoutStarter = bodies.Where(i => i != startingBody).ToArray();
        var barrelsWithoutStarter = barrels.Where(i => i != startingBarrel).ToArray();
        var extensionsWithoutStarter = extensions.Where(i => i != startingExtension).ToArray();
        bodyAuction.SetItems(bodiesWithoutStarter);
        barrelAuction.SetItems(barrelsWithoutStarter);
        extensionAuction.SetItems(extensionsWithoutStarter);
        everythingAuction.SetItems(bodiesWithoutStarter.Union(barrelsWithoutStarter).Union(extensionsWithoutStarter).ToArray());

        ItemsById = new ReadOnlyDictionary<string, Item>(new Dictionary<string, Item>(bodies.Union(barrels)
            .Union(extensions)
            .Select(item => new KeyValuePair<string, Item>(item.id, item))));
    }
}
