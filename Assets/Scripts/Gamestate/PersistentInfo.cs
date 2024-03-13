using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct CombinationWins
{
    public CombinationWins(Item body, Item barrel, int killCount)
    {
        Body = body.displayName;
        Barrel = barrel.displayName;
        Extension = "";
        KillCount = killCount;
    }
    public CombinationWins(Item body, Item barrel, Item extension, int killCount) 
    {
        Body = body.displayName;
        Barrel = barrel.displayName;
        Extension = extension.displayName;
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
public class WinData
{
    public List<CombinationWins> Data = new List<CombinationWins>();
}

public class PersistentInfo : MonoBehaviour
{
    public static PersistentInfo Singleton { get; private set; }
    [SerializeField]
    private EditorCombinationWins[] DefaultCombinationStats;
    public static List<CombinationWins> CombinationStats;
    void Start()
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
        CreateDefaultFile();
        if (File.Exists(Application.persistentDataPath + "/PersistentInfo.dat") == true)
        {
            LoadPersistentFile();
        }
        else
        {
            CreateDefaultFile();
        }
    }

    private void LoadPersistentFile()
    {
        if (File.Exists(Application.persistentDataPath + "/PersistentInfo.dat"))
        {
            BinaryFormatter persistentDataFormatter = new BinaryFormatter();
            FileStream persistentDataStream = File.Open(Application.persistentDataPath + "/PersistentInfo.dat", FileMode.Open);
            List<CombinationWins> weaponData = new List<CombinationWins>();
            try
            {
                weaponData = ((WinData) persistentDataFormatter.Deserialize(persistentDataStream)).Data;
            }
            catch
            {
                // File empty or corrupt, reseting to default
                // TODO: Give users feedback that their persistent data file is corrupt!
                persistentDataStream.Close();
                CreateDefaultFile();
                persistentDataStream = File.Open(Application.persistentDataPath + "/PersistentInfo.dat", FileMode.Open);
                weaponData = ((WinData) persistentDataFormatter.Deserialize(persistentDataStream)).Data;
            }
            
            persistentDataStream.Close();
            CombinationStats = weaponData.OrderByDescending(data => data.KillCount).ToList();
        }
    }

    private void CreateDefaultFile()
    {
        BinaryFormatter persistentDataFormatter = new BinaryFormatter();
        FileStream persistentDataStream = File.Create(Application.persistentDataPath + "/PersistentInfo.dat");
        var dataContainer = new WinData();
        DefaultCombinationStats.ToList()
            .Select(entry => new CombinationWins(entry.Body, entry.Barrel, entry.Extension, entry.KillCount))
            .ToList()
            .ForEach(entry => dataContainer.Data.Add(entry));
        persistentDataFormatter.Serialize(persistentDataStream, dataContainer);
        persistentDataStream.Close();
    }
}
