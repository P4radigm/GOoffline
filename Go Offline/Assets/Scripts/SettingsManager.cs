using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance = null;
    public UserSettings settings;
    [SerializeField] private string settingsFileName;
    [SerializeField] private UserSettings standardSetting;
    public int highScannerFactor;
    public int lowScannerFactor;
    private string filePath;

    private void Awake()
    {
        //Initiate Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        filePath = Path.Combine(Application.persistentDataPath, $"{settingsFileName}.json");

        //Read settings
        TryReadSettingsFile();
    }

    private void Start()
    {

    }

    private void TryReadSettingsFile()
    {
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            settings = JsonUtility.FromJson<UserSettings>(jsonData);
        }
        else
        {
            settings = standardSetting;
            string jsonString = JsonUtility.ToJson(standardSetting);
            File.WriteAllText(filePath, jsonString);
        }
    }

    public void ChangeSortingMethod(int newSortingMethod)
    {
        settings.sortingMode = newSortingMethod;

        string jsonString = JsonUtility.ToJson(settings);
        File.WriteAllText(filePath, jsonString);
    }

    public void ChangeScannerResolution(int newRes)
    {
        settings.scannerDownscaleFactor = newRes;

        string jsonString = JsonUtility.ToJson(settings);
        File.WriteAllText(filePath, jsonString);
    }

    public void ChangeFirstTimeStatus(bool newStatus)
    {
        settings.firstTime = newStatus;

        string jsonString = JsonUtility.ToJson(settings);
        File.WriteAllText(filePath, jsonString);
    }
}
