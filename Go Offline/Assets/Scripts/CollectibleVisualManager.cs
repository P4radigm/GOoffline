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
    [SerializeField] private float maxPupilRadius;
    [Space(5)]
    [SerializeField]  private Disc eyeBorder;
    [SerializeField]  private Disc eyeLid;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI rarityText;
    public TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nameText;
    [Space(10)]
    [SerializeField] private Rectangle cardBackground;
    [SerializeField] private float maxColoredBackgroundAlpha;
    [SerializeField] private Rectangle topBackground;
    [SerializeField] private Rectangle botBackground;
    [SerializeField] private Disc botLeftBackgroundCircle;
    [SerializeField] private Disc botRightBackgroundCircle;
    [SerializeField] private Rectangle rarityBackground;
    [SerializeField] private Rectangle levelBackground;

    [Header("Needed")]
    [SerializeField] private AnimationCurve eyeAnimCurve;
    public CollectibleUnit visibleUnit;
    [HideInInspector] public CollectibleManager collectibleManager;
    private bool isAnimating = false;
    private float eyeTimer;
    private float eyeAnimDuration;
    private bool animateOpen;
    private float eyeAngStart;
    private float eyeAngEnd;
    private float eyeMaskStartYPos;


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

    protected virtual void Update()
    {
        if (!isAnimating) { return; }

        if (eyeTimer > 0)
        {
            eyeTimer -= Time.deltaTime;

            float evaluatedTimeValue = eyeAnimCurve.Evaluate(1 - (eyeTimer / eyeAnimDuration));
            float newAngStart = Mathf.Lerp(eyeAngStart, animateOpen ? (Mathf.PI * 0.5f) : (Mathf.PI * -0.5f), evaluatedTimeValue);
            float newAngEnd = Mathf.Lerp(eyeAngEnd, animateOpen ? (Mathf.PI * 0.5f) : (Mathf.PI * 1.5f), evaluatedTimeValue);

            eyeLid.AngRadiansStart = newAngStart;
            eyeLid.AngRadiansEnd = newAngEnd;

            if (eyeTimer <= 0) { eyeLid.enabled = !animateOpen; isAnimating = false; }
        }
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

        //Set Pupil to random position within eye
        SetPupilToRandomPosition();

        //Set Name
        nameText.text = unitToDisplay.collectibleName;

        //Set Name Font
        nameText.font = collectibleManager.GetFontAsset(unitToDisplay.font);
        nameText.fontSize = collectibleManager.GetFontSize(unitToDisplay.font);

        //Set Current Level
        levelText.text = unitToDisplay.currentLevel.ToString();

        //Set Rarity
        rarityText.text = collectibleManager.GetRarity(unitToDisplay.rarity);

        visibleUnit = unitToDisplay;
    }

    private void Recolor(Color newColor)
    {
        Color oldColorBackgroundTop = topBackground.Color;
        Color oldColorBackgroundBot = botBackground.Color;
        bodyLine.Color = newColor;
        eyeBorder.Color = newColor;
        eyeLid.Color = newColor;
        rarityBackground.Color = newColor;
        levelBackground.Color = newColor;
        topBackground.Color = new Color(newColor.r, newColor.g, newColor.b, oldColorBackgroundTop.a);
        botBackground.Color = new Color(newColor.r, newColor.g, newColor.b, oldColorBackgroundBot.a);
        botLeftBackgroundCircle.Color = new Color(newColor.r, newColor.g, newColor.b, oldColorBackgroundBot.a);
        botRightBackgroundCircle.Color = new Color(newColor.r, newColor.g, newColor.b, oldColorBackgroundBot.a);

        nameText.color = newColor;
    }

    public void SetCardAlpha(float newAlpha)
    {
        cardBackground.Color = new Color(cardBackground.Color.r, cardBackground.Color.g, cardBackground.Color.b, newAlpha);
        rarityBackground.Color = new Color(rarityBackground.Color.r, rarityBackground.Color.g, rarityBackground.Color.b, newAlpha);
        levelBackground.Color = new Color(levelBackground.Color.r, levelBackground.Color.g, levelBackground.Color.b, newAlpha);

        topBackground.Color = new Color(topBackground.Color.r, topBackground.Color.g, topBackground.Color.b, newAlpha * maxColoredBackgroundAlpha);
        botBackground.Color = new Color(botBackground.Color.r, botBackground.Color.g, botBackground.Color.b, newAlpha * maxColoredBackgroundAlpha);
        botLeftBackgroundCircle.Color = new Color(botLeftBackgroundCircle.Color.r, botLeftBackgroundCircle.Color.g, botLeftBackgroundCircle.Color.b, newAlpha * maxColoredBackgroundAlpha);
        botRightBackgroundCircle.Color = new Color(botRightBackgroundCircle.Color.r, botRightBackgroundCircle.Color.g, botRightBackgroundCircle.Color.b, newAlpha * maxColoredBackgroundAlpha);

        nameText.color = new Color(nameText.color.r, nameText.color.g, nameText.color.b, newAlpha);
        rarityText.color = new Color(rarityText.color.r, rarityText.color.g, rarityText.color.b, newAlpha);
        levelText.color = new Color(levelText.color.r, levelText.color.g, levelText.color.b, newAlpha);
    }

    public void AnimateEyeLid(bool toOpen, float animationTime)
    {
        Debug.Log($"Animating Eye Lid = toOpen = {toOpen}");
        eyeAngStart = eyeLid.AngRadiansStart;
        eyeAngEnd = eyeLid.AngRadiansEnd;

        eyeLid.enabled = true;
        animateOpen = toOpen;
        eyeAnimDuration = animationTime;
        eyeTimer = animationTime;
        isAnimating = true;
    }

    public void SetPupilToRandomPosition()
    {
        // Generate a random point within the circle
        float angle = Random.Range(0, Mathf.PI * 2);
        Vector2 randomPoint = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(0, maxPupilRadius);

        // Set the child RectTransform's position to the random point within the circle
        pupil.GetComponent<RectTransform>().anchoredPosition = randomPoint;
    }

    public void SetEyelid(bool isOpen)
    {
        eyeLid.AngRadiansStart = isOpen ? (Mathf.PI * 0f) : (Mathf.PI * -0.5f);
        eyeLid.AngRadiansEnd = isOpen ? (Mathf.PI * 1f) : (Mathf.PI * 1.5f);
        eyeLid.enabled = !isOpen;
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
