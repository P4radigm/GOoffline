using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using UnityEngine.UI;
using TMPro;

public class CollectibleVisualManager : MonoBehaviour
{
    [Header("Own references")]
    [Space(5)]
    [SerializeField] private Polyline bodyLine;
    [Space(5)]
    [SerializeField] private RectTransform eyePivot;
    [SerializeField] private RegularPolygon pupil;
    [Space(5)]
    [SerializeField]  private Disc eyeBorder;
    [Space(10)]
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nameText;
    [Space(10)]
    [SerializeField] private Rectangle topBackground;
    [SerializeField] private Rectangle botBackground;
    [SerializeField] private Disc botLeftBackgroundCircle;
    [SerializeField] private Disc botRightBackgroundCircle;
    [SerializeField] private Rectangle rarityBackground;
    [SerializeField] private Rectangle levelBackground;

    [Header("Needed")]
    public CollectibleUnit visibleUnit;
    private CollectibleManager collectibleManager;

    private void OnEnable()
    {
        collectibleManager = CollectibleManager.instance;
    }

    private void Awake()
    {
        collectibleManager = CollectibleManager.instance;
    }

    private void Start()
    {
        collectibleManager = CollectibleManager.instance;
    }

    public void DisplayNewCollectibleUnit(CollectibleUnit unitToDisplay)
    {
        collectibleManager = CollectibleManager.instance;

        //Set Colors
        Recolor(unitToDisplay.color);

        //Set Body Coords
        SetBodyCoords(unitToDisplay.lineCoords);

        //Set Body Width
        bodyLine.Thickness = unitToDisplay.lineWidth;

        //Set Eye Position
        eyePivot.anchoredPosition = CalculateEyePosition(unitToDisplay.lineCoords, unitToDisplay.eyePosition);

        //Set Pupil Size
        pupil.Radius = unitToDisplay.pupilSize;

        //Set Pupil Shape
        pupil.Sides = collectibleManager.GetPupilShape(unitToDisplay.pupilShape);

        //Set Name
        nameText.text = unitToDisplay.collectibleName;

        //Set Name Font
        nameText.font = collectibleManager.GetFontAsset(unitToDisplay.font);
        nameText.fontSize = collectibleManager.GetFontSize(unitToDisplay.font);

        //Set Current Level
        levelText.text = unitToDisplay.currentLevel.ToString();

        //Set Rarity
        rarityText.text = collectibleManager.GetRarity(unitToDisplay.rarity);
    }

    private void Recolor(Color newColor)
    {
        Color oldColorBackgroundTop = topBackground.Color;
        Color oldColorBackgroundBot = botBackground.Color;
        bodyLine.Color = newColor;
        eyeBorder.Color = newColor;
        rarityBackground.Color = newColor;
        levelBackground.Color = newColor;
        topBackground.Color = new Color(newColor.r, newColor.g, newColor.b, oldColorBackgroundTop.a);
        botBackground.Color = new Color(newColor.r, newColor.g, newColor.b, oldColorBackgroundBot.a);
        botLeftBackgroundCircle.Color = new Color(newColor.r, newColor.g, newColor.b, oldColorBackgroundBot.a);
        botRightBackgroundCircle.Color = new Color(newColor.r, newColor.g, newColor.b, oldColorBackgroundBot.a);

        nameText.color = newColor;
    }

    private void SetBodyCoords(List<Vector2> newCoords)
    {
        if(bodyLine.Count > newCoords.Count)
        {
            bodyLine.points.RemoveRange(newCoords.Count, bodyLine.Count - newCoords.Count);
            bodyLine.meshOutOfDate = true;
        }

        for (int i = 0; i < newCoords.Count; i++)
        {
            if(bodyLine.Count > i)
            {
                bodyLine.SetPointPosition(i, new Vector3(newCoords[i].x, newCoords[i].y, 0));
            }
            else
            {
                bodyLine.AddPoint(new Vector3(newCoords[i].x, newCoords[i].y, 0));
            }
        }
    }

    private Vector2 CalculateEyePosition(List<Vector2> shapeCoords, float positionOnShape)
    {
        // Calculate the total length of the shape
        float totalLength = 0f;
        for (int i = 0; i < shapeCoords.Count; i++)
        {
            Vector2 p1 = shapeCoords[i];
            Vector2 p2 = shapeCoords[(i + 1) % shapeCoords.Count];
            totalLength += Vector2.Distance(p1, p2);
        }

        // Calculate the target length based on positionOnShape
        float targetLength = totalLength * positionOnShape;

        // Find the line segment containing the target length
        float segmentLength = 0f;
        int segmentIndex = -1;
        for (int i = 0; i < shapeCoords.Count; i++)
        {
            Vector2 p1 = shapeCoords[i];
            Vector2 p2 = shapeCoords[(i + 1) % shapeCoords.Count];
            float segmentDistance = Vector2.Distance(p1, p2);
            if (segmentLength + segmentDistance >= targetLength)
            {
                segmentIndex = i;
                break;
            }
            segmentLength += segmentDistance;
        }

        // Calculate the position on the line segment
        Vector2 startPoint = shapeCoords[segmentIndex];
        Vector2 endPoint = shapeCoords[(segmentIndex + 1) % shapeCoords.Count];
        float remainingLength = targetLength - segmentLength;
        float segmentAngle = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x);
        Vector2 offset = new Vector2(remainingLength * Mathf.Cos(segmentAngle), remainingLength * Mathf.Sin(segmentAngle));

        // Calculate the final position of the object and adjust for coord space
        Vector2 position = startPoint + offset;
        Vector2 adjustedPosition = new Vector2(-60 + (position.x * 30), -60 + (position.y * 30));
        return adjustedPosition;
    }
}
