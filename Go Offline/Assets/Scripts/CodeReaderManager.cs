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
    [SerializeField] private CanvasScaler canvasScaler;
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
    [Space(10)]
    [SerializeField] private bool squareBasedOnXPercentage = false;
    [Space(5)]
    [SerializeField] [Range(0, 100)] private float xCutoffPercentage;
    [SerializeField] [Range(0, 100)] private float yCutoffPercentage;
    [Space(5)]
    [SerializeField] [Range(0, 100)] private float xBufferPercentage;
    [SerializeField] [Range(0, 100)] private float yBufferPercentage;
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

            //Calc actual cutoff used
            float xCutoff = xCutoffPercentage * 0.01f;
            float yCutoff = yCutoffPercentage * 0.01f;
            float xBuffer = xBufferPercentage * 0.01f;
            float yBuffer = yBufferPercentage * 0.01f;

            //Get Canvas Scaler max coords based on reference resolution (not dynamic when MatchWidthHeight gets changed from 1)
            float maxCoordX = canvasScaler.referenceResolution.y * ((float)Screen.width / (float)Screen.height);
            float maxCoordY = canvasScaler.referenceResolution.y;

            //Calc screenspace overlay for what part of the image gets scanned
            debugScanArea.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordX * Mathf.Clamp(xCutoff + xBuffer, 0, 100)), Mathf.CeilToInt(maxCoordY * Mathf.Clamp(yCutoff + yBuffer, 0, 100)));
            debugImage.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordY * yCutoff), Mathf.CeilToInt(maxCoordX * xCutoff));

            float leftAnchor = ((float)image.width / 2f) - ((float)image.width * yCutoff / 2f);
            float botAnchor = ((float)image.height / 2f) - (((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff / 2f);
            float imageWidth = (float)image.width * yCutoff;
            float imageHeight = ((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff;

            //Adjust for square option
            if (squareBasedOnXPercentage)
            {
                debugScanArea.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordX * Mathf.Clamp(xCutoff + xBuffer, 0, 100)), Mathf.CeilToInt(maxCoordX * Mathf.Clamp(xCutoff + xBuffer, 0, 100)));
                debugImage.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordX * xCutoff), Mathf.CeilToInt(maxCoordX * xCutoff));

                leftAnchor = ((float)image.width / 2f) - (((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff / 2f);
                imageWidth = ((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff;
            }


            /* //Debug
            Debug.Log($"Screen Resolution = {Screen.width} x {Screen.height}");

            Debug.Log($"Left Anchor = {leftAnchor}");
            Debug.Log($"Bottom Anchor = {botAnchor}");
            Debug.Log($"X Size = {imageWidth}");
            Debug.Log($"Y Size = {imageHeight}");

            Debug.Log($"Input Image size = {image.width} x {image.height}");

            leftAnchor = 0;
            botAnchor = 0;
            imageWidth = (float)image.width;
            imageHeight = (float)image.height;
            debugImage.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordY), Mathf.CeilToInt(maxCoordX));
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
