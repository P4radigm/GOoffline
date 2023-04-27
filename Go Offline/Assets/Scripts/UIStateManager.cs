using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class UIStateManager : MonoBehaviour
{

    //[SerializeField] private TextMeshProUGUI stateDebugText;
    [SerializeField] private OnScreenMenu onScreenMenu;
    [SerializeField] private BackgroundFilter backgroundFilter;
    [SerializeField] private SlideInMenu slideInMenu;
    private CodeReaderManager codeReader;
    private SettingsManager settingsManager;

    public InterfaceState activeInterfaceState = InterfaceState.Scanner;

    public enum InterfaceState
    {
        Scanner,
        Menu,
        Welcome,
        Collection,
        //Individual,
        About,
        Tutorial,
        Privacy,
        Settings,
        Animating
    }

    public static UIStateManager instance;

    private void Awake()
    {
        //Initiate Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start()
    {
        codeReader = CodeReaderManager.instance;
        settingsManager = SettingsManager.instance;

        activeInterfaceState = settingsManager.settings.firstTime ? InterfaceState.Welcome : InterfaceState.Scanner;

        if (settingsManager.settings.firstTime) { InitToWelcome(); }
        SetInterfaceState(activeInterfaceState);
        //SetCodeReaderStateText();
    }

    public void SetInterfaceState(InterfaceState newState)
    {
        activeInterfaceState = newState;
        if(newState == InterfaceState.Scanner) 
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            codeReader.SetScanningEnabled(true); 
        }
        else 
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            codeReader.SetScanningEnabled(false); 
        }
    }

    private void InitToWelcome()
    {
        if (activeInterfaceState != InterfaceState.Animating)
        {
            slideInMenu.StartSlideInAnim(InterfaceState.Welcome);
            onScreenMenu.StartHideMenuAnim();
            backgroundFilter.StartAnimateIn(slideInMenu.slideInDuration);
            backgroundFilter.GetComponent<Button>().interactable = false;
        }
    }

    public void PressFilterButton()
    {
        Debug.Log("Background Filter hit");
        if(activeInterfaceState == InterfaceState.Menu)
        {
            slideInMenu.StartShowAnim();
            onScreenMenu.StartCloseMenuAnim();
            backgroundFilter.StartAnimateOut(onScreenMenu.closeMenuDuration);
        }
        else if(activeInterfaceState != InterfaceState.Animating)
        {
            slideInMenu.StartSlideOutAnim();
            onScreenMenu.StartShowMenuAnim();
            backgroundFilter.StartAnimateOut(slideInMenu.slideOutDuration);
        }
    }

    public void PressMenuButton()
    {
        if(activeInterfaceState == InterfaceState.Menu)
        {
            onScreenMenu.StartCloseMenuAnim();
            slideInMenu.StartShowAnim();
            backgroundFilter.StartAnimateOut(onScreenMenu.closeMenuDuration);
        }
        else if(activeInterfaceState == InterfaceState.Scanner)
        {
            onScreenMenu.StartOpenMenuAnim();
            slideInMenu.StartHideAnim();
            backgroundFilter.StartAnimateIn(onScreenMenu.openMenuDuration);
        }
    }

    public void PressAboutButton()
    {
        if (activeInterfaceState != InterfaceState.Animating)
        {
            slideInMenu.StartSlideInAnim(InterfaceState.About);
            onScreenMenu.StartFarHideMenuAnim();
        }
    }

    public void PressTutorialButton()
    {
        if (activeInterfaceState != InterfaceState.Animating)
        {
            slideInMenu.StartSlideInAnim(InterfaceState.Tutorial);
            onScreenMenu.StartFarHideMenuAnim();
        }
    }

    public void PressPrivacyButton()
    {
        if (activeInterfaceState != InterfaceState.Animating)
        {
            slideInMenu.StartSlideInAnim(InterfaceState.Privacy);
            onScreenMenu.StartFarHideMenuAnim();
        }
    }

    public void PressSettingsButton()
    {
        if (activeInterfaceState != InterfaceState.Animating)
        {
            slideInMenu.StartSlideInAnim(InterfaceState.Settings);
            onScreenMenu.StartFarHideMenuAnim();
        }
    }

    public void PressCollectionButton()
    {
        if (activeInterfaceState == InterfaceState.Scanner)
        {
            slideInMenu.StartSlideInAnim(InterfaceState.Collection);
            onScreenMenu.StartHideMenuAnim();
            backgroundFilter.StartAnimateIn(slideInMenu.slideInDuration);
        }
        else if(activeInterfaceState == InterfaceState.Menu)
        {
            slideInMenu.StartShowAnim();
            onScreenMenu.StartCloseMenuAnim();
            backgroundFilter.StartAnimateOut(onScreenMenu.closeMenuDuration);
        }
        else if (activeInterfaceState != InterfaceState.Animating)
        {
            slideInMenu.StartSlideOutAnim();
            onScreenMenu.StartShowMenuAnim();
            backgroundFilter.StartAnimateOut(slideInMenu.slideOutDuration);
        }
    }

    public void PressCloseButton()
    {
        if (activeInterfaceState != InterfaceState.Animating)
        {
            slideInMenu.StartSlideOutAnim();
            onScreenMenu.StartShowMenuAnim();
            backgroundFilter.StartAnimateOut(slideInMenu.slideOutDuration);
        }
    }

    public void PressGoButton()
    {
        if (activeInterfaceState != InterfaceState.Animating)
        {
            settingsManager.ChangeFirstTimeStatus(false);
            slideInMenu.StartSlideOutAnim();
            onScreenMenu.StartShowMenuAnim();
            backgroundFilter.StartAnimateOut(slideInMenu.slideOutDuration);
            backgroundFilter.GetComponent<Button>().interactable = true;
        }
    }

    //public void SetCodeReaderStateText()
    //{
    //    if (!stateDebugText.gameObject.activeInHierarchy) { return; }
    //    stateDebugText.text = codeReader.DebugShowCurrentState();
    //}
}
