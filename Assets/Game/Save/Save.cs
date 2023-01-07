using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Unity.VisualScripting;
using System;
using System.ComponentModel;

public class Save
{
    private readonly static Save instance = new();
    public static Save Instance => instance;


    private readonly string savePath = Application.persistentDataPath + "/save.json";
    private readonly List<ISaver> savers = new();
    private Data saveData;

    private bool hasFirstLoaded = false;

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

        [JsonProperty("PlayerEXP")]
        public int PlayerExperiencePoints { get; set; }

        [JsonProperty("PlayerSkillPoints")]
        public int PlayerSkillPoints { get; set; }

        [JsonProperty("PlayerSkillSet")]
        public SkillSet PlayerSkillSet { get; set; }


        [JsonProperty("VisitedPOIs")]
        public Dictionary<string, Dictionary<string, int>> VisitedPOIs { get; set; }

        [JsonProperty("VisitedPOIDate")]
        public DateTime VisitedPOIDate;

        [JsonProperty("Inventory")]
        public Dictionary<string, int> Inventory { get; set; }


    }

    private Save()
    {
    }

    public void Init()
    {
        savers.Clear();
        hasFirstLoaded = false;
    }

    public void InitSaver(ISaver saver)
    {
        savers.Add(saver);
    }

    public void SaveRequest()
    {
        if (!hasFirstLoaded)
        {
            return;
        }
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
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        saveData ??= new();

        for (int i = 0; i < savers.Count; i++)
        {
            savers[i].LoadData(saveData);
        }
        hasFirstLoaded = true;
    }


}
