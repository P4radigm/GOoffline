using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelUpCollectibleBehaviour : CollectibleVisualManager
{
    [Header("Settings")]
    private CollectibleGenerator collectibleGenerator;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displaySize;
    [SerializeField] private float collectionSize;
    [SerializeField] private Vector2 displayPosition;
    private Vector2 collectionPosition;

    private RectTransform ownRect;

    private int startLevel;
    private int endLevel;

    private float animationTimer;

    [Header("Timings")]
    [SerializeField] private float fromCollectionTime;
    [SerializeField] private AnimationCurve fromCollectionCurve;

    [SerializeField] private float levelUpTime;
    [SerializeField] private AnimationCurve levelUpCurve;

    [SerializeField] private float showOffTime;

    [SerializeField] private float toCollectionTime;
    [SerializeField] private AnimationCurve toCollectionCurve;

    private void Start()
    {
        collectibleGenerator = CollectibleGenerator.instance;
        ownRect = gameObject.GetComponent<RectTransform>();
        float canvasHeight = GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.y;
        collectionPosition = new Vector2(0, (-canvasHeight / 2) - (collectionSize * ownRect.sizeDelta.y / 2));
        gameObject.SetActive(false);
    }

    public void StartLevelAnim(CollectibleUnit collectibleToLevel, int addedLevels)
    {
        startLevel = collectibleToLevel.currentLevel;
        endLevel = Mathf.Clamp(startLevel + addedLevels, 0, 99);
        SetEyelid(true);
        DisplayNewCollectibleUnit(collectibleToLevel);
        ownRect.localScale = Vector3.one * collectionSize;
        ownRect.anchoredPosition = collectionPosition;
        animationTimer = fromCollectionTime + levelUpTime + showOffTime + toCollectionTime;
    }
    //Animate in from collection area
    //Animate level number
    //Show off time
    //Animate out into collection area

    protected override void Update()
    {
        base.Update();


        if (animationTimer > levelUpTime + showOffTime + toCollectionTime)
        {
            //Animate to scale of display and move to display area
            animationTimer -= Time.deltaTime;

            float evaluatedTimeValue = fromCollectionCurve.Evaluate(1 - ((animationTimer - levelUpTime - showOffTime - toCollectionTime) / fromCollectionTime));
            float newSize = Mathf.Lerp(collectionSize, displaySize, evaluatedTimeValue);
            Vector2 newPos = Vector2.Lerp(collectionPosition, displayPosition, evaluatedTimeValue);

            ownRect.anchoredPosition = newPos;
            ownRect.localScale = Vector3.one * newSize;

        }
        else if (animationTimer > showOffTime + toCollectionTime)
        {
            //Animate display text level up
            animationTimer -= Time.deltaTime;

            float evaluatedTimeValue = fromCollectionCurve.Evaluate(1 - ((animationTimer - showOffTime - toCollectionTime) / fromCollectionTime));
            float interpolatedLevel = Mathf.Lerp((float)startLevel, (float)endLevel, evaluatedTimeValue);

            levelText.text = interpolatedLevel.ToString("F0");

            //Display notification text
            if (animationTimer <= showOffTime + toCollectionTime)
            {
                notificationText.enabled = true;
                if(startLevel != 99)
                {
                    notificationText.text = $"Your {visibleUnit.collectibleName} has" + '\n' + "leveled up!";
                }
                else if(endLevel == 99)
                {
                    notificationText.text = $"Your {visibleUnit.collectibleName} has now" + '\n' + "reached the maximum level!";
                }
                else
                {
                    notificationText.text = $"Your {visibleUnit.collectibleName} has already" + '\n' + "reached the maximum level!";
                }
            }
        }
        else if (animationTimer > toCollectionTime)
        {
            //Show offf -> Do nothing
            animationTimer -= Time.deltaTime;

            //Hide notification text
            if (animationTimer <= toCollectionTime)
            {
                notificationText.enabled = false;
            }
        }
        else if (animationTimer > 0)
        {
            //Animate to scale of collection and move to collection area
            animationTimer -= Time.deltaTime;

            float evaluatedTimeValue = toCollectionCurve.Evaluate(1 - (animationTimer / toCollectionTime));
            float newSize = Mathf.Lerp(displaySize, collectionSize, evaluatedTimeValue);
            Vector2 newPos = Vector2.Lerp(displayPosition, collectionPosition, evaluatedTimeValue);

            ownRect.anchoredPosition = newPos;
            ownRect.localScale = Vector3.one * newSize;

            if (animationTimer <= 0)
            {
                collectibleGenerator.IncrementCollectibleLevel(visibleUnit, endLevel-startLevel);
                gameObject.SetActive(false); 
            }
        }
    }
}
