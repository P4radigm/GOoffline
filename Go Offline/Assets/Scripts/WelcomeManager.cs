using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeManager : MonoBehaviour
{
    [SerializeField] private GameObject pageOneParent;
    [SerializeField] private GameObject pageTwoParent;
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button goButton;
    private UIStateManager uIStateManager;

    public void Init()
    {
        uIStateManager = UIStateManager.instance;

        pageOneParent.SetActive(true);
        pageTwoParent.SetActive(false);
        continueButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
        goButton.gameObject.SetActive(false);
    }

    public void PressContinue()
    {
        contentTransform.anchoredPosition = Vector2.zero;
        pageOneParent.SetActive(false);
        pageTwoParent.SetActive(true);
        continueButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(true);
        goButton.gameObject.SetActive(true);
    }

    public void PressBack()
    {
        contentTransform.anchoredPosition = Vector2.zero;
        pageOneParent.SetActive(true);
        pageTwoParent.SetActive(false);
        continueButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
        goButton.gameObject.SetActive(false);
    }

    public void PressGo()
    {
        uIStateManager.PressGoButton();
    }
}
