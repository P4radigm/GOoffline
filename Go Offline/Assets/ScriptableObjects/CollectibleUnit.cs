using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollectibleUnit
{
    public string originString;
    public Color color;
    public string collectibleName;
    public int font;
    public int rarity;
    public int startLevel;
    public int currentLevel;
    public List<Vector2> lineCoords;
    public float lineWidth;
    public int pupilShape;
    public float pupilSize;
    public float eyePosition;
    public long firstScanDateUtc;
    public long lastScanDateUtc;
}
