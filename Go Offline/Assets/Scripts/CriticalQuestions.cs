using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Shapes;
using TMPro;

public class CriticalQuestions : MonoBehaviour
{
    private CollectibleManager collectibleManager;

    [SerializeField] private GameObject parent;
    [SerializeField] private string[] questionList;
    [SerializeField] private Rectangle[] coloredRectangles;
    [SerializeField] private Disc[] coloredDiscs;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI numberText;

    void Start()
    {
        collectibleManager = CollectibleManager.instance;
    }

    public void RecolorHighlights(Color newCol)
    {
        for (int i = 0; i < coloredRectangles.Length; i++)
        {
            coloredRectangles[i].Color = newCol;
        }

        for (int i = 0; i < coloredDiscs.Length; i++)
        {
            coloredDiscs[i].Color = newCol;
        }
    }

    public void ShowQuestion(int questionIndex)
    {
        questionText.text = questionList[questionIndex];
        numberText.text = questionIndex.ToString();

        RecolorHighlights(collectibleManager.swapfietsColors[questionIndex]);

        Show();
    }
    
    public void Show()
    {
        parent.SetActive(true);
    }

    public void Hide()
    {
        parent.SetActive(false);
    }
}
