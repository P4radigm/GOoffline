using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;
using UnityEditor;

public class CollectibleManager : MonoBehaviour
{
    [Serializable]
    public class FontOptions
    {
        public string name;
        public TMP_FontAsset fontAsset;
        public float relativeFontSize;
    }

    [Header("Collectible Settings")]
    [Space(5)]
    public bool deleteSaveDataOnStart;
    [Space(10)]
    public List<Color> swapfietsColors = new List<Color>();
    public Vector2 minMaxLineWidth;
    public Vector2 minMaxPupilSize;
    public int[] pupilSideOptions;
    public Vector2 minMaxEyePosition;
    public string[] nameOptions;
    [SerializeField] private int maxCharactersForName;
    [SerializeField] private int maxNameAmount; 
    public FontOptions[] fontOptions;
    public string[] rarityOptions;

    [Header("UI")]
    public int[] criticalQuestionMilestone;
    public float criticalQuestionDelay;
    public RectTransform topEdgeTransform;
    public RectTransform botEdgeTransform;

    [Header("In Play")]
    private CollectibleGenerator collectibleGenerator;

    public List<CollectibleUnit> collectedUnits = new List<CollectibleUnit>();

    public static CollectibleManager instance = null;

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
        //Get CollectibleGenerator
        collectibleGenerator = CollectibleGenerator.instance;

        //Possibly delete all JSON files
        if (deleteSaveDataOnStart) { collectibleGenerator.DeleteAllSaveData(); }

        //Readout all JSON files for collectible units
        collectibleGenerator.ReGenerateCollectibles();

        //Load all Names into NameOptions from .txt file
        nameOptions = LoadNameOptions("cleanNames");
        //nameOptions = LoadDirtyNameOptions("names");
    }

    private string[] LoadNameOptions(string filename)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(filename);
        string[] names = new string[maxNameAmount];
        Array.Copy(textAsset.text.Split('\n'), names, maxNameAmount);

        for (int i = 0; i < names.Length; i++)
        {
            names[i] = names[i].TrimEnd('\r');
        }

        return names;
    }

    public int GetPupilShape(int index)
    {
        return pupilSideOptions[index];
    }

    public string GetRarity(int index)
    {
        return rarityOptions[index];
    }

    public TMP_FontAsset GetFontAsset(int fontIndex)
    {
        return fontOptions[fontIndex].fontAsset;
    }

    public float GetFontSize(int fontIndex)
    {
        return fontOptions[fontIndex].relativeFontSize;
    }

    public string GetName(int nameIndex)
    {
        if (nameOptions != null && nameIndex >= 0 && nameIndex < nameOptions.Length)
        {
            return nameOptions[nameIndex];
        }
        else
        {
            Debug.LogWarning("Invalid nameIndex or name list not loaded.");
            return "Errorica";
        }
    }

    public static long DateTimeToLong(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().Ticks - DateTime.UnixEpoch.Ticks;
    }

    public static DateTime LongToDateTime(long longValue)
    {
        return new DateTime(DateTime.UnixEpoch.Ticks + longValue, DateTimeKind.Utc);
    }

    #region Interpret name data
    private string[] LoadDirtyNameOptions(string filename)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(filename);

        if (textAsset != null)
        {
            string[] dirtyNames = textAsset.text.Split('.');
            List<string> cleanNames = new List<string>();
            for (int i = 0; i < dirtyNames.Length; i++)
            {
                if (dirtyNames[i] == "") { continue; }
                if (dirtyNames[i].Contains('\n')) { continue; }
                if (dirtyNames[i].Length > maxCharactersForName) { continue; }
                cleanNames.Add(dirtyNames[i]);
            }

            return cleanNames.ToArray();
        }
        else
        {
            Debug.LogError("Failed to load string list: " + filename);
            return null;
        }
    }

    public void WriteNameArrayToTextFile()
    {
        // Create the folder if it doesn't exist
        if (!Directory.Exists("Assets/Names"))
        {
            Directory.CreateDirectory("Assets/Names");
        }

        // Write the array to a text file
        using (StreamWriter writer = new StreamWriter("Assets/Names/cleanNames.txt"))
        {
            string[] newNames;
            newNames = LoadDirtyNameOptions("names");

            foreach (string str in newNames)
            {
                writer.WriteLine(str);
            }
        }

        Debug.Log("Array written to file.");
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(CollectibleManager))]
    public class ArrayWriterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CollectibleManager writer = (CollectibleManager)target;
            if (GUILayout.Button("Write Name Options to File"))
            {
                writer.WriteNameArrayToTextFile();
            }
        }
    }
    #endif

    #endregion
}
