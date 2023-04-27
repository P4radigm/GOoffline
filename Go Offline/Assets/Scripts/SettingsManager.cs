using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation.Samples;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance = null;
    public UserSettings settings;
    [SerializeField] private string settingsFileName;
    [SerializeField] private UserSettings standardSetting;
    public int highScannerFactor;
    public int lowScannerFactor;
    private string filePath;
    [SerializeField] private ARCameraManager m_CameraManager;
    [SerializeField] private CameraConfigController configController;

    public ARCameraManager cameraManager
    {
        get => m_CameraManager;
        set => m_CameraManager = value;
    }

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
        //configController.SetupInitialValue(settings.cameraConfig);
        Application.targetFrameRate = 60;
        if (cameraManager.currentConfiguration != null && cameraManager.currentConfiguration.HasValue)
        {
            Application.targetFrameRate = (int)cameraManager.currentConfiguration.Value.framerate;
        }
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

    public void ChangeCameraConfig(int newIndex)
    {
        //settings.cameraConfig = newIndex;
        if (cameraManager.currentConfiguration != null || cameraManager.currentConfiguration.Value.framerate != null)
        {
            Application.targetFrameRate = (int)cameraManager.currentConfiguration.Value.framerate;
        }

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
