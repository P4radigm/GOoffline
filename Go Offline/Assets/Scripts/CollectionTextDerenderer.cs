using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CollectionTextDerenderer : MonoBehaviour
{
    private CollectibleManager collectibleManager;
    private RectTransform topEdge;
    private RectTransform botEdge;
    private RectTransform ownRect;
    private TextMeshProUGUI text;
    private Camera cam;

    private void Start()
    {
        collectibleManager = CollectibleManager.instance;
        ownRect = GetComponent<RectTransform>();
        text = GetComponent<TextMeshProUGUI>();
        topEdge = collectibleManager.topEdgeTransform;
        botEdge = collectibleManager.botEdgeTransform;
        cam = Camera.main;
    }

    private void OnEnable()
    {
        collectibleManager = CollectibleManager.instance;
        ownRect = GetComponent<RectTransform>();
        text = GetComponent<TextMeshProUGUI>();
        topEdge = collectibleManager.topEdgeTransform;
        botEdge = collectibleManager.botEdgeTransform;
        cam = Camera.main;
    }

    void Update()
    {
        Vector3 ownViewportPos = cam.WorldToViewportPoint(ownRect.position);
        Vector3 botViewportPos = cam.WorldToViewportPoint(botEdge.position);
        Vector3 topViewportPos = cam.WorldToViewportPoint(topEdge.position);

        if (ownViewportPos.y > botViewportPos.y && ownViewportPos.y < topViewportPos.y)
        {
            text.enabled = true;
        }
        else
        {
            text.enabled = false;
        }
    }
}
