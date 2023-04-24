using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnScreenMenu : MonoBehaviour
{
    private Vector2 scannerPosition;
    private Vector2 animStartPosition;
    private bool isAnimating = false;
    private RectTransform ownRectTransform;
    private UIStateManager uiManager;

    [Header("Open Menu Settings")]
    [SerializeField] private AnimationCurve openMenuCurve;
    [SerializeField] public float openMenuDuration;
    [SerializeField] private float openEndPosX;
    private float openTimer;

    [Header("Close Menu Settings")]
    [SerializeField] private AnimationCurve closeMenuCurve;
    [SerializeField] public float closeMenuDuration;
    [SerializeField] private float closeEndPosX;
    private float closeTimer;

    [Header("Show Menu Settings")]
    [SerializeField] private AnimationCurve showMenuCurve;
    [SerializeField] private float showMenuDuration;
    [SerializeField] private float showEndPosX;
    private float showTimer;

    [Header("Hide Menu Settings")]
    [SerializeField] private AnimationCurve hideMenuCurve;
    [SerializeField] private float hideMenuDuration;
    [SerializeField] private float hideEndPosX;
    private float hideTimer;

    [Header("Hide Menu Far Settings")]
    [SerializeField] private AnimationCurve farHideMenuCurve;
    [SerializeField] private float farHideMenuDuration;
    [SerializeField] private float farHideEndPosX;
    private float farHideTimer;

    private void Start()
    {
        uiManager = UIStateManager.instance;
        ownRectTransform = gameObject.GetComponent<RectTransform>();
        scannerPosition = ownRectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (!isAnimating) { return; }

        if(openTimer > 0)
        {
            openTimer -= Time.deltaTime;

            float evaluatedTimeValue = openMenuCurve.Evaluate(1 - (openTimer / openMenuDuration));
            float newPositionX = Mathf.Lerp(animStartPosition.x, openEndPosX, evaluatedTimeValue);

            ownRectTransform.anchoredPosition = new Vector2(newPositionX, scannerPosition.y);

            if(openTimer <= 0) { isAnimating = false; uiManager.SetInterfaceState(UIStateManager.InterfaceState.Menu); }
        }
        else if (closeTimer > 0)
        {
            closeTimer -= Time.deltaTime;

            float evaluatedTimeValue = closeMenuCurve.Evaluate(1 - (closeTimer / closeMenuDuration));
            float newPositionX = Mathf.Lerp(animStartPosition.x, closeEndPosX, evaluatedTimeValue);

            ownRectTransform.anchoredPosition = new Vector2(newPositionX, scannerPosition.y);

            if (closeTimer <= 0) { isAnimating = false; uiManager.SetInterfaceState(UIStateManager.InterfaceState.Scanner); }
        }
        else if (showTimer > 0)
        {
            showTimer -= Time.deltaTime;
            
            float evaluatedTimeValue = showMenuCurve.Evaluate(1 - (showTimer / showMenuDuration));
            float newPositionX = Mathf.Lerp(animStartPosition.x, showEndPosX, evaluatedTimeValue);

            ownRectTransform.anchoredPosition = new Vector2(newPositionX, scannerPosition.y);

            if (showTimer <= 0) { isAnimating = false; }
        }
        else if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            
            float evaluatedTimeValue = hideMenuCurve.Evaluate(1 - (hideTimer / hideMenuDuration));
            float newPositionX = Mathf.Lerp(animStartPosition.x, hideEndPosX, evaluatedTimeValue);

            ownRectTransform.anchoredPosition = new Vector2(newPositionX, scannerPosition.y);

            if (hideTimer <= 0) { isAnimating = false; }
        }
        else if (farHideTimer > 0)
        {
            farHideTimer -= Time.deltaTime;
            
            float evaluatedTimeValue = farHideMenuCurve.Evaluate(1 - (farHideTimer / farHideMenuDuration));
            float newPositionX = Mathf.Lerp(animStartPosition.x, farHideEndPosX, evaluatedTimeValue);

            ownRectTransform.anchoredPosition = new Vector2(newPositionX, scannerPosition.y);

            if (farHideTimer <= 0) { isAnimating = false; }
        }
    }

    public void StartOpenMenuAnim()
    {
        ReadyAnAnimation();
        openTimer = openMenuDuration;
    }

    public void StartCloseMenuAnim()
    {
        ReadyAnAnimation();
        closeTimer = closeMenuDuration;
    }

    public void StartShowMenuAnim()
    {
        ReadyAnAnimation();
        showTimer = showMenuDuration;
    }

    public void StartHideMenuAnim()
    {
        ReadyAnAnimation();
        hideTimer = hideMenuDuration;
    }

    public void StartFarHideMenuAnim()
    {
        ReadyAnAnimation();
        farHideTimer = farHideMenuDuration;
    }

    private void ReadyAnAnimation()
    {
        openTimer = 0;
        closeTimer = 0;
        showTimer = 0;
        hideTimer = 0;
        farHideTimer = 0;

        isAnimating = true;
        animStartPosition = ownRectTransform.anchoredPosition;
        uiManager.SetInterfaceState(UIStateManager.InterfaceState.Animating);
    }
}
