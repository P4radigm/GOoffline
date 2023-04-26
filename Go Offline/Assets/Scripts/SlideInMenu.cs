using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SlideInMenu : MonoBehaviour
{
    private RectTransform rectTransform;

    private float scannerPosY;
    private float hiddenPosY;
    private float outPosY;

    private float startPositionY;
    private bool isAnimating = false;
    private UIStateManager uiManager;
    private UIStateManager.InterfaceState newOutState;

    [Header("Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private float marginTop;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private RectTransform titleBackground;
    [SerializeField] private string[] titleTexts;
    [SerializeField] private int[] titleBackgroundWidths;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button sortButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button goButton;
    [SerializeField] private RectTransform sortingMenu;
    [SerializeField] private RectTransform collectionContent;
    [SerializeField] private CollectionManager collectionManager;
    [SerializeField] private RectTransform aboutContent;
    [SerializeField] private RectTransform tutorialContent;
    [SerializeField] private RectTransform privacyContent;
    [SerializeField] private RectTransform settingsContent;
    [SerializeField] private SettingsUI settingsUI;
    [SerializeField] private RectTransform welcomeContent;
    [SerializeField] private WelcomeManager welcomeManager;

    [Header("Slide In Settings")]
    [SerializeField] private AnimationCurve slideInCurve;
    [SerializeField] public float slideInDuration;
    private float slideInTimer;

    [Header("Slide Out Settings")]
    [SerializeField] private AnimationCurve slideOutCurveOne;
    [SerializeField] private AnimationCurve slideOutCurveTwo;
    [SerializeField] private float slideOutDurationOne;
    [SerializeField] private float slideOutDurationTwo;
    [HideInInspector] public float slideOutDuration;
    private float slideOutTimer;

    [Header("Hide Settings")]
    [SerializeField] private AnimationCurve hideCurve;
    [SerializeField] private float hideDuration;
    private float hideTimer;

    [Header("Show Settings")]
    [SerializeField] private AnimationCurve showCurve;
    [SerializeField] private float showDuration;
    private float showTimer;

    private void Start()
    {
        uiManager = UIStateManager.instance;

        rectTransform = GetComponent<RectTransform>();

        float canvasHeight = canvas.GetComponent<RectTransform>().sizeDelta.y;

        rectTransform.sizeDelta = new Vector2(1000, canvasHeight - marginTop);
        outPosY = 0;
        hiddenPosY = 50 - (canvasHeight - marginTop);
        scannerPosY = 150 - (canvasHeight - marginTop);

        rectTransform.anchoredPosition = new Vector2(0, scannerPosY);
        slideOutDuration = slideOutDurationOne + slideOutDurationTwo;
    }

    private void Update()
    {
        if (!isAnimating) { return; }

        if (slideInTimer > 0)
        {
            slideInTimer -= Time.deltaTime;

            float evaluatedTimeValue = slideInCurve.Evaluate(1 - (slideInTimer / slideInDuration));
            float newPositionY = Mathf.Lerp(startPositionY, outPosY, evaluatedTimeValue);

            rectTransform.anchoredPosition = new Vector2(0, newPositionY);

            if (slideInTimer <= 0) { isAnimating = false; uiManager.SetInterfaceState(newOutState); }
        }
        else if (slideOutTimer > slideOutDurationTwo)
        {
            slideOutTimer -= Time.deltaTime;

            float evaluatedTimeValue = slideOutCurveOne.Evaluate(1 - ((slideOutTimer-slideOutDurationTwo) / slideOutDurationOne));
            float newPositionY = Mathf.Lerp(startPositionY, hiddenPosY, evaluatedTimeValue);

            rectTransform.anchoredPosition = new Vector2(0, newPositionY);

            if (slideOutTimer <= slideOutDurationTwo) { title.text = titleTexts[0]; titleBackground.sizeDelta = new Vector2(titleBackgroundWidths[0], 100); }
        }
        else if (slideOutTimer > 0)
        {
            slideOutTimer -= Time.deltaTime;

            float evaluatedTimeValue = slideOutCurveOne.Evaluate(1 - (slideOutTimer / slideOutDurationTwo));
            float newPositionY = Mathf.Lerp(hiddenPosY, scannerPosY, evaluatedTimeValue);

            rectTransform.anchoredPosition = new Vector2(0, newPositionY);

            if (slideOutTimer <= 0) { isAnimating = false; uiManager.SetInterfaceState(UIStateManager.InterfaceState.Scanner); }
        }
        else if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;

            float evaluatedTimeValue = hideCurve.Evaluate(1 - (hideTimer / hideDuration));
            float newPositionY = Mathf.Lerp(startPositionY, hiddenPosY, evaluatedTimeValue);

            rectTransform.anchoredPosition = new Vector2(0, newPositionY);

            if (hideTimer <= 0) { isAnimating = false; }
        }
        else if (showTimer > 0)
        {
            showTimer -= Time.deltaTime;

            float evaluatedTimeValue = showCurve.Evaluate(1 - (showTimer / showDuration));
            float newPositionY = Mathf.Lerp(startPositionY, scannerPosY, evaluatedTimeValue);

            rectTransform.anchoredPosition = new Vector2(0, newPositionY);

            if (showTimer <= 0) { isAnimating = false; }
        }
    }

    public void StartSlideInAnim(UIStateManager.InterfaceState outState)
    {
        //Turn off all buttons and content
        closeButton.gameObject.SetActive(false);
        sortButton.gameObject.SetActive(false);
        sortingMenu.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        goButton.gameObject.SetActive(false);
        collectionContent.gameObject.SetActive(false);
        aboutContent.gameObject.SetActive(false);
        tutorialContent.gameObject.SetActive(false);
        privacyContent.gameObject.SetActive(false);
        settingsContent.gameObject.SetActive(false);
        welcomeContent.gameObject.SetActive(false);

        switch (outState)
        {
            case UIStateManager.InterfaceState.Collection:
                //needed button initiation
                closeButton.gameObject.SetActive(true);
                sortButton.gameObject.SetActive(true);
                sortingMenu.gameObject.SetActive(true);

                //needed content initiation
                collectionContent.gameObject.SetActive(true);
                collectionManager.Init();

                title.text = titleTexts[0];
                titleBackground.sizeDelta = new Vector2(titleBackgroundWidths[0], 100);
                break;
            case UIStateManager.InterfaceState.About:
                //needed button initiation
                closeButton.gameObject.SetActive(true);

                //needed content initiation
                aboutContent.gameObject.SetActive(true);

                title.text = titleTexts[1];
                titleBackground.sizeDelta = new Vector2(titleBackgroundWidths[1], 100);
                break;
            case UIStateManager.InterfaceState.Tutorial:
                //needed button initiation
                closeButton.gameObject.SetActive(true);

                //needed content initiation
                tutorialContent.gameObject.SetActive(true);

                title.text = titleTexts[2];
                titleBackground.sizeDelta = new Vector2(titleBackgroundWidths[2], 100);
                break;
            case UIStateManager.InterfaceState.Privacy:
                //needed button initiation
                closeButton.gameObject.SetActive(true);

                //needed content initiation
                privacyContent.gameObject.SetActive(true);

                title.text = titleTexts[3];
                titleBackground.sizeDelta = new Vector2(titleBackgroundWidths[3], 100);
                break;
            case UIStateManager.InterfaceState.Settings:
                //needed button initiation
                closeButton.gameObject.SetActive(true);

                //needed content initiation
                settingsContent.gameObject.SetActive(true);
                settingsUI.Init();

                title.text = titleTexts[4];
                titleBackground.sizeDelta = new Vector2(titleBackgroundWidths[4], 100);
                break;
            case UIStateManager.InterfaceState.Welcome:
                //needed button initiation
                continueButton.gameObject.SetActive(true);

                //needed content initiation
                welcomeContent.gameObject.SetActive(true);
                welcomeManager.Init();

                title.text = titleTexts[5];
                titleBackground.sizeDelta = new Vector2(titleBackgroundWidths[5], 100);
                break;
            default:
                title.text = titleTexts[0];
                titleBackground.sizeDelta = new Vector2(titleBackgroundWidths[0], 100);
                break;
        }

        newOutState = outState;

        ReadyAnAnimation();
        slideInTimer = slideInDuration;
    }

    public void StartSlideOutAnim()
    {
        ReadyAnAnimation();
        slideOutTimer = slideOutDuration;
    }

    public void StartHideAnim()
    {
        ReadyAnAnimation();
        hideTimer = hideDuration;
    }

    public void StartShowAnim()
    {
        ReadyAnAnimation();
        showTimer = showDuration;
    }

    private void ReadyAnAnimation()
    {
        slideInTimer = 0;
        slideOutTimer = 0;
        hideTimer = 0;

        isAnimating = true;
        startPositionY = rectTransform.anchoredPosition.y;
        uiManager.SetInterfaceState(UIStateManager.InterfaceState.Animating);
    }
}
