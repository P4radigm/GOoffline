using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using Unity.XR.CoreUtils;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using Unity.Collections.LowLevel.Unsafe;
using System;

public class CodeReaderManager : MonoBehaviour {

    [Header("Output")]
    [SerializeField] public string lastResult = "";
    [SerializeField] public string lastResultText = "";

    [Header("Needed")]
    [SerializeField] private ARSession session;
    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectTransform scanArea;

    private Texture2D cameraImageTextureR8;
    private Texture2D cameraImageTextureRGBA32;
    private TextureFormat textureFormat;
    private Result result;

    private bool firstImage = true;

    private CollectibleGenerator collectibleGenerator;
    private SettingsManager settingsManager;

    private IBarcodeReader barcodeReader = new BarcodeReader {
        AutoRotate = true,
        Options = new ZXing.Common.DecodingOptions {
            TryHarder = false,
            AllowedLengths = new int[1] { 8 },
            PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.CODE_128 },
        }
    };

    [Header("Settings")]
    [SerializeField] private bool squareBasedOnXPercentage = false;
    [Space(5)]
    [SerializeField][Range(0, 100)] private float xCutoffPercentage;
    [SerializeField][Range(0, 100)] private float yCutoffPercentage;
    [Space(5)]
    [SerializeField][Range(0, 100)] private float xBufferPercentage;
    [SerializeField][Range(0, 100)] private float yBufferPercentage;
    private RectInt pictureFocusRect = new RectInt(0, 0, 100, 100);
    [Space(20)]
    [SerializeField] private Rectangle[] scannerRectangles;
    [SerializeField] private Color[] progressColors;
    [SerializeField] private float scanLength;
    private float scanTimer;
    private List<Result> tempListOfResults = new();
    [Space(10)]
    [SerializeField] private float scannerCooldown;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private Color activeColor;
    private float cooldownTimer;

    public enum scannerState
    {
        searching,
        scanning,
        cooldown
    }
    [Header("State")]
    public scannerState currentState;
    public bool scanningEnabled;

    public static CodeReaderManager instance = null;

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

    }

    private void Start()
    {
        collectibleGenerator = CollectibleGenerator.instance;
        settingsManager = SettingsManager.instance;

        firstImage = true;
        textureFormat = TextureFormat.R8;
    }

    private void OnEnable() {
        //Subscribe to frameRecieved event
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable() {
        //Unsubscribe to frameRecieved event
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs) {
        HandleCameraImageToResult();
    }

    unsafe void HandleCameraImageToResult()
    {
        //Checks if we should even be scanning
        if (!scanningEnabled || currentState == scannerState.cooldown)
        {
            return;
        }

        //Checks if we can access the latest Camera image
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            Debug.LogWarning("Can't access the camera image output");
            return;
        }

        //First time setup only calculates the focusRect based on the first recieved CPUimage
        if (firstImage)
        {
            //Image size = 640 x 480

            //Calc actual cutoff used
            float xCutoff = xCutoffPercentage * 0.01f;
            float yCutoff = yCutoffPercentage * 0.01f;
            float xBuffer = xBufferPercentage * 0.01f;
            float yBuffer = yBufferPercentage * 0.01f;

            //Get Canvas Scaler max coords
            float maxCoordX = canvasScaler.GetComponent<RectTransform>().sizeDelta.x;
            float maxCoordY = canvasScaler.GetComponent<RectTransform>().sizeDelta.y;

            //Calc screenspace overlay for what part of the image gets scanned
            scanArea.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordX * Mathf.Clamp(xCutoff - xBuffer, 0, 100)), Mathf.CeilToInt(maxCoordY * Mathf.Clamp(yCutoff - yBuffer, 0, 100)));

            float leftAnchor = ((float)image.width / 2f) - ((float)image.width * yCutoff / 2f);
            float botAnchor = ((float)image.height / 2f) - (((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff / 2f);
            float imageWidth = (float)image.width * yCutoff;
            float imageHeight = ((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff;

            //Adjust for square option
            if (squareBasedOnXPercentage)
            {
                scanArea.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordX * Mathf.Clamp(xCutoff - xBuffer, 0, 100)), Mathf.CeilToInt(maxCoordX * Mathf.Clamp(xCutoff - xBuffer, 0, 100)));

                leftAnchor = ((float)image.width / 2f) - (((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff / 2f);
                imageWidth = ((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff;
            }

            pictureFocusRect = new RectInt(
                Mathf.FloorToInt(leftAnchor),
                Mathf.FloorToInt(botAnchor),
                Mathf.CeilToInt(imageWidth),
                Mathf.CeilToInt(imageHeight)
                );

            firstImage = false;
        }

        #region getting image

        //Set converion parameters
        var conversionParams = new XRCpuImage.ConversionParams
        {
            // Get the center of the image based on the Focus Rect
            inputRect = pictureFocusRect,

            // Downsample by downSampleFactor.
            outputDimensions = new Vector2Int(pictureFocusRect.width / settingsManager.settings.scannerDownscaleFactor, pictureFocusRect.height / settingsManager.settings.scannerDownscaleFactor),

            // Choose Grayscale format.
            outputFormat = textureFormat,

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        //Set texture to match the conversion parameters if that hasn't been set up already
        if (cameraImageTextureR8 == null || cameraImageTextureR8.width != conversionParams.outputDimensions.x || cameraImageTextureR8.height != conversionParams.outputDimensions.y)
        {
            cameraImageTextureR8 = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
                false);
        }

        if (cameraImageTextureRGBA32 == null || cameraImageTextureRGBA32.width != conversionParams.outputDimensions.x || cameraImageTextureRGBA32.height != conversionParams.outputDimensions.y)
        {
            cameraImageTextureRGBA32 = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                TextureFormat.RGBA32,
                false);
        }

        // Texture2D allows us write directly to the raw texture data
        // This allows us to do the conversion in-place without making any copies.
        var rawTextureData = cameraImageTextureR8.GetRawTextureData<byte>();
        try
        {
            image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
        }
        finally
        {
            // We must dispose of the XRCpuImage after we're finished
            // with it to avoid leaking native resources.
            image.Dispose();
        }

        // Apply the updated texture data to our texture
        cameraImageTextureR8.Apply();

        //Convert r8 format to rgba32 format for ZXing barcodeReader
        byte[] pixelsR8 = cameraImageTextureR8.GetRawTextureData();
        Color32[] pixelsRGBA32 = new Color32[pixelsR8.Length];
        for (int i = pixelsR8.Length - 1; i != -1; i--)
        {
            byte value = pixelsR8[i];
            pixelsRGBA32[i] = new Color32(value, value, value, 255);//simplest R8 to RGBA32 conversion
        }
        cameraImageTextureRGBA32.SetPixels32(pixelsRGBA32);//updates textureRGBA32 data in CPU memory
        cameraImageTextureRGBA32.Apply();//sends textureRGBA32 data from CPU memory to GPU (without this rendering wont change a bit)

        #endregion

        // Detect and decode the barcode inside the bitmap
        result = barcodeReader.Decode(cameraImageTextureRGBA32.GetPixels32(), cameraImageTextureRGBA32.width, cameraImageTextureRGBA32.height);

        //Check if we even have a result
        if (result == null)
        {
            return;
        }

        //First barcode found -> Initialise scanning process
        if (currentState == scannerState.searching)
        {
            tempListOfResults.Clear();
            if (result.Text.Length == 8) { tempListOfResults.Add(result); }


            scanTimer = scanLength;
            currentState = scannerState.scanning;
        }
        //In scanning process
        else if (currentState == scannerState.scanning)
        {
            if (result.Text.Length == 8) { tempListOfResults.Add(result); }
        }
    }

    private void Update()
    {
        if(currentState == scannerState.scanning && scanTimer > 0)
        {
            scanTimer -= Time.deltaTime;
            //Recolor certain scannerRects based on progress
            for (int i = 0; i < scannerRectangles.Length; i++)
            {
                if(scanTimer <= scanLength - (i * (scanLength / (float)scannerRectangles.Length)))
                {
                    RecolorScannerRectangle(new int[] { i + 1 }, progressColors[i]);
                }
                else
                {
                    RecolorScannerRectangle(new int[] { i + 1 }, activeColor);
                }
            }
        }
        else if(currentState == scannerState.scanning)
        {
            GotResult(GetMostCommonResult(tempListOfResults));
            RecolorScannerRectangle(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, inactiveColor);

            cooldownTimer = scannerCooldown;
            currentState = scannerState.cooldown;
        }

        if(currentState == scannerState.cooldown && cooldownTimer > 0)
        { 
            cooldownTimer -= Time.deltaTime; 

            if(cooldownTimer <= 0)
            {
                RecolorScannerRectangle(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, activeColor);

                currentState = scannerState.searching;
            }
        }
    }

    private void GotResult(Result newResult)
    {
        if (newResult.Text == lastResultText)
        {
            //Pop-up that you should go scan something new
            Debug.Log($"This {newResult} is the same barcode as last time...");
            collectibleGenerator.alreadyScannedTodayPopUp.SetActive(true);
            string notificationText = $"You have scanned this bike already, now let's go find a new one!";
            collectibleGenerator.alreadyScannedTodayPopUp.GetComponent<AlreadyScannedPopUp>().StartPopUp(notificationText, collectibleGenerator.scannedTodayPopUpTime);
            return;
        }

        //Output to debug Text
        //lastResult = $"{newResult.Text}, {newResult.BarcodeFormat}";
        //debugResultText.text = lastResult;

        //Send to New Collectible Generator (Need ot implement minigame step before that at some point)
        collectibleGenerator.ScannedBarcode(newResult.Text, UnityEngine.Random.Range(0, 5));

        lastResultText = newResult.Text;
    }

    private void RecolorScannerRectangle(int[] rectsToRecolor, Color newColor)
    {

        for (int i = 0; i < scannerRectangles.Length; i++)
        {
            for (int j = 0; j < rectsToRecolor.Length; j++)
            {
                if(i == (rectsToRecolor[j] - 1))
                {
                    scannerRectangles[i].Color = newColor;
                }
            }
        }
    }

    public void SetScanningEnabled(bool isEnabled) {
        scanningEnabled = isEnabled;
        currentState = scannerState.searching;
        scanTimer = 0;
        cooldownTimer = 0;
    }

    public string DebugShowCurrentState() {
        return $"Is Scanner running? - {currentState}";
    }

    private Result GetMostCommonResult(List<Result> results)
    {
        if (results == null || results.Count == 0)
        {
            return null;
        }

        Dictionary<Result, int> resultCounts = new Dictionary<Result, int>();

        // Count the number of occurrences of each object
        foreach (Result res in results)
        {
            if (resultCounts.ContainsKey(res))
            {
                resultCounts[res]++;
            }
            else
            {
                resultCounts[res] = 1;
            }
        }

        // Find the object with the highest count
        Result mostCommonResult = null;
        int highestCount = 0;

        foreach (KeyValuePair<Result, int> pair in resultCounts)
        {
            if (pair.Value > highestCount)
            {
                mostCommonResult = pair.Key;
                highestCount = pair.Value;
            }
        }

        return mostCommonResult;
    }
}
