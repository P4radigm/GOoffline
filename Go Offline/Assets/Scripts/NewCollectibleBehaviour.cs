using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewCollectibleBehaviour : CollectibleVisualManager
{
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float startSize;
    [SerializeField] private float endSize;
    [SerializeField] private Vector2 startPosition;
    private Vector2 endPosition;
    [Header("Timings")]
    [SerializeField] private float showCritterTime;
    [SerializeField] private float showCardTime;
    [SerializeField] private AnimationCurve showCardCurve;
    [SerializeField] private float openEyeTime;
    [SerializeField] private float showOffTime;
    [SerializeField] private float toCollectionTime;
    [SerializeField] private AnimationCurve toCollectionCurve;
    private float animationTimer = 0;
    private RectTransform ownRect;

    private void Start()
    {
        ownRect = gameObject.GetComponent<RectTransform>();
        float canvasHeight = GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.y;
        endPosition = new Vector2(0, (-canvasHeight / 2) - (startSize * ownRect.sizeDelta.y / 2));
        gameObject.SetActive(false);
    }

    public void StartDisplayAnim(CollectibleUnit newCollectible)
    {
        SetEyelid(false);
        DisplayNewCollectibleUnit(newCollectible);
        SetCardAlpha(0f);
        ownRect.localScale = Vector3.one * startSize;
        ownRect.anchoredPosition = startPosition;
        animationTimer = showCritterTime + showCardTime + openEyeTime + showOffTime + toCollectionTime;
    }

    protected override void Update()
    {
        base.Update();

        if(animationTimer > showCardTime + openEyeTime + showOffTime + toCollectionTime)
        {
            //Show Critter
            animationTimer -= Time.deltaTime;
        }
        else if(animationTimer > openEyeTime + showOffTime + toCollectionTime)
        {
            //Animate in Card Details
            animationTimer -= Time.deltaTime;

            float evaluatedTimeValue = showCardCurve.Evaluate(1 - ((animationTimer - openEyeTime - showOffTime - toCollectionTime) / showCardTime));
            float newAlpha = Mathf.Lerp(0f, 1f, evaluatedTimeValue);

            SetCardAlpha(newAlpha);
            //Open Eyelid
            if (animationTimer <= openEyeTime + showOffTime + toCollectionTime) { AnimateEyeLid(true, openEyeTime); }
        }
        else if (animationTimer > showOffTime + toCollectionTime)
        {
            //Wait for open eyelid anim
            animationTimer -= Time.deltaTime;
            
            //Show notification text
            if (animationTimer <= showOffTime + toCollectionTime)
            {
                notificationText.text = $"{visibleUnit.collectibleName} was added to" + '\n' + "your collection!";
                notificationText.enabled = true;
            }
        }
        else if (animationTimer > toCollectionTime)
        {
            //Show Off Time
            animationTimer -= Time.deltaTime;

            //Hide notification text
            if (animationTimer <= toCollectionTime)
            {
                notificationText.enabled = false;
            }
        }
        else if (animationTimer > 0)
        {
            //Animate to scale of menu and move to collection area
            animationTimer -= Time.deltaTime;

            float evaluatedTimeValue = toCollectionCurve.Evaluate(1 - (animationTimer / toCollectionTime));
            float newSize = Mathf.Lerp(startSize, endSize, evaluatedTimeValue);
            Vector2 newPos = Vector2.Lerp(startPosition, endPosition, evaluatedTimeValue);

            ownRect.anchoredPosition = newPos;
            ownRect.localScale = Vector3.one * newSize;

            if (animationTimer <= 0) 
            {
                //Check if we need to throw a critical question
                int throwQuestionIndex = -1;
                for (int i = 0; i < collectibleManager.criticalQuestionMilestone.Length; i++)
                {
                    if(collectibleManager.collectedUnits.Count == collectibleManager.criticalQuestionMilestone[i])
                    {
                        throwQuestionIndex = i;
                    }
                }
                if (throwQuestionIndex != -1) { CriticalQuestions.instance.ShowQuestion(throwQuestionIndex, collectibleManager.criticalQuestionDelay); }

                gameObject.SetActive(false); 
            }
        }
    }
    //Find Animation

    //Show Critter -> Eyelid closed

    //Animate in card details

    //Open Eyelid

    //Display [Name] added to collection text

    //Animate scale to menu size (1) and animate into cellection menu
}
