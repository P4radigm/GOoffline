using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI counter;
    [SerializeField] private float updateInterval = 0.5f;

    private float lastInterval;
    private int frames;

    private void Start()
    {
        counter = GetComponent<TextMeshProUGUI>();
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }

    private void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            float fps = frames / (timeNow - lastInterval);
            counter.text = "FPS: " + fps.ToString("F2");
            frames = 0;
            lastInterval = timeNow;
        }
    }
}
