using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.XR;
public static class SaveSystem 
{
    public static void InitSave()
    {
        PaintObject[] paints = Resources.LoadAll<PaintObject>("Paints");

        SaveElement dataList = new();
        dataList.dataElements = new();

        foreach (PaintObject pa in paints)
        {
            bool[] paintData = new bool[pa.regionSprites.Count];
            Array.Fill(paintData, false);
            bool[] catData = new bool[pa.catsSprites.Length];
            Array.Fill(catData, false);
            DataElement data = new(pa.id, paintData, catData, false, 0f, 2, false);
            dataList.dataElements.Add(data);
        }
        dataList.GameVersion = Application.version;
        Debug.Log(dataList.dataElements.Count);
        SavePlayer(dataList);
    }
    public static void InitReplay()
    {
        PaintObject[] paints = Resources.LoadAll<PaintObject>("Paints");

        ReplaySaveElement replaySaveElement = new ReplaySaveElement();
        replaySaveElement.replayElements = new();

        foreach (PaintObject pa in paints)
        {
            Vector2[] poss = new Vector2[0];
            int[] itemID = new int[0];
            string paintID = pa.id;

            ReplayElement data = new(paintID, poss, itemID);

            replaySaveElement.replayElements.Add(data);
        }
        replaySaveElement.GameVersion = Application.version;


        SaveReplay(replaySaveElement);
    }

    public static void RestartSaveTogetherByID(string id)
    {
        PaintObject[] paints = Resources.LoadAll<PaintObject>("Paints");

        DataElement data = null;
        ReplayElement dataR = null;
        foreach (PaintObject pa in paints)
        {
            if(pa.id == id)
            {
                bool[] paintData = new bool[pa.regionSprites.Count];
                Array.Fill(paintData, false);
                bool[] catData = new bool[pa.catsSprites.Length];
                Array.Fill(catData, false);
                data = new(pa.id, paintData, catData, false, 0f, 2, false);

                Vector2[] poss = new Vector2[0];
                int[] itemID = new int[0];
                dataR = new(pa.id, poss,itemID);
                break;
            }
        }
        SaveDataElement(data);
        SaveReplayElement(dataR);
    }




    public static bool SaveExists()
    {
        string path = Application.persistentDataPath + "/player_save.json";
        return File.Exists(path);
    }

    public static (string,string) GetSaveVersion()
    {
        string path = Application.persistentDataPath + "/player_save.json";

        // Dosyayı oku
        string saveData = File.ReadAllText(path);

        // Hash'i ayır
        int hashIndex = saveData.LastIndexOf("hash:");
        string jsonData = saveData.Substring(0, hashIndex).Trim();
        string storedHash = saveData.Substring(hashIndex + 5).Trim();

        // SHA256 hash hesapla
        string computedHash = ComputeSHA256(jsonData);

        // Hash doğrulaması yap
        if (storedHash == computedHash)
        {
            // Hash eşleşti, veriyi deserialize et
            SaveElement data = JsonUtility.FromJson<SaveElement>(jsonData);
            return (data.GameVersion,null);
        }
        else
        {
            string msg = "Save file might be corrupted or tampered. File will be deleted";
            File.Delete(path);
            InitSave();
            return (null,msg);
        }
    }

    public static void SaveDataElement(DataElement data)
    {
        string path = Application.persistentDataPath + "/player_save.json";

        SaveElement saveElement;
        if(File.Exists(path))
        {
            // Dosyayı oku
            string saveData = File.ReadAllText(path);

            // Hash'i ayır
            int hashIndex = saveData.LastIndexOf("hash:");
            string jsonData = saveData.Substring(0, hashIndex).Trim();
            string storedHash = saveData.Substring(hashIndex + 5).Trim();

            // SHA256 hash hesapla
            string computedHash = ComputeSHA256(jsonData);

            // Hash doğrulaması yap
            if (storedHash == computedHash)
            {
                // Hash eşleşti, veriyi deserialize et
                saveElement = JsonUtility.FromJson<SaveElement>(jsonData);
            }
            else
            {
                return;
            }
        }
        else
        {
            return;
        }


        int c = 0;
        foreach (DataElement dataElem in saveElement.dataElements)
        {
            if (dataElem.paintID == data.paintID)
            {
                saveElement.dataElements[saveElement.dataElements.IndexOf(dataElem)] = data;
                break;
            }
            c++;
        }
        SavePlayer(saveElement);
    }

    public static void SavePlayer(SaveElement saveElement)
    {
        // JSON serileştirme
        string json = JsonUtility.ToJson(saveElement);

        // Hash hesaplama
        string hash = ComputeSHA256(json);

        // JSON ve Hash'i birleştir
        string saveDataJ = json + "\n" + "hash:" + hash;

        // Dosya yolu
        string path = Application.persistentDataPath + "/player_save.json";

        // Dosyaya yazma
        File.WriteAllText(path, saveDataJ);
    }

    public static (List<DataElement>,string) LoadAllData()
    {
        string path = Application.persistentDataPath + "/player_save.json";

        if (File.Exists(path))
        {
            // Dosyayı oku
            string saveData = File.ReadAllText(path);

            // Hash'i ayır
            int hashIndex = saveData.LastIndexOf("hash:");
            string jsonData = saveData.Substring(0, hashIndex).Trim();
            string storedHash = saveData.Substring(hashIndex + 5).Trim();

            // SHA256 hash hesapla
            string computedHash = ComputeSHA256(jsonData);

            // Hash doğrulaması yap
            if (storedHash == computedHash)
            {
                // Hash eşleşti, veriyi deserialize et
                SaveElement data = JsonUtility.FromJson<SaveElement>(jsonData);
                if (data.GameVersion != Application.version)
                {
                    string msg = "Game version is not matching with save version, Your save might corrupted";
                    return (data.dataElements, msg);
                }
                else
                {
                    return (data.dataElements, null);
                }
            }
            else
            {
                string msg = "Save file might be corrupted or tampered. File will be deleted";
                File.Delete(path);
                InitSave();
                return (null, msg);
            }
        }
        else
        {
            string msg = "Save file not found! Creating a new one...";
            return (null, msg);
        }

    }

    public static (DataElement,string) LoadData(string id)
    {
        string path = Application.persistentDataPath + "/player_save.json";

        if (File.Exists(path))
        {
            // Dosyayı oku
            string saveData = File.ReadAllText(path);

            // Hash'i ayır
            int hashIndex = saveData.LastIndexOf("hash:");
            string jsonData = saveData.Substring(0, hashIndex).Trim();
            string storedHash = saveData.Substring(hashIndex + 5).Trim();

            // SHA256 hash hesapla
            string computedHash = ComputeSHA256(jsonData);

            // Hash doğrulaması yap
            if (storedHash == computedHash)
            {
                // Hash eşleşti, veriyi deserialize et
                SaveElement data = JsonUtility.FromJson<SaveElement>(jsonData);
                Debug.Log(data.dataElements.Count);
                if (data.GameVersion != Application.version)
                {
                    string msg = "Game version is not matching with save version, Your save might corrupted";
                    return (GetDesiredData(data,id), msg);
                }
                else
                {
                    return (GetDesiredData(data, id), null);
                }
            }
            else
            {
                string msg = "Save file might be corrupted or tampered. File will be deleted";
                File.Delete(path);
                InitSave();
                return (null,msg);
            }
        }
        else
        {
            string msg = "Save file not found! Creating a new one...";
            return (null,msg);
        }
    }

    public static DataElement GetDesiredData(SaveElement saveElement,string id)
    {
        foreach(DataElement data in saveElement.dataElements)
        {
            if(data.paintID == id) return data;
        }
        return null;
    }


    /// <summary>
    ///  Oh No Big Imorr
    /// </summary>
    /// <returns></returns>


    public static bool SaveReplayExits()
    {
        string path = Application.persistentDataPath + "/player_replays.json";
        return File.Exists(path);
    }

    public static (string, string) GetSaveReplayVersion()
    {
        string path = Application.persistentDataPath + "/player_replays.json";

        // Dosyayı oku
        string saveData = File.ReadAllText(path);

        // Hash'i ayır
        int hashIndex = saveData.LastIndexOf("hash:");
        string jsonData = saveData.Substring(0, hashIndex).Trim();
        string storedHash = saveData.Substring(hashIndex + 5).Trim();

        // SHA256 hash hesapla
        string computedHash = ComputeSHA256(jsonData);

        // Hash doğrulaması yap
        if (storedHash == computedHash)
        {
            // Hash eşleşti, veriyi deserialize et
            ReplaySaveElement data = JsonUtility.FromJson<ReplaySaveElement>(jsonData);
            return (data.GameVersion, null);
        }
        else
        {
            string msg = "Save file might be corrupted or tampered. File will be deleted";
            File.Delete(path);
            InitSave();
            return (null, msg);
        }
    }

    public static void SaveReplayElement(ReplayElement data)
    {
        string path = Application.persistentDataPath + "/player_replays.json";

        ReplaySaveElement replaySaveElement;
        if (File.Exists(path))
        {

            // Dosyayı oku
            string saveData = File.ReadAllText(path);

            // Hash'i ayır
            int hashIndex = saveData.LastIndexOf("hash:");
            string jsonData = saveData.Substring(0, hashIndex).Trim();
            string storedHash = saveData.Substring(hashIndex + 5).Trim();

            // SHA256 hash hesapla
            string computedHash = ComputeSHA256(jsonData);

            // Hash doğrulaması yap
            if (storedHash == computedHash)
            {
                // Hash eşleşti, veriyi deserialize et
                replaySaveElement = JsonUtility.FromJson<ReplaySaveElement>(jsonData);
            }
            else
            {
                return;
            }
        }
        else
        {
            return;
        }

        int c = 0;
        foreach (ReplayElement dataElem in replaySaveElement.replayElements)
        {
            if (dataElem.paintID == data.paintID)
            {
                replaySaveElement.replayElements[replaySaveElement.replayElements.IndexOf(dataElem)] = data;
                break;
            }
            c++;
        }

        SaveReplay(replaySaveElement);
    }

    public static void SaveReplay(ReplaySaveElement data)
    {
        // JSON serileştirme
        string json = JsonUtility.ToJson(data);

        // Hash hesaplama
        string hash = ComputeSHA256(json);

        // JSON ve Hash'i birleştir
        string saveDataJ = json + "\n" + "hash:" + hash;

        // Dosya yolu
        string path = Application.persistentDataPath + "/player_replays.json";

        // Dosyaya yazma
        File.WriteAllText(path, saveDataJ);

    }

    public static (List<ReplayElement>, string) LoadAllReplays()
    {
        string path = Application.persistentDataPath + "/player_replays.json";

        if (File.Exists(path))
        {
            // Dosyayı oku
            string saveData = File.ReadAllText(path);

            // Hash'i ayır
            int hashIndex = saveData.LastIndexOf("hash:");
            string jsonData = saveData.Substring(0, hashIndex).Trim();
            string storedHash = saveData.Substring(hashIndex + 5).Trim();

            // SHA256 hash hesapla
            string computedHash = ComputeSHA256(jsonData);

            // Hash doğrulaması yap
            if (storedHash == computedHash)
            {
                // Hash eşleşti, veriyi deserialize et
                ReplaySaveElement data = JsonUtility.FromJson<ReplaySaveElement>(jsonData);
                if (data.GameVersion != Application.version)
                {
                    string msg = "Game version is not matching with save version, Your save might corrupted";
                    return (data.replayElements, msg);
                }
                else
                {
                    return (data.replayElements, null);
                }
            }
            else
            {
                string msg = "Save file might be corrupted or tampered. File will be deleted";
                File.Delete(path);
                InitSave();
                return (null, msg);
            }
        }
        else
        {
            string msg = "Save file not found! Creating a new one...";
            return (null, msg);
        }

    }

    public static (ReplayElement, string) LoadReplay(string id)
    {
        string path = Application.persistentDataPath + "/player_replays.json";

        if (File.Exists(path))
        {
            // Dosyayı oku
            string saveData = File.ReadAllText(path);

            // Hash'i ayır
            int hashIndex = saveData.LastIndexOf("hash:");
            string jsonData = saveData.Substring(0, hashIndex).Trim();
            string storedHash = saveData.Substring(hashIndex + 5).Trim();

            // SHA256 hash hesapla
            string computedHash = ComputeSHA256(jsonData);

            // Hash doğrulaması yap
            if (storedHash == computedHash)
            {
                // Hash eşleşti, veriyi deserialize et
                ReplaySaveElement data = JsonUtility.FromJson<ReplaySaveElement>(jsonData);
                if (data.GameVersion != Application.version)
                {
                    string msg = "Game version is not matching with save version, Your save might corrupted";
                    return (GetDesiredReplay(data, id), msg);
                }
                else
                {
                    return (GetDesiredReplay(data, id), null);
                }
            }
            else
            {
                string msg = "Save file might be corrupted or tampered. File will be deleted";
                File.Delete(path);
                InitSave();
                return (null, msg);
            }
        }
        else
        {
            string msg = "Save file not found! Creating a new one...";
            return (null, msg);
        }
    }

    public static ReplayElement GetDesiredReplay(ReplaySaveElement replaySaveElement, string id)
    {
        foreach (ReplayElement data in replaySaveElement.replayElements)
        {
            if (data.paintID == id) return data;
        }
        return null;
    }


    /// <summary>
    /// Ohno Veri Big Imorr
    /// </summary>


    public static void WipeData()
    {
        string path = Application.persistentDataPath + "/player_save.json";
        string path1 = Application.persistentDataPath + "/player_replays.json";


        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (File.Exists(path1))
        {
            File.Delete(path1);
        }

        InitSave();
        InitReplay();
    }

    private static string ComputeSHA256(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }



}

[System.Serializable]
public class SaveElement
{
    public string GameVersion;
    public List<DataElement> dataElements;
}
[System.Serializable]
public class ReplaySaveElement
{
    public string GameVersion;
    public List<ReplayElement> replayElements;
}
