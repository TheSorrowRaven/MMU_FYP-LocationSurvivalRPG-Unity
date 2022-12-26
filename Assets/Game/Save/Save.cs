using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class Save
{
    private readonly static Save instance = new();
    public static Save Instance => instance;


    private readonly string savePath = Application.persistentDataPath + "/save.json";
    private readonly List<ISaver> savers = new();
    private Data saveData;

    public interface ISaver
    {
        void StartInit();
        void SaveData(Data data);
        void LoadData(Data data);
    }


    [System.Serializable]
    public class Data
    {
        [JsonProperty("Namez")]
        public string Name { get; set; }

        [JsonProperty("PlayerStats")]
        public int[] PlayerStats { get; set; }

        [JsonProperty("Inventory")]
        public Dictionary<string, int> Inventory { get; set; }

    }

    private Save()
    {
    }

    public void Init()
    {
        savers.Clear();
    }

    public void InitSaver(ISaver saver)
    {
        savers.Add(saver);
    }

    public void SaveRequest()
    {
        //TODO queue based on number of requests, don't save every time there is a request
        SaveToFile();
    }

    private void SaveToFile()
    {
        for (int i = 0; i < savers.Count; i++)
        {
            savers[i].SaveData(saveData);
        }

        string text = JsonConvert.SerializeObject(saveData);
        File.WriteAllText(savePath, text);
    }

    public void LoadFromFile()
    {
        try
        {
            string text = File.ReadAllText(savePath);
            saveData = JsonConvert.DeserializeObject<Data>(text);
        }
        catch (System.Exception)
        {

        }
        saveData ??= new();

        for (int i = 0; i < savers.Count; i++)
        {
            savers[i].LoadData(saveData);
        }
    }


}
