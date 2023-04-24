using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Shapes;
using System.Linq;

public class CollectionManager : MonoBehaviour
{
    [SerializeField] private RectTransform contentArea;
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private float collectibleWidth;
    [SerializeField] private float collectibleWidthPadding;
    [SerializeField] private float collectibleHeight;
    [SerializeField] private float collectibleHeightPadding;
    private List<CollectibleUnit> sortedUnits = new List<CollectibleUnit>();
    private List<GameObject> instantiatedUnits = new List<GameObject>();
    public sortingOption selectedSortingMethod;
    private bool sortingMenuOpen = false;
    private bool isAnimating = false;

    [Header("Open Sort Anim Settings")]
    [SerializeField] private AnimationCurve openSortingCurve;
    [SerializeField] private float openSortingDuration;
    private float openTimer;
    
    [Header("Close Sort Anim Settings")]
    [SerializeField] private AnimationCurve closeSortingCurve;
    [SerializeField] private float closeSortingDuration;
    private float closeTimer;

    [Header("Change Sorting Method Anim Settings")]
    [SerializeField] private AnimationCurve changeSortingCurve;
    [SerializeField] private float changeSortingDuration;
    [SerializeField] private float closeSortMenuDelay;
    private float changeSortingTimer;
    private sortingOption newChangeToSorting;
    [SerializeField] private RectTransform buttonBarcodeSection;
    [SerializeField] private float barcodeOutHeight;
    [SerializeField] private float barcodeInHeight;
    [SerializeField] private RectTransform shapesMask;
    [SerializeField] private float shapesMaskBotOutPos;
    [SerializeField] private float shapesMaskBotInPos;
    [SerializeField] private RectTransform sortingButton;
    [SerializeField] private float sortingButtonOutPos;
    [SerializeField] private float sortingButtonInPos;
    [SerializeField] private RectTransform[] directionArrows;
    [SerializeField] private RectTransform[] underlines;
    [SerializeField] private float underlinesOutLeft;
    [SerializeField] private float underlinesInLeft;
    [SerializeField] private RectTransform closeButton;
    [SerializeField] private Button closeSortingMenuButton;

    private float barcodeTopStart;
    private float shapesMaskBotStart;
    private float sortingButtonYPosStart;
    private float[] underlinesLeftStart = new float[5];

    private CollectibleManager collectibleManager;

    public enum sortingOption
    {
        timeDown,
        timeUp,
        nameDown,
        nameUp,
        levelDown,
        levelUp,
        rarityDown,
        rarityUp,
        colourDown,
        colourUp,
    }

    public void Init()
    {
        collectibleManager = CollectibleManager.instance;
        sortingMenuOpen = false;
        isAnimating = false;
        Display(selectedSortingMethod);
        closeSortingMenuButton.gameObject.SetActive(false);
    }

    public void Display(sortingOption sortingMethod)
    {
        //Remove all previous instantiated units
        foreach(GameObject go in instantiatedUnits)
        {
            Destroy(go);
        }
        instantiatedUnits.Clear();


        //Set rect size based on number of collectibles -> We display 3 collectibles per row, total height per row = collectibleVisualHeight + set padding.
        contentArea.sizeDelta = new Vector2(contentArea.sizeDelta.x, 2 * collectibleHeightPadding + Mathf.Ceil(collectibleManager.collectedUnits.Count / 3) * (collectibleHeight + 2 * collectibleHeightPadding));

        //Sort collectibles into list based on sorting option
        selectedSortingMethod = sortingMethod;

        SortList(sortingMethod);

        //Instantiate prefabs and load with correct collectible
        int rowIndex = 0;
        int columnIndex = 0;

        for (int i = 0; i < sortedUnits.Count; i++)
        {
            GameObject newCollectibleVisual = Instantiate(collectiblePrefab, contentArea);

            //Set correct position
            RectTransform cvRect = newCollectibleVisual.GetComponent<RectTransform>();
            cvRect.pivot = new Vector2(0, 1);
            cvRect.anchorMin = new Vector2(0, 1);
            cvRect.anchorMax = new Vector2(0, 1);
            float positionX = 70f + ((float)columnIndex * (collectibleWidth + collectibleWidthPadding * 2));
            float positionY = -70f - ((float)rowIndex * (collectibleHeight + collectibleHeightPadding * 2));
            cvRect.anchoredPosition = new Vector2(positionX, positionY);
            

            //Display the collectible
            CollectibleVisualManager cvM = newCollectibleVisual.GetComponent<CollectibleVisualManager>();
            cvM.DisplayNewCollectibleUnit(sortedUnits[i]);

            //Add to instantiated units list
            instantiatedUnits.Add(newCollectibleVisual);

            columnIndex++;
            columnIndex %= 3;
            if(columnIndex == 0) { rowIndex++; }
        }

    }

    private void SortList(sortingOption sortingMethod)
    {
        //Clear list
        sortedUnits.Clear();

        switch (sortingMethod)
        {
            case sortingOption.timeDown:
                sortedUnits = collectibleManager.collectedUnits.OrderBy(c => c.firstScanDateUtc).ToList();
                break;
            case sortingOption.timeUp:
                sortedUnits = collectibleManager.collectedUnits.OrderByDescending(c => c.firstScanDateUtc).ToList();
                break;
            case sortingOption.nameDown:
                sortedUnits = collectibleManager.collectedUnits.OrderBy(c => c.collectibleName).ToList();
                break;
            case sortingOption.nameUp:
                sortedUnits = collectibleManager.collectedUnits.OrderByDescending(c => c.collectibleName).ToList();
                break;
            case sortingOption.levelDown:
                sortedUnits = collectibleManager.collectedUnits.OrderBy(c => c.currentLevel).ToList();
                break;
            case sortingOption.levelUp:
                sortedUnits = collectibleManager.collectedUnits.OrderByDescending(c => c.currentLevel).ToList();
                break;
            case sortingOption.rarityDown:
                sortedUnits = collectibleManager.collectedUnits.OrderByDescending(c => c.rarity).ToList();
                break;
            case sortingOption.rarityUp:
                sortedUnits = collectibleManager.collectedUnits.OrderBy(c => c.rarity).ToList();
                break;
            case sortingOption.colourDown:
                //might want to interpret first on hue instead of just relying on RGB values
                sortedUnits = collectibleManager.collectedUnits.OrderBy(c => HueFromColor(c.color)).ToList();
                break;
            case sortingOption.colourUp:
                sortedUnits = collectibleManager.collectedUnits.OrderByDescending(c => HueFromColor(c.color)).ToList();
                break;
            default:
                sortedUnits = collectibleManager.collectedUnits.OrderBy(c => c.firstScanDateUtc).ToList();
                break;
        }
    }

    private float HueFromColor(Color color)
    {
        float hue;
        Color.RGBToHSV(color, out hue, out _, out _);
        return hue;
    }

    private void Update()
    {
        if (!isAnimating) { return; }

        if (openTimer > 0)
        {
            openTimer -= Time.deltaTime;

            float evaluatedTimeValue = openSortingCurve.Evaluate(1 - (openTimer / openSortingDuration));
            //Animate Barcode Area out
            float newBarcodeHeightValue = Mathf.Lerp(barcodeTopStart, barcodeOutHeight, evaluatedTimeValue);
            buttonBarcodeSection.sizeDelta = new Vector2(buttonBarcodeSection.sizeDelta.x, newBarcodeHeightValue);
            //Animate Shapes Mask out
            float newShapesMaskBotValue = Mathf.Lerp(shapesMaskBotStart, shapesMaskBotOutPos, evaluatedTimeValue);
            shapesMask.offsetMin = new Vector2(shapesMask.offsetMin.x, newShapesMaskBotValue);
            //Animate Sorting Button down
            float newSortingButtonPos = Mathf.Lerp(sortingButtonYPosStart, sortingButtonOutPos, evaluatedTimeValue);
            sortingButton.anchoredPosition = new Vector2(sortingButton.anchoredPosition.x, newSortingButtonPos);

            if (openTimer <= 0)
            { 
                sortingMenuOpen = true;
                closeSortingMenuButton.gameObject.SetActive(true);
                isAnimating = false; 
            }
        }
        else if (changeSortingTimer > 0)
        {
            changeSortingTimer -= Time.deltaTime;

            float evaluatedTimeValue = changeSortingCurve.Evaluate(1 - (changeSortingTimer / changeSortingDuration));

            AnimateSortingChangeLine(evaluatedTimeValue);

            if (changeSortingTimer <= 0)
            { 
                EndSortingChangeAnim();
                isAnimating = false;
                ToggleSortMenu();
            }
        }
        else if (closeTimer > 0)
        {
            closeTimer -= Time.deltaTime;

            float evaluatedTimeValue = closeSortingCurve.Evaluate(1 - (closeTimer / closeSortingDuration));

            //Animate Barcode Area back
            float newBarcodeHeightValue = Mathf.Lerp(barcodeTopStart, barcodeInHeight, evaluatedTimeValue);
            buttonBarcodeSection.sizeDelta = new Vector2(buttonBarcodeSection.sizeDelta.x, newBarcodeHeightValue);
            //Animate Shapes Mask in
            float newShapesMaskBotValue = Mathf.Lerp(shapesMaskBotStart, shapesMaskBotInPos, evaluatedTimeValue);
            shapesMask.offsetMin = new Vector2(shapesMask.offsetMin.x, newShapesMaskBotValue);
            //Animate Sorting Button up
            float newSortingButtonPos = Mathf.Lerp(sortingButtonYPosStart, sortingButtonInPos, evaluatedTimeValue);
            sortingButton.anchoredPosition = new Vector2(sortingButton.anchoredPosition.x, newSortingButtonPos);

            if (closeTimer <= 0)
            {
                //enable Close button
                closeButton.gameObject.SetActive(true);
                sortingMenuOpen = false;
                closeSortingMenuButton.gameObject.SetActive(false);
                isAnimating = false; 
            }
        }
        
    }

    public void ToggleSortMenu()
    {
        if (isAnimating) { return; }

        //check if sorting menu is open. either start opening anim or start closing anim
        if (sortingMenuOpen)
        {
            //Start close animation
            barcodeTopStart = buttonBarcodeSection.offsetMax.y;
            shapesMaskBotStart = shapesMask.offsetMin.y;
            sortingButtonYPosStart = sortingButton.anchoredPosition.y;

            closeTimer = closeSortingDuration;

            isAnimating = true;
        }
        else
        {
            //Disable Close Button
            closeButton.gameObject.SetActive(false);
            
            //Display sorting buttons correctly
            //!!!!!!!!!!!!

            //Start open animation
            barcodeTopStart = buttonBarcodeSection.offsetMax.y;
            shapesMaskBotStart = shapesMask.offsetMin.y;
            sortingButtonYPosStart = sortingButton.anchoredPosition.y;

            openTimer = openSortingDuration;

            isAnimating = true;
        }
    }

    public void ToggleTimeSort()
    {
        if (isAnimating) { return; }

        if (selectedSortingMethod == sortingOption.timeDown)
        {
            //Change arrow rotation
            directionArrows[0].localEulerAngles = new Vector3(0, 0, 0);
            Display(sortingOption.timeUp);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else if(selectedSortingMethod == sortingOption.timeUp)
        {
            //Change arrow rotation
            directionArrows[0].localEulerAngles = new Vector3(0, 0, 180);
            Display(sortingOption.timeDown);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else
        {
            newChangeToSorting = sortingOption.timeDown;
            //Start animation
            StartSortingChangeAnim(0);
        }
    }

    public void ToggleNameSort()
    {
        if (isAnimating) { return; }

        if (selectedSortingMethod == sortingOption.nameDown)
        {
            //Change arrow rotation
            directionArrows[1].localEulerAngles = new Vector3(0, 0, 0);
            Display(sortingOption.nameUp);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else if (selectedSortingMethod == sortingOption.nameUp)
        {
            //Change arrow rotation
            directionArrows[1].localEulerAngles = new Vector3(0, 0, 180);
            Display(sortingOption.nameDown);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else
        {
            newChangeToSorting = sortingOption.nameDown;
            //Start animation
            StartSortingChangeAnim(2);
        }
    }

    public void ToggleLevelSort()
    {
        if (isAnimating) { return; }

        if (selectedSortingMethod == sortingOption.levelDown)
        {
            //Change arrow rotation
            directionArrows[2].localEulerAngles = new Vector3(0, 0, 0);
            Display(sortingOption.levelUp);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else if (selectedSortingMethod == sortingOption.levelUp)
        {
            //Change arrow rotation
            directionArrows[2].localEulerAngles = new Vector3(0, 0, 180);
            Display(sortingOption.levelDown);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else
        {
            newChangeToSorting = sortingOption.levelDown;
            //Start animation
            StartSortingChangeAnim(4);
        }
    }
    
    public void ToggleRaritySort()
    {
        if (isAnimating) { return; }

        if (selectedSortingMethod == sortingOption.rarityDown)
        {
            //Change arrow rotation
            directionArrows[3].localEulerAngles = new Vector3(0, 0, 0);
            Display(sortingOption.rarityUp);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else if (selectedSortingMethod == sortingOption.rarityUp)
        {
            //Change arrow rotation
            directionArrows[3].localEulerAngles = new Vector3(0, 0, 180);
            Display(sortingOption.rarityDown);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else
        {
            newChangeToSorting = sortingOption.rarityDown;
            //Start animation
            StartSortingChangeAnim(6);
        }
    }
   
    public void ToggleColourSort()
    {
        if (isAnimating) { return; }

        if (selectedSortingMethod == sortingOption.colourDown)
        {
            //Change arrow rotation
            directionArrows[4].localEulerAngles = new Vector3(0, 0, 0);
            Display(sortingOption.colourUp);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else if (selectedSortingMethod == sortingOption.colourUp)
        {
            //Change arrow rotation
            directionArrows[4].localEulerAngles = new Vector3(0, 0, 180);
            Display(sortingOption.colourDown);
            ToggleSortMenu();
            closeTimer += changeSortingDuration + closeSortMenuDelay;
        }
        else
        {
            newChangeToSorting = sortingOption.colourDown;
            //Start animation
            StartSortingChangeAnim(8);
        }
    }

    private void StartSortingChangeAnim(int buttonIndex)
    {
        //Set startingValuesLeft of all underlines
        for (int i = 0; i < underlines.Length; i++)
        {
            underlinesLeftStart[i] = underlines[i].offsetMin.x;
        }

        for (int i = 0; i < directionArrows.Length; i++)
        {
            directionArrows[i].GetComponent<Image>().enabled = false;
            directionArrows[i].localEulerAngles = new Vector3(0, 0, 180);
        }

        closeTimer += changeSortingDuration + closeSortMenuDelay;
        changeSortingTimer = changeSortingDuration;
        isAnimating = true;
    }

    private void EndSortingChangeAnim()
    {
        int buttonIndex = (int)newChangeToSorting / 2;

        //Set currect direction arrow active
        directionArrows[buttonIndex].GetComponent<Image>().enabled = true;

        Display(newChangeToSorting);
    }

    private void AnimateSortingChangeLine(float evalTime)
    {
        int buttonIndex = (int)newChangeToSorting / 2;
        for (int i = 0; i < underlines.Length; i++)
        {
            float newLeft = Mathf.Lerp(underlinesLeftStart[i], i == buttonIndex ? underlinesOutLeft : underlinesInLeft, evalTime);
            underlines[i].offsetMin = new Vector2(newLeft, underlines[i].offsetMin.y);
        }
    }
}
