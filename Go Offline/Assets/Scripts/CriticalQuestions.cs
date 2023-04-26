using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using TMPro;

public class CriticalQuestions : MonoBehaviour
{
    private CollectibleManager collectibleManager;
    public static CriticalQuestions instance = null;

    [SerializeField] private GameObject parent;
    [SerializeField] private string[] questionList;
    [SerializeField] private Rectangle[] coloredRectangles;
    [SerializeField] private Disc[] coloredDiscs;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI numberText;

    private float delayTimer = 0;
    private bool delaying = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        collectibleManager = CollectibleManager.instance;
    }

    private void Update()
    {
        if(!delaying) { return; }

        if(delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0)
            {
                Show();
                delaying = false;
            }
        }
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

    public void ShowQuestion(int questionIndex, float delay)
    {
        questionText.text = questionList[questionIndex];
        numberText.text = questionIndex.ToString();

        RecolorHighlights(collectibleManager.swapfietsColors[questionIndex]);

        delayTimer = delay;
        delaying = true;
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
