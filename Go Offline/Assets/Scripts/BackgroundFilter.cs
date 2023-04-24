using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundFilter : MonoBehaviour
{
    [SerializeField] private AnimationCurve animateInCurve;
    private float inDuration;
    private float inTimer;
    [SerializeField] private Color activeColor;
    [SerializeField] private AnimationCurve animateOutCurve;
    private float outDuration;
    private float outTimer;
    [SerializeField] private Color inActiveColor;

    private bool isAnimating = false;
    private Color currentColor;

    private Button button;
    private Image image;

    private void Start()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if (!isAnimating) { return; }

        if (inTimer > 0)
        {
            float evaluatedTimeValue = animateInCurve.Evaluate(1 - (inTimer / inDuration));
            Color newColor = Color.Lerp(currentColor, activeColor, evaluatedTimeValue);

            image.color = newColor;

            inTimer -= Time.deltaTime;
            if (inTimer <= 0) { isAnimating = false; button.interactable = true; }
        }
        else if (outTimer > 0)
        {
            float evaluatedTimeValue = animateOutCurve.Evaluate(1 - (outTimer / outDuration));
            Color newColor = Color.Lerp(currentColor, inActiveColor, evaluatedTimeValue);

            image.color = newColor;

            outTimer -= Time.deltaTime;
            if (outTimer <= 0) { isAnimating = false; image.enabled = false; }
        }
    }

    public void StartAnimateIn(float duration)
    {
        ReadyAnAnimation();
        inDuration = duration;
        inTimer = inDuration;
    }

    public void StartAnimateOut(float duration)
    {
        ReadyAnAnimation();
        outDuration = duration;
        outTimer = outDuration;
    }

    private void ReadyAnAnimation()
    {
        inTimer = 0;
        outTimer = 0;

        button.interactable = false;
        isAnimating = true;
        image.enabled = true;
        currentColor = image.color;
    }
}
