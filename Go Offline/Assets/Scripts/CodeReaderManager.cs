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

public class CodeReaderManager : MonoBehaviour {

    [Header("Output")]
    [SerializeField] public string lastResult = "";

    [Header("Needed")]
    [SerializeField] private ARSession session;
    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private ARCameraManager cameraManager;
    private bool scanningEnabled = false;
    private Texture2D cameraImageTexture;
    private Result result;
    private bool firstImage = true;

    private IBarcodeReader barcodeReader = new BarcodeReader {
        AutoRotate = true,
        Options = new ZXing.Common.DecodingOptions {
            TryHarder = false,
            AllowedLengths = new int[1] { 8 },
            PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.CODE_128 },
        }
    };

    [Header("Settings")]
    [SerializeField] private int downSampleFactor;
    [SerializeField] [Range(0, 100)] private float xCutoffPercentage;
    [SerializeField] [Range(0, 100)] private float yCutoffPercentage;
    private RectInt pictureFocusRect = new RectInt(0, 0, 100, 100);

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugResultText;
    [Space(10)]
    [SerializeField] private Image debugScanArea;
    [Space(10)]
    [SerializeField] private float debugImageSize;
    [SerializeField] private Image debugImage;

    private void Awake()
    {
        firstImage = true;
    }

    private void OnEnable() {
        //Subscribe to frameRecieved event
        cameraManager.frameReceived += OnCameraFrameReceived;

        debugResultText.text = "No Barcode Found";
    }

    private void OnDisable() {
        //Unsubscribe to frameRecieved event
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs) {
        //Checks if we should even be scanning
        if (!scanningEnabled) {
            return;
        }

        //Checks if we can access the latest Camera image
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) {
            Debug.LogWarning("Can't access the camera image output");
            return;
        }

        //First time setup
        if (firstImage)
        {
            //Image size = 640 x 480

            xCutoffPercentage *= 0.01f;
            yCutoffPercentage *= 0.01f;

            //Calc screenspace overlay for what part of the image gets scanned
            debugScanArea.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(Screen.width * xCutoffPercentage), Mathf.CeilToInt(Screen.height * yCutoffPercentage));

            float leftAnchor = (image.width / 2f) - (Mathf.CeilToInt(image.width * yCutoffPercentage) / 2f);
            float botAnchor = (image.height / 2f) - ((Mathf.CeilToInt((image.width * (Screen.width / Screen.height)) * xCutoffPercentage)) / 2f);
            float imageWidth = image.width * yCutoffPercentage;
            float imageHeight = (image.width * (Screen.width / Screen.height)) * xCutoffPercentage;

            Debug.Log($"Left Anchor = {leftAnchor}");
            Debug.Log($"Bottom Anchor = {botAnchor}");
            Debug.Log($"X Size = {imageWidth}");
            Debug.Log($"Y Size = {imageHeight}");

            Debug.Log($"Input Image size = {image.width} x {image.height}");

            /*
            //Calc focus area to be scanned in the recieved image
            pictureFocusRect = new RectInt(
                (image.height / 2) - (Mathf.CeilToInt(image.height * yCutoffPercentage) / 2),
                (image.width / 2) - (Mathf.CeilToInt((image.height * (Screen.width / Screen.height)) * xCutoffPercentage) / 2),
                Mathf.CeilToInt(image.height * yCutoffPercentage),
                Mathf.CeilToInt((image.height * (Screen.width / Screen.height)) * xCutoffPercentage)
                );
            */

            pictureFocusRect = new RectInt(
                Mathf.FloorToInt(leftAnchor),
                Mathf.FloorToInt(botAnchor),
                Mathf.CeilToInt(imageWidth),
                Mathf.CeilToInt(imageHeight)
                );

            firstImage = false;
        }

        var conversionParams = new XRCpuImage.ConversionParams {
            // Get the center of the image based on the Focus Rect
            inputRect = pictureFocusRect,

            // Downsample by downSampleFactor.
            outputDimensions = new Vector2Int(pictureFocusRect.width / downSampleFactor, pictureFocusRect.height / downSampleFactor),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        // See how many bytes you need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image.
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, buffer);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
        image.Dispose();

        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.

        // You've got the data; let's put it into a texture so you can visualize it.
        cameraImageTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        cameraImageTexture.LoadRawTextureData(buffer);
        cameraImageTexture.Apply();

        // Done with your temporary data, so you can dispose it.
        buffer.Dispose();

        // Debug image size calc
        debugImage.rectTransform.sizeDelta = new Vector2(cameraImageTexture.width * debugImageSize, cameraImageTexture.height * debugImageSize);

        //Send to debug image visualiser
        Sprite debugSprite = Sprite.Create(cameraImageTexture, new Rect(0, 0, cameraImageTexture.width, cameraImageTexture.height), new Vector2(0.5f, 0.5f));
        debugImage.sprite = debugSprite;

        // Detect and decode the barcode inside the bitmap
        result = barcodeReader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);

        // Do something with the result
        if (result != null) {
            lastResult = $"{result.Text}, {result.BarcodeFormat}";
            debugResultText.text = lastResult;
        }
    }

    public void ToggleScanning() {
        scanningEnabled = !scanningEnabled;
    }

    public string GetCurrentState() {
        return "Is Scanner running? - " + scanningEnabled;
    }
}
