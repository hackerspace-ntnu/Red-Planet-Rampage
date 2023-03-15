using System;
using System.Collections;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using File = System.IO.File;

public class JsonDataService : DataService
{
    public bool SaveData<T>(string relativePath, T data, bool encrypted)
    {
        var path = Application.persistentDataPath + relativePath;
        if (File.Exists(path))
        {
            Debug.Log("Data exists creating a new one");
            File.Delete(path);
        }

        try
        {
            using FileStream stream = File.Create(path);
            stream.Close();
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Could save because: {e.Message} {e.StackTrace}");
            return false;
        }
        
        
    }

    public T LoadData<T>(string relativePath, T Data, bool encrypted)
    {
        throw new System.NotImplementedException();
    } 
}
