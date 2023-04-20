using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCollectible")]
public class CollectibleUnit : ScriptableObject
{
    public string originString; //scanned barcode
    public Color color; //color of swapfiets
    public string collectibleName; //assigned name out of 999 options
    [Range(0, 9)] public int font; //assigned font out of 9 options
    [Range(0, 9)] public int rarity; //assigned rarity out of 9 options
    [Range(0, 99)] public int startLevel; //assigned level out of 99 options
    [Range(0, 100)] public int currentLevel; //current level out of 100 -> This can increment if you come across the same barcode twice
    public List<Vector2> lineCoords; //assigned coords that the critter is built out of (List should contain 5 coords in a distinct order)
    [Range(0, 1)] public float lineWidth; //assigned line width interpolate to range 0.2-0.5
    [Range(0, 9)] public int pupilShape; //assigned pupil shape out of 9 options
    [Range(0, 1)] public float pupilSize; //assigned pupil size interpolate to range 0.1-0.3
    [Range(0, 1)] public float eyePosition; //assigned eye position on the polyline defined by the lineCoords. 0-1, where 0 = the start of the line and 1 = the end of the line

    public long firstScanDateUtc; //first scan date
    public long lastScanDateUtc; //last scan date
}
