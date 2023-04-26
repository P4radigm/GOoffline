using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    private SettingsManager settingsManager;
    private CollectibleGenerator collectibleGenerator;
    [SerializeField] private GameObject deleteSaveFilesParent;
    [SerializeField] private GameObject deleteSaveFilesForSureParent;
    [SerializeField] private TextMeshProUGUI precisionText;
    [SerializeField] private TextMeshProUGUI permissionText;


    public void Init()
    {
        settingsManager = SettingsManager.instance;
        collectibleGenerator = CollectibleGenerator.instance;
        deleteSaveFilesParent.SetActive(true);
        deleteSaveFilesForSureParent.SetActive(false);
        precisionText.text = settingsManager.settings.scannerDownscaleFactor == settingsManager.highScannerFactor ? "HIGH" : "LOW";

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        { permissionText.text = "GO: Offline already has all the permissions it needs"; }
        else
        { permissionText.text = "GO: Offline needs permission to use your camera for barcode scanning"; }

    }

    public void PressRetryPermissions()
    {
        Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        { permissionText.text = "GO: Offline already has all the permissions it needs"; }
        else
        { permissionText.text = "GO: Offline needs permission to use your camera for barcode scanning"; }
    }

    public void PressBarcodePrecision()
    {
        if(settingsManager.settings.scannerDownscaleFactor == settingsManager.highScannerFactor)
        {
            precisionText.text = "LOW";
            settingsManager.ChangeScannerResolution(settingsManager.lowScannerFactor);
        }
        else
        {
            precisionText.text = "HIGH";
            settingsManager.ChangeScannerResolution(settingsManager.highScannerFactor);
        }
    }

    public void PressDeleteSave()
    {
        deleteSaveFilesParent.SetActive(false);
        deleteSaveFilesForSureParent.SetActive(true);
    }

    public void RejectDeleteSave()
    {
        deleteSaveFilesParent.SetActive(true);
        deleteSaveFilesForSureParent.SetActive(false);
    }

    public void AcceptDeleteSave()
    {
        deleteSaveFilesParent.SetActive(true);
        deleteSaveFilesForSureParent.SetActive(false);

        collectibleGenerator.DeleteAllSaveData();
    }
}
