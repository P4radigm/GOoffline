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
    [SerializeField] public string lastResultText = "";

    [Header("Needed")]
    [SerializeField] private ARSession session;
    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private CanvasScaler canvasScaler;
    private bool scanningEnabled = false;
    private Texture2D cameraImageTexture;
    private Result result;
    private bool firstImage = true;
    private CollectibleGenerator collectibleGenerator;
    private CollectibleManager collectibleManager;
    private Color swapfietsColor;

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
    [SerializeField] private Image debugImage;

    private void Awake()
    {
        firstImage = true;
        collectibleGenerator = CollectibleGenerator.instance;
        collectibleManager = CollectibleManager.instance;
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
            debugScanArea.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordX * Mathf.Clamp(xCutoff - xBuffer, 0, 100)), Mathf.CeilToInt(maxCoordY * Mathf.Clamp(yCutoff - yBuffer, 0, 100)));
            debugImage.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordY * yCutoff), Mathf.CeilToInt(maxCoordX * xCutoff));

            float leftAnchor = ((float)image.width / 2f) - ((float)image.width * yCutoff / 2f);
            float botAnchor = ((float)image.height / 2f) - (((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff / 2f);
            float imageWidth = (float)image.width * yCutoff;
            float imageHeight = ((float)image.width * ((float)Screen.width / (float)Screen.height)) * xCutoff;

            //Adjust for square option
            if (squareBasedOnXPercentage)
            {
                debugScanArea.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordX * Mathf.Clamp(xCutoff - xBuffer, 0, 100)), Mathf.CeilToInt(maxCoordX * Mathf.Clamp(xCutoff - xBuffer, 0, 100)));
                debugImage.rectTransform.sizeDelta = new Vector2Int(Mathf.CeilToInt(maxCoordX * xCutoff), Mathf.CeilToInt(maxCoordX * xCutoff));

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

        // Detect and decode the barcode inside the bitmap
        result = barcodeReader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);

        //!!!!Take reading over a period of a couple of seconds and see which one shows up the most to get an accurate reading.

        // Do something with the result
        if (result != null) {
            if (lastResultText == result.Text)
            {
                //Maybe after idk 1 minute after og scan time give pop-up that you should scan something new
                return;
            }

            // Detect the color of the swapfiets?
            //WriteAverageColorOnLine(cameraImageTexture, result);
            //Debug.Log("Average color: " + swapfietsColor);

            // Compare the average color against the predefined colors
            //Color closestColor = CompareColors(swapfietsColor, collectibleManager.swapfietsColors);
            //Debug.Log("Closest color: " + closestColor);

            //Send to debug image visualiser
            Sprite debugSprite = Sprite.Create(cameraImageTexture, new Rect(0, 0, cameraImageTexture.width, cameraImageTexture.height), new Vector2(0.5f, 0.5f));
            debugImage.sprite = debugSprite;

            //Output to debug Text
            lastResult = $"{result.Text}, {result.BarcodeFormat}";
            debugResultText.text = lastResult;

            //Send to New Collectible Generator (Need ot implemint minigame step before that at some point)
            collectibleGenerator.FinishedMinigameCollectible(result.Text, Random.Range(0, 5));

            lastResultText = result.Text;
        }
    }

    //private void WriteAverageColorOnLine(Texture2D texture, Result result)
    //{
    //    // Check barcode result
    //    if (result == null)
    //    {
    //        return;
    //    }

    //    // Get barcode location and size
    //    var location = result.ResultPoints;
    //    var x1 = location[0].X;
    //    var y1 = texture.height - location[0].Y;
    //    var x2 = location[1].X;
    //    var y2 = texture.height - location[1].Y;

    //    // Calculate angle of barcode
    //    var angle = Mathf.Atan2(y2 - y1, x2 - x1) * Mathf.Rad2Deg;

    //    // Calculate rotation matrix
    //    var pivot = new Vector2(x1, y1);
    //    var rotationMatrix = Matrix4x4.TRS(-pivot, Quaternion.Euler(0, 0, angle), Vector3.one) *
    //                          Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1)) *
    //                          Matrix4x4.TRS(pivot, Quaternion.identity, Vector3.one);

    //    // Rotate the line points
    //    var p1 = rotationMatrix.MultiplyPoint(new Vector3(x1, y1, 0));
    //    var p2 = rotationMatrix.MultiplyPoint(new Vector3(x2, y2, 0));

    //    // Calculate line parameters
    //    var dx = p2.x - p1.x;
    //    var dy = p2.y - p1.y;
    //    var length = Mathf.Sqrt(dx * dx + dy * dy);
    //    var unitDx = dx / length;
    //    var unitDy = dy / length;

    //    // Calculate average color along the line
    //    var pixelCount = 0;
    //    var totalColor = Color.black;
    //    for (var i = 0; i < length; i++)
    //    {
    //        var x = Mathf.RoundToInt(p1.x + i * unitDx);
    //        var y = Mathf.RoundToInt(p1.y + i * unitDy);

    //        // Get color of pixel at (x, y)
    //        if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
    //        {
    //            totalColor += texture.GetPixel(x, y);
    //            pixelCount++;
    //        }
    //    }

    //    // Calculate average color
    //    if (pixelCount > 0)
    //    {
    //        var averageColor = totalColor / pixelCount;

    //        // Write average color to each pixel along the line
    //        for (var i = 0; i < length; i++)
    //        {
    //            var x = Mathf.RoundToInt(p1.x + i * unitDx);
    //            var y = Mathf.RoundToInt(p1.y + i * unitDy);

    //            // Set color of pixel at (x, y)
    //            if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
    //            {
    //                texture.SetPixel(x, y, averageColor);
    //            }
    //        }

    //        // Apply the changes to the texture
    //        texture.Apply();
    //    }
    //}

    //private Color CompareColors(Color targetColor, List<Color> colors)
    //{
    //    Color closestColor = Color.white;
    //    float closestDistance = float.MaxValue;
    //    Color.RGBToHSV(targetColor, out float targetHue, out _, out _);
    //    foreach (Color color in colors)
    //    {
    //        Color.RGBToHSV(color, out float hue, out _, out _);
    //        float distance = Mathf.Abs(hue - targetHue);
    //        if (distance > 0.5f)
    //        {
    //            distance = 1f - distance;
    //        }
    //        if (distance < closestDistance)
    //        {
    //            closestDistance = distance;
    //            closestColor = color;
    //        }
    //    }
    //    return closestColor;
    //}

    public void ToggleScanning() {
        scanningEnabled = !scanningEnabled;
    }

    public string GetCurrentState() {
        return "Is Scanner running? - " + scanningEnabled;
    }
}
