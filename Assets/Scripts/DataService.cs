public interface DataService
    {
        bool SaveData<T>(string relativePath, T data, bool encrypted);
        
        T LoadData<T>(string relativePath, T data, bool encrypted);
    }
