using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


public class CollectibleGenerator : MonoBehaviour
{
    private CollectibleManager collectibleManager;
    public GameObject alreadyScannedTodayPopUp;
    public float scannedTodayPopUpTime;
    [SerializeField] private NewCollectibleBehaviour newCollectibleVisual;
    [SerializeField] private LevelUpCollectibleBehaviour levelCollectibleVisual;

    [Header("Options")]
    [Space(10)]
    [SerializeField] private Vector2Int namePositionRange;
    [Space(5)]
    [SerializeField] private Vector2Int fontPositionRange;

    [Space(20)]
    [SerializeField] private Vector2Int colorPositionRange;

    [Space(20)]
    [SerializeField] private Vector2Int rarityPositionRange;
    [Space(5)]
    [SerializeField] private Vector2Int startLevelPositionRange;
    
    [Space(20)]
    [SerializeField] private Vector2Int coordsPositionRange;
    [Space(5)]
    [SerializeField] private Vector2Int lineWidthPositionRange;
    
    [Space(20)]
    [SerializeField] private Vector2Int pupilShapePositionRange;
    [Space(5)]
    [SerializeField] private Vector2Int pupilSizePositionRange;
    [Space(5)]
    [SerializeField] private Vector2Int eyePositionPositionRange;

    public static CollectibleGenerator instance = null;

    private void Awake()
    {
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
        collectibleManager = CollectibleManager.instance;
    }

    //Regenerate all units based on JSON files
    public void ReGenerateCollectibles()
    {
        // search for all JSON files with 8 character name in the specified directory
        string[] files = Directory.GetFiles(Application.persistentDataPath, "????????.json");

        Debug.Log($"Found {files.Length} saved units to regenerate");

        // load each file and add to the units list
        foreach (string file in files)
        {
            Debug.Log($"Trying to regenerate collectible from {file}");
            string jsonString = File.ReadAllText(file);
            CollectibleUnit unit = JsonUtility.FromJson<CollectibleUnit>(jsonString);
            Debug.Log($"Succesfully read unit from {file}");
            collectibleManager.collectedUnits.Add(unit);
            Debug.Log($"Succesfully added unit to collectibles from {file}");
        }
    }

    public void DeleteAllSaveData()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "????????.json");

        Debug.Log($"Found {files.Length} saved units for deletion");

        foreach (string file in files)
        {
            File.Delete(file);
        }

        collectibleManager.collectedUnits.Clear();
        CodeReaderManager.instance.lastResult = "";
    }

    //Generate new scriptableObject and add to inventory list -> Also immediatly add to JSON save file.
    public void ScannedBarcode(string barcodeReadout, float possibleLevelIncrement)
    {
        if(barcodeReadout.Length < 8)
        {
            //Not a swapfiets barcode
            return;
        }

        //Convert barcode into int -> some swapfietsen have some letters in there for some reason, we'll just make this 0
        if (!int.TryParse(ConvertToDigitString(barcodeReadout), out int convertedBarcode))
        {
            Debug.LogError($"barcodeReadout '{barcodeReadout}' was not converted correctly to digit only string -> {convertedBarcode}");
            return;
        }

        //Check if we already scanned this barcode before -> if yes, increment level by amount earned?
        string filePath = Path.Combine(Application.persistentDataPath, $"{convertedBarcode}.json");
        if (File.Exists(filePath))
        {
            CollectibleUnit scannedCollectible = ReturnCollectibleFromJson(filePath);

            if (CollectibleIsScannedToday(scannedCollectible))
            {
                Debug.Log("Barcode was already scanned today, come back tomorrow");
                
                //Need to send this to some UI element
                alreadyScannedTodayPopUp.SetActive(true);
                string popUpText = $"Bike was already scanned today, come back tomorrow to level your {scannedCollectible.collectibleName} up!";
                alreadyScannedTodayPopUp.GetComponent<AlreadyScannedPopUp>().StartPopUp(popUpText, scannedTodayPopUpTime);

                return;
            }

            //Show Level Up to player
            levelCollectibleVisual.StartLevelAnim(scannedCollectible, Mathf.FloorToInt(possibleLevelIncrement));
           
            return;
        }

        //Create scriptable object instance
        CollectibleUnit newCollectible = new()
        {
            //Set params
            originString = convertedBarcode.ToString(),

            color = collectibleManager.swapfietsColors[GetIndexRange(convertedBarcode, colorPositionRange.x, colorPositionRange.y)],

            collectibleName = GetCollectibleName(GetIndexRange(convertedBarcode, namePositionRange.x, namePositionRange.y)),
            font = GetIndexRange(convertedBarcode, fontPositionRange.x, fontPositionRange.y),

            rarity = GetIndexRange(convertedBarcode, rarityPositionRange.x, rarityPositionRange.y),
            startLevel = GetIndexRange(convertedBarcode, startLevelPositionRange.x, startLevelPositionRange.y),

            currentLevel = GetIndexRange(convertedBarcode, startLevelPositionRange.x, startLevelPositionRange.y),

            lineCoords = ReadoutLineCoords(GetIndexRange(convertedBarcode, coordsPositionRange.x, coordsPositionRange.y)),
            lineWidth = InterpolateIndexRange(GetIndexRange(convertedBarcode, lineWidthPositionRange.x, lineWidthPositionRange.y), collectibleManager.minMaxLineWidth.x, collectibleManager.minMaxLineWidth.y),

            pupilShape = GetIndexRange(convertedBarcode, pupilShapePositionRange.x, pupilShapePositionRange.y),
            pupilSize = InterpolateIndexRange(GetIndexRange(convertedBarcode, pupilSizePositionRange.x, pupilSizePositionRange.y), collectibleManager.minMaxPupilSize.x, collectibleManager.minMaxPupilSize.y),
            eyePosition = InterpolateIndexRange(GetIndexRange(convertedBarcode, eyePositionPositionRange.x, eyePositionPositionRange.y), collectibleManager.minMaxEyePosition.x, collectibleManager.minMaxEyePosition.y),

            firstScanDateUtc = CollectibleManager.DateTimeToLong(DateTime.UtcNow),
            lastScanDateUtc = CollectibleManager.DateTimeToLong(DateTime.UtcNow)
        };


        //Add collected unit to JSON library
        SaveCollectibelUnitToJson(newCollectible);

        //add to session collectible list
        collectibleManager.collectedUnits.Add(newCollectible);

        //Show new collectible to player
        newCollectibleVisual.gameObject.SetActive(true);
        newCollectibleVisual.StartDisplayAnim(newCollectible);
    }

    private CollectibleUnit ReturnCollectibleFromJson(string filePath)
    {
        // Load the JSON data from the file
        string jsonData = File.ReadAllText(filePath);

        // Convert the JSON string to a CollectibleUnit object
        CollectibleUnit loadedUnit = JsonUtility.FromJson<CollectibleUnit>(jsonData);
        return loadedUnit;
    }

    public void IncrementCollectibleLevel(CollectibleUnit loadedUnit, int levelIncrement)
    {
        // Increment the currentLevel attribute
        loadedUnit.currentLevel += levelIncrement;
        Mathf.Clamp(loadedUnit.currentLevel, 0, 99);

        //Set last scanned time
        loadedUnit.lastScanDateUtc = CollectibleManager.DateTimeToLong(DateTime.UtcNow);

        // Convert the updated CollectibleUnit object to a JSON string
        string updatedJsonData = JsonUtility.ToJson(loadedUnit);

        // Write the updated JSON string back to the file
        string filePath = Path.Combine(Application.persistentDataPath, $"{loadedUnit.originString}.json");

        File.WriteAllText(filePath, updatedJsonData);
    }

    private void SaveCollectibelUnitToJson(CollectibleUnit inputUnit)
    {
        /*
        Debug.Log($"collectibleUnit.originString = {inputUnit.originString}");
        Debug.Log($"collectibleUnit.color = {inputUnit.color}");
        Debug.Log($"collectibleUnit.collectibleName = {inputUnit.collectibleName}");
        Debug.Log($"collectibleUnit.font = {inputUnit.font}");
        Debug.Log($"collectibleUnit.rarity = {inputUnit.rarity}");
        Debug.Log($"collectibleUnit.startLevel = {inputUnit.startLevel}");
        Debug.Log($"collectibleUnit.currentLevel = {inputUnit.currentLevel}");
        Debug.Log($"collectibleUnit.lineCoords = {inputUnit.lineCoords}");
        Debug.Log($"collectibleUnit.lineWidth = {inputUnit.lineWidth}");
        Debug.Log($"collectibleUnit.pupilShape = {inputUnit.pupilShape}");
        Debug.Log($"collectibleUnit.pupilSize = {inputUnit.pupilSize}");
        Debug.Log($"collectibleUnit.eyePosition = {inputUnit.eyePosition}");
        Debug.Log($"collectibleUnit.firstScanDateUtc = {inputUnit.firstScanDateUtc}");
        Debug.Log($"collectibleUnit.lastScanDateUtc = {inputUnit.lastScanDateUtc}");
        */

        // Serialize CollectibleUnit to JSON string
        string jsonString = JsonUtility.ToJson(inputUnit);

        Debug.Log($"jsonString = {jsonString}");

        // Write JSON string to file
        string filePath = Path.Combine(Application.persistentDataPath, $"{inputUnit.originString}.json");
        File.WriteAllText(filePath, jsonString);

        Debug.Log($"CollectibleUnit saved to file: {filePath}");
    }

    private bool CollectibleIsScannedToday(CollectibleUnit collectibleUnit)
    {
        DateTime lastScanned = CollectibleManager.LongToDateTime(collectibleUnit.lastScanDateUtc);
        return lastScanned.Date == DateTime.UtcNow.Date;
    }

    private string ConvertToDigitString(string input)
    {
        char[] digitString = new char[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            digitString[i] = char.IsDigit(c) ? c : '9';
        }

        string result = new string(digitString);
        return result;
    }

    private int GetIndexRange(int input, int start, int end)
    {
        start -= 1;
        end -= 1;
        int inputLength = input.ToString().Length;
        int extracted = (int)Mathf.Floor((input % (IntPow(10, inputLength - start))) / IntPow(10, inputLength - end - 1));

        //Debug.Log($"Output of index extractor = {extracted}, input: {input}, start: {start}, end: {end}");

        return extracted;
    }

    private float InterpolateIndexRange(int input, float outputMin, float outputMax)
    {
        int inputMax = 0;

        for (int i = 0; i < input.ToString().Length; i++)
        {
            inputMax *= 10;
            inputMax += 9;
        }

        return Mathf.Lerp(outputMin, outputMax, (float)input / (float)inputMax);
    }

    private int IntPow(int number, int power)
    {
        int result = 1;
        for (int i = 0; i < power; i++)
        {
            result *= number;
        }
        return result;
    }

    private string GetCollectibleName(int index)
    {
        //Search list for correct name based on index (0-999)
        return collectibleManager.GetName(index);
    }

    private List<Vector2> ReadoutLineCoords(int input)
    {
        List<Vector2> coordsReversed = new();
        List<Vector2> coordsinOrder = new();
        int inputLength = input.ToString().Length;

        for (int i = 0; i < inputLength; i++)
        {
            int digit = input % 10; // get the last digit
            coordsReversed.Add(new Vector2(inputLength - 1 - i, digit)); // add the digit to end of reversed list
            coordsinOrder.Insert(0, new Vector2(inputLength - 1 - i, digit)); //add to front of ordered list
            input = Mathf.FloorToInt((float)input/10f); // remove the last digit from the number
        }

        List<Vector2> coordsNewOrder = new();

        int indexNumber = 0;
        
        for (int i = 0; i < inputLength; i++)
        {
            indexNumber += Mathf.FloorToInt(coordsReversed[i].y * 19.032001f);
            indexNumber %= coordsinOrder.Count;

            coordsNewOrder.Add(new Vector2(coordsinOrder[indexNumber].x, coordsinOrder[indexNumber].y % inputLength));
            coordsinOrder.RemoveAt(indexNumber);
        }

        return coordsNewOrder;
    }
}
