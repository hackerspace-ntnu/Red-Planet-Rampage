using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class CombinationStats
{
    public CombinationStats(Item body, Item barrel, int killCount = 0)
    {
        Body = body.id;
        Barrel = barrel.id;
        Extension = "None";
        KillCount = killCount;
    }

    public CombinationStats(Item body, Item barrel, Item extension, int killCount = 0)
    {
        Body = body.id;
        Barrel = barrel.id;
        Extension = extension ? extension.id : "None";
        KillCount = killCount;
    }

    public string Body;
    public string Barrel;
    public string Extension;
    public int KillCount;
}

// Only for setting Item objects in editor, which unfortunately cannot be serialized to binary
[System.Serializable]
public struct EditorCombinationWins
{
    public Item Body;
    public Item Barrel;
    public Item Extension;
    public int KillCount;
}

// Container for List of structs
[System.Serializable]
public class PersistentData
{
    public List<CombinationStats> Data = new();
}

public class PersistentInfo : MonoBehaviour
{
    private const string FileName = "/PersistentInfo.dat";
    private static string FilePath => Application.persistentDataPath + FileName;

    public static PersistentInfo Singleton { get; private set; }

    [SerializeField]
    private EditorCombinationWins[] DefaultCombinationStats;

    public static List<CombinationStats> CombinationStats;
    private static Dictionary<(string, string, string), CombinationStats> combinationStatsLookup;

    private void Start()
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

        if (!File.Exists(FilePath))
        {
            CreateDefaultFile();
        }
        LoadPersistentFile();
    }

    #region I/O

    private void LoadPersistentFile()
    {
        BinaryFormatter persistentDataFormatter = new();
        FileStream persistentDataStream = File.Open(FilePath, FileMode.Open);
        List<CombinationStats> weaponData = new();
        try
        {
            weaponData = ((PersistentData)persistentDataFormatter.Deserialize(persistentDataStream)).Data;
            Debug.Log($"Loaded stats for {weaponData.Count} combinations");
        }
        catch
        {
            // TODO: Give users feedback that their persistent data file is corrupt!
            Debug.Log("File empty or corrupt, resetting to default");
            persistentDataStream.Close();
            CreateDefaultFile();
            persistentDataStream = File.Open(FilePath, FileMode.Open);
            weaponData = ((PersistentData)persistentDataFormatter.Deserialize(persistentDataStream)).Data;
        }

        persistentDataStream.Close();
        CombinationStats = weaponData.OrderByDescending(data => data.KillCount).ToList();
        combinationStatsLookup = new(weaponData.Select(d => KeyValuePair.Create((d.Body, d.Barrel, d.Extension), d)));
    }

    private void CreateDefaultFile()
    {
        BinaryFormatter persistentDataFormatter = new();
        FileStream persistentDataStream = File.Create(FilePath);
        var dataContainer = new PersistentData();
        DefaultCombinationStats.ToList()
            .Select(entry => new CombinationStats(entry.Body, entry.Barrel, entry.Extension, entry.KillCount))
            .ToList()
            .ForEach(entry => dataContainer.Data.Add(entry));
        persistentDataFormatter.Serialize(persistentDataStream, dataContainer);
        persistentDataStream.Close();
    }

    // TODO Reset if match is aborted?
    public static void SavePersistentData()
    {
        BinaryFormatter persistentDataFormatter = new();
        FileStream persistentDataStream = File.OpenWrite(FilePath);
        var dataContainer = new PersistentData();
        CombinationStats.ForEach(entry => dataContainer.Data.Add(entry));
        persistentDataFormatter.Serialize(persistentDataStream, dataContainer);
        persistentDataStream.Close();
    }

    #endregion I/O

    public static void RegisterKill(PlayerIdentity identity)
    {
        RegisterKill(identity.Body, identity.Barrel, identity.Extension);
    }

    public static void RegisterKill(Item body, Item barrel, Item extension)
    {
        var key = (body.id, barrel.id, extension ? extension.id : "None");
        if (combinationStatsLookup.TryGetValue(key, out var stats))
        {
            stats.KillCount += 1;
        }
        else
        {
            stats = new CombinationStats(body, barrel, extension)
            {
                KillCount = 1
            };
            combinationStatsLookup.Add(key, stats);
            CombinationStats.Add(stats);
        }
    }
}
